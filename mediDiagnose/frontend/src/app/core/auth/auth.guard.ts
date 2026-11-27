import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const session = auth.session();

  if (!session) {
    return router.createUrlTree(['/login'], {
      queryParams: { returnUrl: state.url },
    });
  }

  const allowedRoles = route.data['roles'] as string[] | undefined;
  if (allowedRoles && !allowedRoles.includes(session.role)) {
    return router.createUrlTree(['/access-denied']);
  }

  return true;
};
