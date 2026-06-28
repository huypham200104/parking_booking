import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, of, switchMap } from 'rxjs';
import { ApiError } from '../../../../../core/infrastructure/http/api-client.service';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { RequestOtpUseCase } from '../../../application/use-cases/request-otp.use-case';
import { VerifyOtpUseCase } from '../../../application/use-cases/verify-otp.use-case';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrls: ['./register.scss'],
})
export class RegisterComponent {
  private readonly requestOtp = inject(RequestOtpUseCase);
  private readonly verifyOtp = inject(VerifyOtpUseCase);
  private readonly api = inject(ParkingBookingApiService);
  private readonly router = inject(Router);
  readonly fullName = signal('');
  readonly phoneNumber = signal('');
  readonly vehiclePlate = signal('');
  readonly acceptTerms = signal(true);
  readonly otp = signal('');
  readonly awaitingOtp = signal(false);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');
  readonly helperMessage = signal('');

  quickFill(): void {
    this.fullName.set('Nguyễn Minh Anh');
    this.phoneNumber.set('0901234567');
    this.vehiclePlate.set('51A-482.10');
    this.acceptTerms.set(true);
    this.errorMessage.set('');
    this.helperMessage.set('Đã điền thông tin mẫu. Bạn có thể bấm Tạo tài khoản.');
  }

  editDetails(): void {
    this.awaitingOtp.set(false);
    this.otp.set('');
    this.errorMessage.set('');
  }

  submit(form: NgForm): void {
    this.errorMessage.set('');

    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    if (!this.awaitingOtp()) {
      this.requestOtp.execute(this.phoneNumber()).pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
        next: () => {
          this.awaitingOtp.set(true);
          this.helperMessage.set('Nhập OTP để tạo tài khoản. Môi trường phát triển cho phép mã bất kỳ từ 4–6 chữ số.');
        },
        error: (error: ApiError) => this.errorMessage.set(error.message),
      });
      return;
    }

    this.verifyOtp.execute(this.phoneNumber(), this.otp(), true, this.fullName()).pipe(
      switchMap(() => this.vehiclePlate().trim()
        ? this.api.createVehicle({ licensePlate: this.vehiclePlate().trim(), vehicleType: 0, isDefault: true })
        : of(null)),
      finalize(() => this.isSubmitting.set(false)),
    ).subscribe({
      next: () => void this.router.navigateByUrl('/map'),
      error: (error: ApiError) => this.errorMessage.set(error.message),
    });
  }
}
