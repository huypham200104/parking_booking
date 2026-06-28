import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, Observable } from 'rxjs';
import { ApiError } from '../../../../../core/infrastructure/http/api-client.service';
import { RequestOtpUseCase } from '../../../application/use-cases/request-otp.use-case';
import { VerifyOtpUseCase } from '../../../application/use-cases/verify-otp.use-case';

type DemoLoginRole = 'customer' | 'owner' | 'admin';

interface DemoLoginAccount {
  role: DemoLoginRole;
  label: string;
  phoneNumber: string;
  description: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent {
  private readonly requestOtp = inject(RequestOtpUseCase);
  private readonly verifyOtp = inject(VerifyOtpUseCase);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly demoAccounts: readonly DemoLoginAccount[] = [
    {
      role: 'customer',
      label: 'Khách hàng',
      phoneNumber: '0933000001',
      description: 'Đặt chỗ và xem lịch sử giữ xe',
    },
    {
      role: 'owner',
      label: 'Chủ bãi',
      phoneNumber: '0911000001',
      description: 'Quản lý bãi đỗ và lượt đặt',
    },
    {
      role: 'admin',
      label: 'Quản trị viên',
      phoneNumber: '0900000000',
      description: 'Theo dõi hệ thống và người dùng',
    },
  ];

  readonly phoneNumber = signal('');
  readonly selectedRole = signal<DemoLoginRole | null>(null);
  readonly rememberMe = signal(true);
  readonly otp = signal('');
  readonly awaitingOtp = signal(false);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');
  readonly helperMessage = signal('');

  quickFill(account: DemoLoginAccount): void {
    this.phoneNumber.set(account.phoneNumber);
    this.selectedRole.set(account.role);
    this.errorMessage.set('');
    this.helperMessage.set(`Đã điền tài khoản ${account.label}. Bạn chỉ cần bấm Đăng nhập.`);
  }

  editPhoneNumber(): void {
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
    const request: Observable<unknown> = this.awaitingOtp()
      ? this.verifyOtp.execute(this.phoneNumber(), this.otp(), this.rememberMe())
      : this.requestOtp.execute(this.phoneNumber());

    request.pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
      next: () => {
        if (!this.awaitingOtp()) {
          this.awaitingOtp.set(true);
          this.helperMessage.set('OTP đã được gửi. Môi trường phát triển cho phép mã bất kỳ từ 4–6 chữ số.');
          return;
        }
        const requestedUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        const destination = requestedUrl?.startsWith('/') && !requestedUrl.startsWith('//')
          ? requestedUrl
          : '/map';
        void this.router.navigateByUrl(destination);
      },
      error: (error: ApiError) => this.errorMessage.set(error.message),
    });
  }
}
