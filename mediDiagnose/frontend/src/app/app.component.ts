import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ApiNoticeService } from './core/api/api-notice.service';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class App {
  protected readonly auth = inject(AuthService);
  protected readonly apiNotice = inject(ApiNoticeService);
  private readonly router = inject(Router);

  protected signOut(): void {
    this.auth.logout();
    void this.router.navigate(['/doctors']);
  }
}
