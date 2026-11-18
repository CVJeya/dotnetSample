import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { TaskService } from '../../services/task.service';
import { TASK_STATUS_OPTIONS, TaskStatus } from '../../models/task.model';

@Component({
  selector: 'app-add-task',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './add-task.component.html',
  styleUrl: './add-task.component.scss'
})
export class AddTaskComponent implements OnInit {
  readonly statusOptions = TASK_STATUS_OPTIONS;
  isSubmitting = false;

  form = this.fb.nonNullable.group({
    taskTitle: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    taskDescription: ['', [Validators.maxLength(2000)]],
    taskDueDate: ['', [Validators.required]],
    taskStatus: ['Pending' as TaskStatus, [Validators.required]],
    taskRemarks: ['', [Validators.maxLength(1000)]],
    createdBy: ['system-user', [Validators.required, Validators.maxLength(100)]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly tasksApi: TaskService,
    private readonly router: Router,
    private readonly toastr: ToastrService
  ) {}

  ngOnInit(): void {}

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const due = new Date(raw.taskDueDate);

    this.isSubmitting = true;
    this.tasksApi
      .create({
        taskTitle: raw.taskTitle.trim(),
        taskDescription: raw.taskDescription?.trim() || null,
        taskDueDate: due.toISOString(),
        taskStatus: raw.taskStatus,
        taskRemarks: raw.taskRemarks?.trim() || null,
        createdBy: raw.createdBy.trim()
      })
      .subscribe({
        next: () => {
          this.toastr.success('Task created successfully.');
          void this.router.navigate(['/tasks']);
        },
        error: () => {
          this.isSubmitting = false;
          this.toastr.error('Unable to create task. Please verify the API is reachable.');
        }
      });
  }

  reset(): void {
    this.form.reset({
      taskTitle: '',
      taskDescription: '',
      taskDueDate: '',
      taskStatus: 'Pending',
      taskRemarks: '',
      createdBy: 'system-user'
    });
  }
}
