import { DatePipe } from '@angular/common';
import { Component, ElementRef, OnDestroy, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { ApiError } from '../../../../../core/infrastructure/http/api-client.service';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { BrowserQRCodeReader, IScannerControls } from '@zxing/browser';
import { Booking, ParkingLotSummary, ProcessBookingQr, StaffBooking, VerifyBookingQr } from '../../../../../core/infrastructure/models/api.models';

@Component({
  selector: 'app-staff-dashboard',
  standalone: true,
  imports: [FormsModule, DatePipe],
  templateUrl: './staff-dashboard.html',
  styleUrl: './staff-dashboard.scss',
})
export class StaffDashboardComponent implements OnInit, OnDestroy {
  private readonly api = inject(ParkingBookingApiService);
  private readonly qrReader = new BrowserQRCodeReader();
  private scannerControls: IScannerControls | null = null;
  private isProcessingQr = false;
  @ViewChild('cameraVideo') private cameraVideo?: ElementRef<HTMLVideoElement>;

  readonly parkingLots = signal<ParkingLotSummary[]>([]);
  readonly primaryParkingLot = computed(() => this.parkingLots()[0] ?? null);
  readonly isLoadingParkingLots = signal(true);
  readonly staffBookings = signal<StaffBooking[]>([]);
  readonly selectedBooking = signal<StaffBooking | null>(null);
  readonly isLoadingBookings = signal(true);
  readonly qrToken = signal('');
  readonly verifiedBooking = signal<VerifyBookingQr | null>(null);
  readonly booking = signal<Booking | null>(null);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly checkoutResult = signal<{ totalPrice: number; vietQrUrl: string | null; status: string; checkOutTimestamp: string } | null>(null);
  readonly isCameraOpen = signal(false);
  readonly cameraMessage = signal('');
  readonly checkoutConfirmation = signal<ProcessBookingQr | null>(null);
  readonly bookingPage = signal(1);
  readonly bookingPageSize = 10;
  readonly bookingTotalPages = signal(0);
  readonly bookingTotalCount = signal(0);

  ngOnInit(): void {
    this.api.getMyStaffParkingLots().pipe(finalize(() => this.isLoadingParkingLots.set(false))).subscribe({
      next: parkingLots => this.parkingLots.set(parkingLots),
      error: (error: ApiError) => this.errorMessage.set(error.message),
    });
    this.loadBookings();
  }

  loadBookings(): void {
    this.isLoadingBookings.set(true);
    this.api.getStaffBookings(this.bookingPage(), this.bookingPageSize).pipe(finalize(() => this.isLoadingBookings.set(false))).subscribe({
      next: response => {
        const bookings = response.items;
        this.staffBookings.set(bookings);
        this.bookingPage.set(response.pageIndex);
        this.bookingTotalPages.set(response.totalPages);
        this.bookingTotalCount.set(response.totalCount);
        const currentId = this.selectedBooking()?.id;
        this.selectedBooking.set(bookings.find(item => item.id === currentId) ?? bookings[0] ?? null);
      },
      error: (error: ApiError) => this.errorMessage.set(error.message),
    });
  }

  goToBookingPage(page: number): void {
    if (page < 1 || page > this.bookingTotalPages() || page === this.bookingPage()) return;
    this.bookingPage.set(page);
    this.loadBookings();
  }

  selectBooking(booking: StaffBooking): void {
    this.selectedBooking.set(booking);
    this.verifiedBooking.set(null);
    this.booking.set(null);
    this.checkoutResult.set(null);
    this.checkoutConfirmation.set(null);
    this.resetMessages();
  }

  statusLabel(status: number): string {
    return ['Chờ check-in', 'Đang trong bãi', 'Đã hoàn tất', 'Đã hủy', 'Không đến'][status] ?? 'Không xác định';
  }

  ngOnDestroy(): void { this.stopScanner(); }

  async startScanner(): Promise<void> {
    this.resetMessages();
    this.cameraMessage.set('Đang mở camera...');

    if (!navigator.mediaDevices?.getUserMedia) {
      this.cameraMessage.set('Trình duyệt không hỗ trợ camera. Hãy dán nội dung QR vào ô bên dưới.');
      return;
    }

    try {
      this.isCameraOpen.set(true);
      window.setTimeout(async () => {
        const video = this.cameraVideo?.nativeElement;
        if (!video) return;
        try {
          this.cameraMessage.set('Đưa mã QR vào giữa khung hình.');
          this.scannerControls = await this.qrReader.decodeFromVideoDevice(undefined, video, result => {
            if (result && !this.isProcessingQr) this.handleDecodedToken(result.getText());
          });
        } catch {
          this.stopScanner();
          this.cameraMessage.set('Không mở được camera. Hãy cấp quyền camera hoặc tải ảnh QR.');
        }
      });
    } catch {
      this.stopScanner();
      this.cameraMessage.set('Không mở được camera. Hãy cấp quyền camera hoặc dán nội dung QR.');
    }
  }

  stopScanner(): void {
    this.scannerControls?.stop();
    this.scannerControls = null;
    this.isCameraOpen.set(false);
  }

  verifyQr(): void {
    const token = this.qrToken().trim();
    if (!token) {
      this.errorMessage.set('Vui lòng quét hoặc dán mã QR của khách hàng.');
      return;
    }

    this.resetMessages();
    this.booking.set(null);
    this.checkoutResult.set(null);
    this.checkoutConfirmation.set(null);
    this.processQrToken(token);
  }

  async uploadQrImage(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    this.resetMessages();
    if (!file.type.startsWith('image/')) { this.errorMessage.set('Vui lòng chọn một file ảnh.'); return; }
    if (file.size > 10 * 1024 * 1024) { this.errorMessage.set('Ảnh QR không được vượt quá 10 MB.'); return; }
    const url = URL.createObjectURL(file);
    try {
      const result = await this.qrReader.decodeFromImageUrl(url);
      this.handleDecodedToken(result.getText());
    } catch {
      this.errorMessage.set('Không tìm thấy mã QR hợp lệ trong ảnh đã chọn.');
    } finally {
      URL.revokeObjectURL(url);
    }
  }

  private processQrToken(token: string): void {
    if (this.isProcessingQr) return;
    this.isProcessingQr = true;
    this.isSubmitting.set(true);
    this.checkoutConfirmation.set(null);
    this.api.processBookingQr(token).pipe(finalize(() => { this.isSubmitting.set(false); this.isProcessingQr = false; })).subscribe({
      next: result => {
        this.verifiedBooking.set({ isValid: true, bookingId: result.bookingId, bookingCode: result.bookingCode, licensePlate: result.licensePlate, message: null });
        const matchedBooking = this.staffBookings().find(item => item.id === result.bookingId) ?? null;
        this.selectedBooking.set(matchedBooking);
        if (result.action === 'CheckedIn') {
          this.staffBookings.update(items => items.map(item => item.id === result.bookingId ? { ...item, status: 1, checkInTimestamp: result.checkInTimestamp } : item));
          this.selectedBooking.update(item => item?.id === result.bookingId ? { ...item, status: 1, checkInTimestamp: result.checkInTimestamp } : item);
          this.successMessage.set('QR hợp lệ. Xe đã được check-in tự động.');
        } else {
          this.checkoutConfirmation.set(result);
          this.successMessage.set('QR hợp lệ. Vui lòng xác nhận đã thu tiền để checkout.');
        }
      },
      error: (error: ApiError) => {
        this.verifiedBooking.set(null);
        this.errorMessage.set(error.message);
      },
    });
  }

  private handleDecodedToken(token: string): void {
    const value = token.trim();
    if (!value || this.isProcessingQr) return;
    this.qrToken.set(value);
    this.stopScanner();
    this.cameraMessage.set('Đã đọc mã QR. Đang xử lý...');
    this.processQrToken(value);
  }

  checkIn(): void {
    const bookingId = this.selectedBooking()?.id ?? this.verifiedBooking()?.bookingId;
    if (!bookingId) return;
    this.runBookingAction(
      () => this.api.checkIn(bookingId),
      'Check-in thành công. Xe đã được ghi nhận vào bãi.'
    );
  }

  checkOut(): void {
    const bookingId = this.checkoutConfirmation()?.bookingId ?? this.selectedBooking()?.id ?? this.verifiedBooking()?.bookingId;
    if (!bookingId) return;

    this.resetMessages();
    this.isSubmitting.set(true);
    this.api.checkOut(bookingId, false, true).pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
      next: result => {
        this.checkoutResult.set(result);
        this.selectedBooking.update(current => current?.id === bookingId
          ? { ...current, status: 2, totalPrice: result.totalPrice, checkOutTimestamp: result.checkOutTimestamp }
          : current);
        this.staffBookings.update(items => items.map(item => item.id === bookingId
          ? { ...item, status: 2, totalPrice: result.totalPrice, checkOutTimestamp: result.checkOutTimestamp }
          : item));
        this.successMessage.set('Check-out thành công. Thu đúng số tiền hiển thị từ khách hàng.');
        this.checkoutConfirmation.set(null);
        this.api.getMyStaffParkingLots().subscribe({ next: lots => this.parkingLots.set(lots) });
      },
      error: (error: ApiError) => this.errorMessage.set(error.message),
    });
  }

  clearScanner(): void {
    this.stopScanner();
    this.qrToken.set('');
    this.verifiedBooking.set(null);
    this.booking.set(null);
    this.checkoutResult.set(null);
    this.checkoutConfirmation.set(null);
    this.resetMessages();
  }

  private runBookingAction(action: () => ReturnType<ParkingBookingApiService['checkIn']>, message: string): void {
    this.resetMessages();
    this.isSubmitting.set(true);
    action().pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
      next: booking => {
        this.booking.set(booking);
        this.selectedBooking.update(current => current?.id === booking.id
          ? { ...current, checkInTimestamp: booking.checkInTimestamp, status: Number(booking.status) }
          : current);
        this.staffBookings.update(items => items.map(item => item.id === booking.id
          ? { ...item, checkInTimestamp: booking.checkInTimestamp, status: Number(booking.status) }
          : item));
        this.successMessage.set(message);
      },
      error: (error: ApiError) => this.errorMessage.set(error.message),
    });
  }

  private resetMessages(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
  }
}
