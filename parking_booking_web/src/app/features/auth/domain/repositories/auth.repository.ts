import { Observable } from 'rxjs';
import { AuthSession } from '../entities/auth-session';

export abstract class AuthRepository {
  abstract requestOtp(phoneNumber: string): Observable<{ otpSent: boolean }>;
  abstract verifyOtp(phoneNumber: string, otp: string, remember: boolean, fullName?: string): Observable<AuthSession>;
}
