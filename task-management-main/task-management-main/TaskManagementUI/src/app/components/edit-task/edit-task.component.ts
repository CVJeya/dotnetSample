import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { TaskService } from '../../services/task.service';
import { TASK_STATUS_OPTIONS, TaskStatus } from '../../models/task.model';
import { LoadingSpinnerComponent } from '../../shared/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-edit-task',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LoadingSpinnerComponent],
  templateUrl: './edit-task.component.html',
  styleUrl: './edit-task.component.scss'
})
export class EditTaskComponent implements OnInit {
  readonly statusOptions = TASK_STATUS_OPTIONS;
  isLoading = false;
  isSubmitting = false;
  private taskId!: number;

  form = this.fb.nonNullable.group({
    taskTitle: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    taskDescription: ['', [Validators.maxLength(2000)]],
    taskDueDate: ['', [Validators.required]],
    taskStatus: ['Pending' as TaskStatus, [Validators.required]],
    taskRemarks: ['', [Validators.maxLength(1000)]],
    lastUpdatedBy: ['system-user', [Validators.required, Validators.maxLength(100)]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly tasksApi: TaskService,
    private readonly toastr: ToastrService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const parsed = idParam ? Number(idParam) : NaN;
    if (!Number.isFinite(parsed)) {
      this.toastr.error('Invalid task id.');
      void this.router.navigate(['/tasks']);
      return;
    }

    this.taskId = parsed;
    this.load();
  }

  private load(): void {
    this.isLoading = true;
    this.tasksApi.getById(this.taskId).subscribe({
      next: (task) => {
        this.form.patchValue({
          taskTitle: task.taskTitle,
          taskDescription: task.taskDescription ?? '',
          taskDueDate: task.taskDueDate.split('T')[0],
          taskStatus: task.taskStatus,
          taskRemarks: task.taskRemarks ?? '',
          lastUpdatedBy: 'system-user'
        });
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastr.error('Task not found or API unavailable.');
        void this.router.navigate(['/tasks']);
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const due = new Date(raw.taskDueDate);

    this.isSubmitting = true;
    this.tasksApi
      .update(this.taskId, {
        taskTitle: raw.taskTitle.trim(),
        taskDescription: raw.taskDescription?.trim() || null,
        taskDueDate: due.toISOString(),
        taskStatus: raw.taskStatus,
        taskRemarks: raw.taskRemarks?.trim() || null,
        lastUpdatedBy: raw.lastUpdatedBy.trim()
      })
      .subscribe({
        next: () => {
          this.toastr.success('Task updated successfully.');
          void this.router.navigate(['/tasks']);
        },
        error: () => {
          this.isSubmitting = false;
          this.toastr.error('Update failed.');
        }
      });
  }
}
