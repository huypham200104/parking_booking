import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthSessionService } from '../../../features/auth/data/storage/auth-session.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const sessions = inject(AuthSessionService);
  const router = inject(Router);
  const session = sessions.session();
  if (!session || !request.url.startsWith('/api')) return next(request);

  const authenticatedRequest = request.clone({
    setHeaders: {
      Authorization: `Bearer ${session.token}`,
      'X-User-Id': session.userId,
    },
  });

  return next(authenticatedRequest).pipe(
    catchError(error => {
      if (error.status === 401 && !request.url.startsWith('/api/auth/')) {
        const returnUrl = router.url.startsWith('/auth/') ? '/' : router.url;
        sessions.clear();
        void router.navigate(['/auth/login'], { queryParams: { returnUrl, reason: 'session-expired' } });
      }

      return throwError(() => error);
    }),
  );
};
