import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';

@Component({
  selector: 'app-user-detail',
  imports: [DatePipe, RouterLink],
  templateUrl: './user-detail.html'
})
export class UserDetail implements OnInit {
  private readonly userService = inject(UserService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly user = signal<User | null>(null);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.userService.getUserById(id).subscribe({
      next: (u) => this.user.set(u),
      error: () => this.error.set('Failed to fetch user')
    });
  }

  deleteUser(): void {
    const user = this.user();
    if (!user) return;
    if (!confirm(`Are you sure you want to delete "${user.name}"?`)) return;

    this.userService.deleteUser(user.id).subscribe({
      next: () => this.router.navigate(['/users']),
      error: () => this.error.set('Failed to delete user')
    });
  }
}
