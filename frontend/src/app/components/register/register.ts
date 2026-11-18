import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <h1>Register</h1>

      <div *ngIf="error" class="alert alert-danger">{{ error }}</div>

      <form (ngSubmit)="register()" #regForm="ngForm" class="auth-form">
        <div class="form-group">
          <label for="name">Name</label>
          <input id="name" name="name" [(ngModel)]="name" required #nameModel="ngModel" class="form-control" placeholder="Enter name" />
        </div>
        <div class="form-group">
          <label for="email">Email</label>
          <input id="email" name="email" type="email" [(ngModel)]="email" required email #emailModel="ngModel" class="form-control" placeholder="Enter email" />
        </div>
        <div class="form-group">
          <label for="password">Password</label>
          <input id="password" name="password" type="password" [(ngModel)]="password" required minlength="6" #passModel="ngModel" class="form-control" placeholder="Min 6 characters" />
        </div>
        <div class="form-actions">
          <button type="submit" [disabled]="submitting || regForm.invalid" class="btn-primary">
            {{ submitting ? 'Registering...' : 'Register' }}
          </button>
        </div>
        <p class="auth-link">Already have an account? <a routerLink="/login">Login</a></p>
      </form>
    </div>
  `
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  name = '';
  email = '';
  password = '';
  error: string | null = null;
  submitting = false;

  ngOnInit(): void {
    console.log('[RegisterComponent] Component initialized');
  }

  register(): void {
    console.log('[RegisterComponent] register() called - name:', this.name, 'email:', this.email);
    this.submitting = true;
    this.error = null;
    console.log('[RegisterComponent] submitting = true, error = null');

    console.log('[RegisterComponent] Calling auth.register()...');
    this.auth.register(this.name, this.email, this.password).subscribe({
      next: () => {
        console.log('[RegisterComponent] ✓ register() succeeded - navigating to /products');
        this.router.navigate(['/products']);
      },
      error: (err) => {
        console.log('[RegisterComponent] ✗ register() failed - error:', err);
        this.error = err.error?.message || 'Registration failed';
        this.submitting = false;
        console.log('[RegisterComponent] submitting = false, error displayed:', this.error);
      }
    });
  }
}
