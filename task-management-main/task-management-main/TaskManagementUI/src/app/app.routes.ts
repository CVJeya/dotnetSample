import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  {
    path: 'tasks',
    loadComponent: () =>
      import('./components/task-list/task-list.component').then((m) => m.TaskListComponent)
  },
  {
    path: 'tasks/create',
    loadComponent: () =>
      import('./components/add-task/add-task.component').then((m) => m.AddTaskComponent)
  },
  {
    path: 'tasks/edit/:id',
    loadComponent: () =>
      import('./components/edit-task/edit-task.component').then((m) => m.EditTaskComponent)
  },
  {
    path: 'tasks/details/:id',
    loadComponent: () =>
      import('./components/task-details/task-details.component').then((m) => m.TaskDetailsComponent)
  },
  { path: '**', redirectTo: 'tasks' }
];
