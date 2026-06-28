import { Injectable, inject } from '@angular/core';
import { AuthRepository } from '../../domain/repositories/auth.repository';

@Injectable({ providedIn: 'root' })
export class RequestOtpUseCase {
  private readonly repository = inject(AuthRepository);
  execute(phoneNumber: string) { return this.repository.requestOtp(phoneNumber); }
}
