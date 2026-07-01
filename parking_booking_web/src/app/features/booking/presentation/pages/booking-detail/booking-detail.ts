import { Component, inject, OnDestroy, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { BookingHistoryItem } from '../../../../../core/infrastructure/models/api.models';

@Component({ selector: 'app-booking-detail', standalone: true, imports: [FormsModule, RouterLink], templateUrl: './booking-detail.html', styleUrl: './booking-detail.scss' })
export class BookingDetailComponent implements OnDestroy {
  private readonly api = inject(ParkingBookingApiService);
  private readonly bookingId = inject(ActivatedRoute).snapshot.paramMap.get('id') ?? '';
  private countdownTimer: number | undefined;
  private refreshTimer: number | undefined;
  readonly booking = signal<BookingHistoryItem | null>(null);
  readonly qrUrl = signal('');
  readonly remainingSeconds = signal(0);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly voucherMessage = signal('');
  readonly isApplyingVoucher = signal(false);
  readonly isReviewOpen = signal(false);
  readonly isSubmittingReview = signal(false);
  readonly reviewMessage = signal('');
  voucherCode = '';
  rating = 5;
  reviewComment = '';

  constructor() { this.load(); }
  ngOnDestroy(): void { window.clearInterval(this.countdownTimer); window.clearTimeout(this.refreshTimer); }

  load(): void {
    this.isLoading.set(true); this.errorMessage.set('');
    forkJoin({ bookings: this.api.getMyBookings(), qr: this.api.getBookingQr(this.bookingId) }).subscribe({
      next: async ({ bookings, qr }) => {
        const booking = bookings.find(item => item.id === this.bookingId);
        if (!booking) { this.errorMessage.set('Không tìm thấy lượt đặt chỗ này.'); this.isLoading.set(false); return; }
        this.booking.set(booking);
        if (booking.status === 0) {
          try {
            const { toDataURL } = await import('qrcode');
            this.qrUrl.set(await toDataURL(qr.qrToken, { width: 320, margin: 2, errorCorrectionLevel: 'M' }));
          } catch { this.errorMessage.set('Không thể tạo hình QR.'); }
          this.startCountdown(booking.checkInDeadline);
        }
        this.isLoading.set(false);
      },
      error: error => { this.isLoading.set(false); this.errorMessage.set(error.message ?? 'Không thể tải lượt đặt chỗ.'); },
    });
  }
  countdownLabel(): string { const value = this.remainingSeconds(); return `${Math.floor(value / 60).toString().padStart(2, '0')}:${(value % 60).toString().padStart(2, '0')}`; }
  isExpired(): boolean { return this.booking()?.status === 0 && this.remainingSeconds() === 0; }
  statusLabel(): string { return ['Đang giữ chỗ', 'Đã check-in', 'Hoàn thành', 'Đã hủy', 'Không đến'][this.booking()?.status ?? -1] ?? 'Không xác định'; }
  applyVoucher(): void {
    const code = this.voucherCode.trim().toUpperCase();
    if (!code) return;
    this.isApplyingVoucher.set(true); this.voucherMessage.set('');
    this.api.applyVoucher(this.bookingId, code).subscribe({
      next: () => { 
        this.isApplyingVoucher.set(false); 
        this.booking.update(b => b ? { ...b, appliedVoucherCode: code } : null);
      },
      error: error => { this.isApplyingVoucher.set(false); this.voucherMessage.set(error.message ?? 'Không thể áp dụng voucher.'); },
    });
  }
  submitReview(): void {
    this.isSubmittingReview.set(true); this.reviewMessage.set('');
    this.api.createReview({ bookingId: this.bookingId, rating: this.rating, comment: this.reviewComment.trim() || null }).subscribe({
      next: () => { this.isSubmittingReview.set(false); this.isReviewOpen.set(false); this.reviewMessage.set('Cảm ơn bạn đã gửi đánh giá.'); },
      error: error => { this.isSubmittingReview.set(false); this.reviewMessage.set(error.status === 409 ? 'Bạn đã đánh giá chuyến đi này hoặc booking chưa hoàn thành.' : error.message); },
    });
  }

  formatVietnamDateTime(timestamp: string): string {
    return new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit', timeZone: 'Asia/Ho_Chi_Minh' }).format(new Date(timestamp));
  }
  private startCountdown(checkInDeadline: string): void {
    const validDateStr = checkInDeadline.includes('Z') || checkInDeadline.includes('+') ? checkInDeadline : checkInDeadline + 'Z';
    const expiresAt = new Date(validDateStr).getTime();
    const tick = () => {
      const remaining = Math.max(0, Math.ceil((expiresAt - Date.now()) / 1000));
      this.remainingSeconds.set(remaining);
      if (remaining === 0) { window.clearInterval(this.countdownTimer); this.refreshStatus(); }
    };
    tick();
    if (this.remainingSeconds() > 0) this.countdownTimer = window.setInterval(tick, 1000);
  }
  private refreshStatus(): void {
    this.api.getMyBookings().subscribe({
      next: bookings => {
        const latest = bookings.find(item => item.id === this.bookingId);
        if (latest) this.booking.set(latest);
        if (latest?.status === 0) this.refreshTimer = window.setTimeout(() => this.refreshStatus(), 5000);
        else this.qrUrl.set('');
      },
    });
  }
}
