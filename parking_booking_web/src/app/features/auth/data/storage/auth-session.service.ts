import { Injectable, computed, signal } from '@angular/core';
import { AuthSession } from '../../domain/entities/auth-session';

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private readonly storageKey = 'parkgo.auth';
  private readonly state = signal<AuthSession | null>(this.restore());
  readonly session = this.state.asReadonly();
  readonly isAuthenticated = computed(() => this.state() !== null);

  save(session: AuthSession, persistent: boolean): void {
    this.clearStorage();
    (persistent ? localStorage : sessionStorage).setItem(this.storageKey, JSON.stringify(session));
    this.state.set(session);
  }

  clear(): void { this.clearStorage(); this.state.set(null); }

  private restore(): AuthSession | null {
    const raw = localStorage.getItem(this.storageKey) ?? sessionStorage.getItem(this.storageKey);
    try { return raw ? JSON.parse(raw) as AuthSession : null; } catch { this.clearStorage(); return null; }
  }
  private clearStorage(): void { localStorage.removeItem(this.storageKey); sessionStorage.removeItem(this.storageKey); }
}
