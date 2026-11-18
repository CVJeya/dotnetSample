import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-user-form',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './user-form.html',
  styleUrl: './user-form.css'
})
export class UserForm implements OnInit {
  private readonly userService = inject(UserService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  name = '';
  email = '';
  isEditMode = false;
  userId: number | null = null;
  loading = false;
  saving = false;
  error: string | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.userId = Number(id);
      this.loadUser(this.userId);
    }
  }

  private loadUser(id: number): void {
    this.loading = true;
    this.userService.getUserById(id).subscribe({
      next: (user) => {
        this.name = user.name;
        this.email = user.email;
        this.loading = false;
      },
      error: (err: unknown) => {
        this.error = 'Failed to load user';
        this.loading = false;
        console.error(err);
      }
    });
  }

  submit(): void {
    if (!this.name.trim() || !this.email.trim()) {
      this.error = 'Name and email are required';
      return;
    }

    this.saving = true;
    this.error = null;

    const request = { name: this.name.trim(), email: this.email.trim() };

    if (this.isEditMode && this.userId) {
      this.userService.updateUser(this.userId, request).subscribe({
        next: () => {
          this.router.navigate(['/']);
        },
        error: (err: unknown) => {
          this.error = 'Failed to update user';
          this.saving = false;
          console.error(err);
        }
      });
    } else {
      this.userService.createUser(request).subscribe({
        next: () => {
          this.router.navigate(['/']);
        },
        error: (err: unknown) => {
          this.error = 'Failed to create user';
          this.saving = false;
          console.error(err);
        }
      });
    }
  }
}
