import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/products', pathMatch: 'full' },

  {
    path: 'login',
    loadComponent: () =>
      import('./components/login/login').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./components/register/register').then(m => m.RegisterComponent),
  },

  {
    path: 'products',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/product-list/product-list').then(m => m.ProductListComponent),
  },
  {
    path: 'products/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/product-form/product-form').then(m => m.ProductFormComponent),
  },
  {
    path: 'products/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/product-form/product-form').then(m => m.ProductFormComponent),
  },

  // The API now requires a login for user endpoints, so these are guarded too
  {
    path: 'users',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/user-list/user-list').then(m => m.UserList),
  },
  {
    path: 'users/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/user-form/user-form').then(m => m.UserForm),
  },
  {
    path: 'users/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/user-form/user-form').then(m => m.UserForm),
  },
  {
    path: 'users/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/user-detail/user-detail').then(m => m.UserDetail),
  },
];
