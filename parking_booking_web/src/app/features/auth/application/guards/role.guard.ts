import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthSessionService } from '../../data/storage/auth-session.service';

export const roleGuard: CanActivateFn = route => {
  const sessions = inject(AuthSessionService);
  const router = inject(Router);
  const session = sessions.session();
  const expectedRole = route.data['role'] as number;

  if (!session) return router.createUrlTree(['/auth/login']);
  if (session.role === expectedRole) return true;

  return router.createUrlTree([homeForRole(session.role)]);
};

export function homeForRole(role: number): string {
  if (role === 3) return '/admin';
  if (role === 2) return '/staff';
  if (role === 1) return '/owner';
  return '/map';
}
