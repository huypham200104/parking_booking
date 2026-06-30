import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnDestroy, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { BookingFacade } from '../../../application/booking.facade';
import { BookingFloor, BookingParkingLot, BookingSlot, BookingVehicle, CreatedBooking } from '../../../domain/entities/booking';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { Review } from '../../../../../core/infrastructure/models/api.models';
import { ApiError } from '../../../../../core/infrastructure/http/api-client.service';

@Component({ selector: 'app-create-booking', standalone: true, imports: [CommonModule, RouterLink], templateUrl: './create-booking.html', styleUrl: './create-booking.scss' })
export class CreateBookingComponent implements OnDestroy {
  private readonly facade = inject(BookingFacade);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(ParkingBookingApiService);
  readonly parkingLotId = signal(this.route.snapshot.queryParamMap.get('parkingLotId'));
  readonly parkingLots = signal<BookingParkingLot[]>([]);
  readonly recentParkingLots = signal<BookingParkingLot[]>([]);
  readonly page = signal(1);
  readonly pageSize = 6;
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.parkingLots().length / this.pageSize)));
  readonly pagedParkingLots = computed(() => this.parkingLots().slice((this.page() - 1) * this.pageSize, this.page() * this.pageSize));
  readonly parkingLot = signal<BookingParkingLot | null>(null);
  readonly floors = signal<BookingFloor[]>([]);
  readonly slots = signal<BookingSlot[]>([]);
  readonly vehicles = signal<BookingVehicle[]>([]);
  readonly selectedFloor = signal<BookingFloor | null>(null);
  readonly selectedSlot = signal<BookingSlot | null>(null);
  readonly selectedVehicle = signal<BookingVehicle | null>(null);
  readonly isLoading = signal(false);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');
  readonly createdBooking = signal<CreatedBooking | null>(null);
  readonly bookingQrUrl = signal('');
  readonly isLoadingQr = signal(false);
  readonly qrErrorMessage = signal('');
  readonly remainingSeconds = signal(0);
  readonly isHoldExpired = signal(false);
  readonly lockedMessage = signal('');
  private countdownTimer: number | undefined;
  readonly reviews = signal<Review[]>([]);
  readonly isLoadingReviews = signal(false);
  readonly reviewAverage = computed(() => {
    const items = this.reviews();
    return items.length ? items.reduce((sum, review) => sum + review.rating, 0) / items.length : 0;
  });
  readonly ratingDistribution = computed(() => [5, 4, 3, 2, 1].map(rating => {
    const count = this.reviews().filter(review => review.rating === rating).length;
    return { rating, count, percentage: this.reviews().length ? count / this.reviews().length * 100 : 0 };
  }));

  constructor() { const id = this.parkingLotId(); id ? this.load(id) : this.loadParkingLots(); }
  ngOnDestroy(): void { window.clearInterval(this.countdownTimer); }
  loadParkingLots(): void {
    this.isLoading.set(true); this.errorMessage.set('');
    this.api.getRecentCompletedParkingLots().subscribe({
      next: lots => this.recentParkingLots.set(lots as BookingParkingLot[]),
      error: () => this.recentParkingLots.set([]),
    });
    this.facade.getAllParkingLots().pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: lots => this.parkingLots.set(lots),
      error: error => this.errorMessage.set(error.message ?? 'Không thể tải danh sách bãi đỗ xe.'),
    });
  }
  chooseParkingLot(lot: BookingParkingLot): void {
    this.parkingLotId.set(lot.id);
    void this.router.navigate([], { relativeTo: this.route, queryParams: { parkingLotId: lot.id }, replaceUrl: true });
    this.load(lot.id);
  }
  changeParkingLot(): void {
    this.parkingLotId.set(null); this.parkingLot.set(null); this.selectedFloor.set(null); this.selectedSlot.set(null); this.slots.set([]);
    void this.router.navigate([], { relativeTo: this.route, queryParams: {}, replaceUrl: true });
    if (this.parkingLots().length === 0) this.loadParkingLots();
  }
  goToPage(page: number): void { if (page >= 1 && page <= this.totalPages()) this.page.set(page); }
  load(id: string): void {
    this.isLoading.set(true); this.errorMessage.set(''); this.loadReviews(id);
    this.facade.load(id).pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: data => { this.parkingLot.set(data.parkingLot); this.floors.set(data.floors); this.vehicles.set(data.vehicles); this.selectedVehicle.set(data.vehicles.find(v => v.isDefault) ?? data.vehicles[0] ?? null); if (data.floors[0]) this.selectFloor(data.floors[0]); },
      error: error => this.errorMessage.set(error.message ?? 'Không thể tải thông tin đặt chỗ.'),
    });
  }
  loadReviews(parkingLotId: string): void {
    this.isLoadingReviews.set(true);
    this.api.getParkingLotReviews(parkingLotId).pipe(finalize(() => this.isLoadingReviews.set(false))).subscribe({
      next: reviews => this.reviews.set(reviews),
      error: () => this.reviews.set([]),
    });
  }
  selectFloor(floor: BookingFloor): void { this.selectedFloor.set(floor); this.selectedSlot.set(null); this.facade.getSlots(floor.parkingLotId, floor.id).subscribe({ next: slots => this.slots.set(slots), error: error => this.errorMessage.set(error.message) }); }
  selectSlot(slot: BookingSlot): void { if (slot.status === 0) this.selectedSlot.set(slot); }
  selectVehicle(vehicle: BookingVehicle): void { this.selectedVehicle.set(vehicle); }
  submit(): void {
    const slot = this.selectedSlot(); const vehicle = this.selectedVehicle();
    if (!slot || !vehicle) { this.errorMessage.set('Vui lòng chọn chỗ đỗ và phương tiện.'); return; }
    this.isSubmitting.set(true); this.errorMessage.set('');
    this.facade.create(slot.id, vehicle.id).pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
      next: booking => {
        this.createdBooking.set(booking);
        this.startHoldCountdown(booking.checkInDeadline);
        this.loadBookingQr(booking.id);
      },
      error: (error: ApiError) => {
        if (error.status === 403) {
          this.lockedMessage.set(error.message || 'Tài khoản của bạn đã bị khóa. Không thể tạo Booking mới.');
        } else {
          this.errorMessage.set(error.message ?? 'Không thể tạo đặt chỗ.');
        }
      },
    });
  }
  countdownLabel(): string {
    const seconds = this.remainingSeconds();
    return `${Math.floor(seconds / 60).toString().padStart(2, '0')}:${(seconds % 60).toString().padStart(2, '0')}`;
  }
  closeLockedDialog(): void { this.lockedMessage.set(''); }
  bookingDateTime(value: string): string {
    return new Intl.DateTimeFormat('vi-VN', {
      day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit',
    }).format(new Date(value));
  }
  private startHoldCountdown(checkInDeadline: string): void {
    window.clearInterval(this.countdownTimer);
    const expiresAt = new Date(checkInDeadline).getTime();
    const update = () => {
      const seconds = Math.max(0, Math.ceil((expiresAt - Date.now()) / 1000));
      this.remainingSeconds.set(seconds);
      if (seconds === 0) {
        window.clearInterval(this.countdownTimer);
        this.isHoldExpired.set(true);
        this.refreshExpiredBooking();
      }
    };
    update();
    this.countdownTimer = window.setInterval(update, 1000);
  }
  private refreshExpiredBooking(): void {
    const bookingId = this.createdBooking()?.id;
    if (!bookingId) return;
    this.api.getMyBookings().subscribe({
      next: bookings => {
        const latest = bookings.find(item => item.id === bookingId);
        if (latest && latest.status !== 0) this.bookingQrUrl.set('');
      },
    });
  }
  private loadBookingQr(bookingId: string): void {
    this.isLoadingQr.set(true);
    this.qrErrorMessage.set('');
    this.api.getBookingQr(bookingId).subscribe({
      next: async ({ qrToken }) => {
        try {
          const { toDataURL } = await import('qrcode');
          this.bookingQrUrl.set(await toDataURL(qrToken, { width: 280, margin: 2, errorCorrectionLevel: 'M' }));
        } catch {
          this.qrErrorMessage.set('Không thể tạo hình QR. Bạn vẫn có thể dùng mã đặt chỗ bên dưới.');
        } finally {
          this.isLoadingQr.set(false);
        }
      },
      error: error => {
        this.isLoadingQr.set(false);
        this.qrErrorMessage.set(error.message ?? 'Không thể tải mã QR đặt chỗ.');
      },
    });
  }
  money(value: number): string { return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value); }
  reviewDate(value: string): string { return new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(new Date(value)); }
}
