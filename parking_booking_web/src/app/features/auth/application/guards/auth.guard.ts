import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthSessionService } from '../../data/storage/auth-session.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const sessions = inject(AuthSessionService);
  const router = inject(Router);

  const hadSession = sessions.session() !== null;
  if (sessions.isSessionValid()) {
    return true;
  }

  return router.createUrlTree(['/auth/login'], {
    queryParams: { returnUrl: state.url, reason: hadSession ? 'session-expired' : 'session-required' },
  });
};
