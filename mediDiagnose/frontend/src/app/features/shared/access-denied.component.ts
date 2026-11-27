import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-access-denied',
  imports: [RouterLink],
  template: `
    <section class="access-denied">
      <span class="eyebrow">ACCESS RESTRICTED</span>
      <h1>This page isn’t available for your account.</h1>
      <p>Sign in with an account that has access, or return to the doctor directory.</p>
      <a class="button button-primary" routerLink="/doctors">Back to doctors</a>
    </section>
  `,
  styles: `
    .access-denied {
      max-width: 560px;
      margin: 70px auto;
      padding: 42px;
      border: 1px solid var(--line);
      border-radius: 18px;
      background: #fff;
      text-align: center;
    }
    .eyebrow {
      color: var(--teal-dark);
      font-size: 11px;
      font-weight: 800;
      letter-spacing: .11em;
    }
    h1 {
      margin: 12px 0 10px;
      color: var(--ink);
      font-size: 27px;
      letter-spacing: -.04em;
    }
    p {
      margin: 0 0 22px;
      color: var(--muted);
      font-size: 14px;
      line-height: 1.6;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccessDeniedComponent {}
