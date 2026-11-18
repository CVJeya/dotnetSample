import { TaskStatusConstants } from '../models/task.model';

/** Maps API status strings to Bootstrap badge classes. */
export function taskStatusBadgeClass(status: string): string {
  switch (status) {
    case TaskStatusConstants.Pending:
      return 'badge text-bg-warning';
    case TaskStatusConstants.InProgress:
      return 'badge text-bg-primary';
    case TaskStatusConstants.Completed:
      return 'badge text-bg-success';
    case TaskStatusConstants.Cancelled:
      return 'badge text-bg-secondary';
    default:
      return 'badge text-bg-light text-dark';
  }
}
