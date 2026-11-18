import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <h1>Login</h1>

      <div *ngIf="error" class="alert alert-danger">{{ error }}</div>

      <form (ngSubmit)="login()" #loginForm="ngForm" class="auth-form">
        <div class="form-group">
          <label for="email">Email</label>
          <input id="email" name="email" type="email" [(ngModel)]="email" required #emailModel="ngModel" class="form-control" placeholder="Enter email" />
        </div>
        <div class="form-group">
          <label for="password">Password</label>
          <input id="password" name="password" type="password" [(ngModel)]="password" required class="form-control" placeholder="Enter password" />
        </div>
        <div class="form-actions">
          <button type="submit" [disabled]="submitting || loginForm.invalid" class="btn-primary">
            {{ submitting ? 'Logging in...' : 'Login' }}
          </button>
        </div>
        <p class="auth-link">Don't have an account? <a routerLink="/register">Register</a></p>
      </form>
    </div>
  `
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  error: string | null = null;
  submitting = false;

  ngOnInit(): void {
    console.log('[LoginComponent] Component initialized');
  }

  login(): void {
    console.log('[LoginComponent] login() called - email:', this.email);
    this.submitting = true;
    this.error = null;
    console.log('[LoginComponent] submitting = true, error = null');

    console.log('[LoginComponent] Calling auth.login()...');
    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        console.log('[LoginComponent] ✓ login() succeeded - navigating to /products');
        this.router.navigate(['/products']);
      },
      error: (err) => {
        console.log('[LoginComponent] ✗ login() failed - error:', err);
        this.error = 'Invalid email or password';
        this.submitting = false;
        console.log('[LoginComponent] submitting = false, error displayed');
      }
    });
  }
}
