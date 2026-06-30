import { CommonModule } from '@angular/common';
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, Observable } from 'rxjs';
import { ApiError } from '../../../../../core/infrastructure/http/api-client.service';
import { RequestOtpUseCase } from '../../../application/use-cases/request-otp.use-case';
import { VerifyOtpUseCase } from '../../../application/use-cases/verify-otp.use-case';
import { AuthSessionService } from '../../../data/storage/auth-session.service';

type DemoLoginRole = 'customer' | 'owner' | 'guard' | 'admin';

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
export class LoginComponent implements OnInit, OnDestroy {
  private readonly requestOtp = inject(RequestOtpUseCase);
  private readonly verifyOtp = inject(VerifyOtpUseCase);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly sessions = inject(AuthSessionService);
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
      role: 'guard',
      label: 'Nhân viên bãi xe',
      phoneNumber: '0922000001',
      description: 'Kiểm soát xe vào, ra tại bãi',
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
  readonly lockedMessage = signal('');
  readonly carouselImages = [
    '/assets/carousel/parking-city.png',
    '/assets/carousel/parking-night.png',
    '/assets/carousel/parking-app.png',
  ] as const;
  readonly activeImage = signal(0);
  private carouselTimer: number | undefined;

  ngOnInit(): void {
    if (this.route.snapshot.queryParamMap.get('reason') === 'session-expired') {
      this.helperMessage.set('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại để tiếp tục.');
    } else if (this.route.snapshot.queryParamMap.get('reason') === 'session-required') {
      this.helperMessage.set('Vui lòng đăng nhập để truy cập trang này.');
    }
    this.carouselImages.forEach(source => { const image = new Image(); image.src = source; });
    if (!window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      this.carouselTimer = window.setInterval(() => this.activeImage.update(index => (index + 1) % this.carouselImages.length), 5000);
    }
  }

  ngOnDestroy(): void { window.clearInterval(this.carouselTimer); }

  showImage(index: number): void { this.activeImage.set(index); }

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

  quickFillOtp(): void {
    this.otp.set('123456');
    this.errorMessage.set('');
    this.helperMessage.set('Đã nhập mã OTP dùng cho môi trường phát triển.');
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
        const currentRole = this.sessions.session()?.role;
        let roleHome = '/map';
        if (currentRole === 3) {
          roleHome = '/admin';
        } else if (currentRole === 2) {
          roleHome = '/staff';
        } else if (currentRole === 1) {
          roleHome = '/owner';
        }

        const destination = requestedUrl?.startsWith('/') && !requestedUrl.startsWith('//')
          ? requestedUrl
          : roleHome;
        void this.router.navigateByUrl(destination);
      },
      error: (error: ApiError) => {
        if (this.awaitingOtp() && error.status === 403) {
          this.sessions.clear();
          this.otp.set('');
          this.awaitingOtp.set(false);
          this.lockedMessage.set(error.message || 'Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.');
          return;
        }
        this.errorMessage.set(error.message);
      },
    });
  }
  closeLockedDialog(): void { this.lockedMessage.set(''); }
}
