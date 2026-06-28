import { Injectable, inject } from '@angular/core';
import { tap } from 'rxjs';
import { ApiClient } from '../../../../core/infrastructure/http/api-client.service';
import { AuthSession } from '../../domain/entities/auth-session';
import { AuthRepository } from '../../domain/repositories/auth.repository';
import { AuthSessionService } from '../storage/auth-session.service';

@Injectable({ providedIn: 'root' })
export class HttpAuthRepository extends AuthRepository {
  private readonly api = inject(ApiClient);
  private readonly sessions = inject(AuthSessionService);

  requestOtp(phoneNumber: string) { return this.api.post<{ otpSent: boolean }>('/auth/login', { phoneNumber }); }
  verifyOtp(phoneNumber: string, otp: string, remember = true, fullName?: string) {
    return this.api.post<AuthSession>('/auth/verify', { phoneNumber, otp, fullName }).pipe(tap(session => this.sessions.save(session, remember)));
  }
}
