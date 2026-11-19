import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-user-form',
  imports: [FormsModule, RouterLink],
  templateUrl: './user-form.html'
})
export class UserForm implements OnInit {
  private readonly userService = inject(UserService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly name = signal('');
  readonly email = signal('');
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  isEditMode = false;
  private userId: number | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.isEditMode = true;
    this.userId = Number(id);
    this.loading.set(true);

    this.userService.getUserById(this.userId).subscribe({
      next: (user) => {
        this.name.set(user.name);
        this.email.set(user.email);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load user');
        this.loading.set(false);
      }
    });
  }

  submit(): void {
    const name = this.name().trim();
    const email = this.email().trim();
    if (!name || !email) {
      this.error.set('Name and email are required');
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const request = { name, email };

    const request$: Observable<unknown> = this.isEditMode
      ? this.userService.updateUser(this.userId!, request)
      : this.userService.createUser(request);

    request$.subscribe({
      next: () => this.router.navigate(['/users']),
      error: () => {
        this.error.set(this.isEditMode ? 'Failed to update user' : 'Failed to create user');
        this.saving.set(false);
      }
    });
  }
}
