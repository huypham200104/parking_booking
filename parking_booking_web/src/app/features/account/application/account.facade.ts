import { Injectable, inject } from '@angular/core';
import { AuthSessionService } from '../../auth/data/storage/auth-session.service';
import { AccountRepository } from '../domain/repositories/account.repository';

@Injectable({ providedIn: 'root' })
export class AccountFacade {
  private readonly repository = inject(AccountRepository);
  private readonly sessions = inject(AuthSessionService);
  readonly isAuthenticated = this.sessions.isAuthenticated;
  getCurrentUser() { return this.repository.getCurrentUser(); }
  logout(): void { this.sessions.clear(); }
}
