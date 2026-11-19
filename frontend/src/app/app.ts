import { Component, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html'
})
export class App {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoggedIn = signal(this.auth.isLoggedIn);
  readonly username = signal(this.auth.user?.name ?? '');

  constructor() {
    // Re-read the session after every navigation so the navbar updates after login/logout
    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe(() => this.syncAuth());
  }

  logout(): void {
    this.auth.logout();
    this.syncAuth();
  }

  private syncAuth(): void {
    this.isLoggedIn.set(this.auth.isLoggedIn);
    this.username.set(this.auth.user?.name ?? '');
  }
}
