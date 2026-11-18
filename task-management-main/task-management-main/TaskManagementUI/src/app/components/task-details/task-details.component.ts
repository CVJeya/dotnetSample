import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Task } from '../../models/task.model';
import { TaskService } from '../../services/task.service';
import { LoadingSpinnerComponent } from '../../shared/loading-spinner/loading-spinner.component';
import { taskStatusBadgeClass } from '../../helpers/task-status.helpers';

@Component({
  selector: 'app-task-details',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, LoadingSpinnerComponent],
  templateUrl: './task-details.component.html',
  styleUrl: './task-details.component.scss'
})
export class TaskDetailsComponent implements OnInit {
  task: Task | null = null;
  isLoading = false;

  protected readonly badgeClass = taskStatusBadgeClass;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly tasksApi: TaskService,
    private readonly toastr: ToastrService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : NaN;
    if (!Number.isFinite(id)) {
      this.toastr.error('Invalid task id.');
      void this.router.navigate(['/tasks']);
      return;
    }

    this.isLoading = true;
    this.tasksApi.getById(id).subscribe({
      next: (task) => {
        this.task = task;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastr.error('Unable to load task details.');
        void this.router.navigate(['/tasks']);
      }
    });
  }

  delete(): void {
    if (!this.task) {
      return;
    }

    if (!window.confirm(`Delete "${this.task.taskTitle}"?`)) {
      return;
    }

    this.tasksApi.delete(this.task.id).subscribe({
      next: () => {
        this.toastr.success('Task deleted.');
        void this.router.navigate(['/tasks']);
      },
      error: () => this.toastr.error('Delete failed.')
    });
  }
}
