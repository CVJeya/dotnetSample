import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';

@Component({
  selector: 'app-user-list',
  imports: [DatePipe, RouterLink],
  templateUrl: './user-list.html'
})
export class UserList implements OnInit {
  private readonly userService = inject(UserService);

  readonly users = signal<User[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.userService.getUsers().subscribe({
      next: (list) => {
        this.users.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to fetch users');
        this.loading.set(false);
      }
    });
  }

  deleteUser(user: User): void {
    if (!confirm(`Are you sure you want to delete "${user.name}"?`)) return;

    this.userService.deleteUser(user.id).subscribe({
      next: () => this.users.update((list) => list.filter((u) => u.id !== user.id)),
      error: () => this.error.set('Failed to delete user')
    });
  }
}
