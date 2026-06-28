import { Injectable, inject } from '@angular/core';
import { AuthRepository } from '../../domain/repositories/auth.repository';

@Injectable({ providedIn: 'root' })
export class VerifyOtpUseCase {
  private readonly repository = inject(AuthRepository);
  execute(phoneNumber: string, otp: string, remember: boolean, fullName?: string) {
    return this.repository.verifyOtp(phoneNumber, otp, remember, fullName);
  }
}
