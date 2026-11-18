import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, NgIf],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly auth = inject(AuthService);
  readonly title = 'Angular Product Manager';

  ngOnInit(): void {
    console.log('[App] Root component initialized');
  }

  get isLoggedIn(): boolean {
    const loggedIn = this.auth.isLoggedIn;
    console.log('[App] isLoggedIn getter called → loggedIn:', loggedIn);
    return loggedIn;
  }

  get username(): string {
    const user = this.auth.user?.name ?? '';
    console.log('[App] username getter called → username:', user);
    return user;
  }

  logout(): void {
    console.log('[App] logout() called');
    this.auth.logout();
    console.log('[App] logout() complete ✓');
  }
}
