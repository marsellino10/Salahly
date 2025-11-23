import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { AuthService } from '../services/auth-service';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  const isAuthRequest = req.url.toLowerCase().includes('/auth/login') || req.url.toLowerCase().includes('/auth/refresh');

  const authReq = !token || req.headers.has('Authorization') || isAuthRequest
    ? req
    : req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
        },
      });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthRequest) {
        return throwError(() => error);
      }

      const refreshToken = authService.getRefreshToken();
      if (!refreshToken) {
        authService.logout();
        return throwError(() => error);
      }

      return authService.refresh(refreshToken).pipe(
        switchMap((tokens) => {
          if (!tokens?.token) {
            authService.logout();
            return throwError(() => error);
          }

          const retryReq = authReq.clone({
            setHeaders: {
              Authorization: `Bearer ${tokens.token}`,
            },
          });

          return next(retryReq);
        }),
        catchError((refreshError: HttpErrorResponse) => {
          authService.logout();
          return throwError(() => refreshError);
        })
      );
    })
  );
};
