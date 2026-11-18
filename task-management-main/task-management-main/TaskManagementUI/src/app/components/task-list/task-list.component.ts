import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { Task } from '../../models/task.model';
import { TaskService } from '../../services/task.service';
import { LoadingSpinnerComponent } from '../../shared/loading-spinner/loading-spinner.component';
import { taskStatusBadgeClass } from '../../helpers/task-status.helpers';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, DatePipe, LoadingSpinnerComponent],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss'
})
export class TaskListComponent implements OnInit, OnDestroy {
  tasks: Task[] = [];
  isLoading = false;
  searchTerm = '';
  deletingId: number | null = null;

  protected readonly badgeClass = taskStatusBadgeClass;

  private readonly destroy$ = new Subject<void>();
  private readonly search$ = new Subject<string>();

  constructor(
    private readonly tasksApi: TaskService,
    private readonly toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.search$
      .pipe(
        debounceTime(350),
        distinctUntilChanged(),
        switchMap((keyword) => {
          this.isLoading = true;
          return this.tasksApi.search(keyword);
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (items) => {
          this.tasks = items;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          this.toastr.error('Unable to load tasks. Ensure the API is running.');
        }
      });

    this.search$.next('');
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearchInput(value: string): void {
    this.searchTerm = value;
    this.search$.next(value.trim());
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.search$.next('');
  }

  deleteTask(task: Task): void {
    const confirmed = window.confirm(`Delete "${task.taskTitle}"?`);
    if (!confirmed) {
      return;
    }

    this.deletingId = task.id;
    this.tasksApi.delete(task.id).subscribe({
      next: () => {
        this.tasks = this.tasks.filter((t) => t.id !== task.id);
        this.deletingId = null;
        this.toastr.success('Task deleted.');
      },
      error: () => {
        this.deletingId = null;
        this.toastr.error('Delete failed.');
      }
    });
  }

  trackById(_: number, item: Task): number {
    return item.id;
  }
}
