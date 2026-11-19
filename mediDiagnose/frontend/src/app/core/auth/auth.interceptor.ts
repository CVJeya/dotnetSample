import {
  HttpErrorResponse,
  HttpInterceptorFn,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { ApiNoticeService } from '../api/api-notice.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const token = auth.getAccessToken();
  const router = inject(Router);
  const notices = inject(ApiNoticeService);
  const isAuthRequest = request.url.startsWith('/api/auth/');
  const authorizedRequest =
    token && !isAuthRequest
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;

  return next(authorizedRequest).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && !isAuthRequest) {
        if (error.status === 401) {
          auth.logout();
          notices.show('Your session has expired. Sign in again to continue.');
          void router.navigate(['/login']);
        } else if (error.status === 403) {
          notices.show('You do not have permission to complete that action.');
        }
      }

      return throwError(() => error);
    }),
  );
};
