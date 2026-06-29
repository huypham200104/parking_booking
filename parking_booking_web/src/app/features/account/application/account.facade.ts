import { Injectable, inject, signal } from '@angular/core';
import { AuthSessionService } from '../../auth/data/storage/auth-session.service';
import { AccountRepository } from '../domain/repositories/account.repository';
import { UpdateCurrentUser } from '../domain/entities/current-user';

@Injectable({ providedIn: 'root' })
export class AccountFacade {
  private readonly repository = inject(AccountRepository);
  private readonly sessions = inject(AuthSessionService);
  readonly isAuthenticated = this.sessions.isAuthenticated;
  readonly showLogoutModal = signal(false);

  getCurrentUser() { return this.repository.getCurrentUser(); }
  updateCurrentUser(request: UpdateCurrentUser) { return this.repository.updateCurrentUser(request); }
  logout(): void { 
    this.sessions.clear(); 
    this.showLogoutModal.set(true); 
  }
}
