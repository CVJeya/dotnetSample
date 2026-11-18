import { Injectable, inject } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private readonly auth = inject(AuthService);

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    console.log('[AuthInterceptor] intercept() called for:', req.method, req.url);
    const token = this.auth.token;
    
    if (token) {
      console.log('[AuthInterceptor] ✓ Token found - adding Authorization header');
      const cloned = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
      console.log('[AuthInterceptor] Request cloned with Bearer token');
      return next.handle(cloned);
    }
    
    console.log('[AuthInterceptor] ✗ No token found - proceeding without Authorization header');
    return next.handle(req);
  }
}
