import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthSessionService } from '../../data/storage/auth-session.service';
import { homeForRole } from './role.guard';

export const landingGuard: CanActivateFn = () => {
  const sessions = inject(AuthSessionService);
  const router = inject(Router);
  const session = sessions.session();
  return session ? router.createUrlTree([homeForRole(session.role)]) : true;
};

export const guestOnlyGuard: CanActivateFn = () => {
  const sessions = inject(AuthSessionService);
  const router = inject(Router);
  const session = sessions.session();
  return session ? router.createUrlTree([homeForRole(session.role)]) : true;
};
