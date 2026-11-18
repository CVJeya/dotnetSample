import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard = () => {
  console.log('[AuthGuard] Route guard activated - checking authentication status');
  const auth = inject(AuthService);
  const router = inject(Router);

  const isLoggedIn = auth.isLoggedIn;
  console.log('[AuthGuard] User login status:', isLoggedIn);

  if (isLoggedIn) {
    console.log('[AuthGuard] ✓ User authorized - allowing route access');
    return true;
  }

  console.log('[AuthGuard] ✗ User NOT authorized - redirecting to /login');
  return router.parseUrl('/login');
};
