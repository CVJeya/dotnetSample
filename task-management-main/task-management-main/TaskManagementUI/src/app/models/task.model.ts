export type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled';

export const TaskStatusConstants = {
  Pending: 'Pending' as const,
  InProgress: 'InProgress' as const,
  Completed: 'Completed' as const,
  Cancelled: 'Cancelled' as const
};

export interface Task {
  id: number;
  taskTitle: string;
  taskDescription?: string | null;
  taskDueDate: string;
  taskStatus: TaskStatus;
  taskRemarks?: string | null;
  createdOn: string;
  lastUpdatedOn: string;
  createdBy: string;
  lastUpdatedBy: string;
}

export interface CreateTaskRequest {
  taskTitle: string;
  taskDescription?: string | null;
  taskDueDate: string;
  taskStatus: TaskStatus;
  taskRemarks?: string | null;
  createdBy: string;
}

export interface UpdateTaskRequest {
  taskTitle: string;
  taskDescription?: string | null;
  taskDueDate: string;
  taskStatus: TaskStatus;
  taskRemarks?: string | null;
  lastUpdatedBy: string;
}

export const TASK_STATUS_OPTIONS: { value: TaskStatus; label: string }[] = [
  { value: TaskStatusConstants.Pending, label: 'Pending' },
  { value: TaskStatusConstants.InProgress, label: 'In Progress' },
  { value: TaskStatusConstants.Completed, label: 'Completed' },
  { value: TaskStatusConstants.Cancelled, label: 'Cancelled' }
];
