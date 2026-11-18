import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BehaviorSubject, Observable, switchMap } from 'rxjs';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';

@Component({
  selector: 'app-user-list',
  imports: [CommonModule, RouterLink],
  templateUrl: './user-list.html',
  styleUrl: './user-list.css'
})
export class UserList {
  private readonly userService = inject(UserService);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);

  users$: Observable<User[]> = this.refresh$.pipe(
    switchMap(() => this.userService.getUsers())
  );

  deleteUser(id: number): void {
    if (!confirm('Delete this user?')) return;

    this.userService.deleteUser(id).subscribe({
      next: () => this.refresh$.next(),
      error: (err: unknown) => console.error(err)
    });
  }
}
