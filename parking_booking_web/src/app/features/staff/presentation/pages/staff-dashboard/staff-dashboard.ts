import { DatePipe } from '@angular/common';
import { Component, ElementRef, OnDestroy, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { ApiError } from '../../../../../core/infrastructure/http/api-client.service';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { Booking, ParkingLotSummary, StaffBooking, VerifyBookingQr } from '../../../../../core/infrastructure/models/api.models';

interface DetectedBarcode { rawValue: string; }
interface BarcodeDetectorInstance { detect(source: CanvasImageSource): Promise<DetectedBarcode[]>; }
interface BarcodeDetectorConstructor { new(options: { formats: string[] }): BarcodeDetectorInstance; }

@Component({
  selector: 'app-staff-dashboard',
  standalone: true,
  imports: [FormsModule, DatePipe],
  templateUrl: './staff-dashboard.html',
  styleUrl: './staff-dashboard.scss',
})
export class StaffDashboardComponent implements OnInit, OnDestroy {
  private readonly api = inject(ParkingBookingApiService);
  private cameraStream: MediaStream | null = null;
  private scanFrameId: number | null = null;
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

  ngOnInit(): void {
    this.api.getMyStaffParkingLots().pipe(finalize(() => this.isLoadingParkingLots.set(false))).subscribe({
      next: parkingLots => this.parkingLots.set(parkingLots),
      error: (error: ApiError) => this.errorMessage.set(error.message),
    });
    this.loadBookings();
  }

  loadBookings(): void {
    this.isLoadingBookings.set(true);
    this.api.getStaffBookings().pipe(finalize(() => this.isLoadingBookings.set(false))).subscribe({
      next: bookings => {
        this.staffBookings.set(bookings);
        const currentId = this.selectedBooking()?.id;
        this.selectedBooking.set(bookings.find(item => item.id === currentId) ?? bookings[0] ?? null);
      },
      error: (error: ApiError) => this.errorMessage.set(error.message),
    });
  }

  selectBooking(booking: StaffBooking): void {
    this.selectedBooking.set(booking);
    this.verifiedBooking.set(null);
    this.booking.set(null);
    this.checkoutResult.set(null);
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

    const Detector = (window as unknown as { BarcodeDetector?: BarcodeDetectorConstructor }).BarcodeDetector;
    if (!Detector) {
      this.cameraMessage.set('Trình duyệt này chưa hỗ trợ đọc QR trực tiếp. Hãy dùng Chrome/Edge hoặc dán nội dung QR.');
      return;
    }

    try {
      this.cameraStream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: { ideal: 'environment' } }, audio: false });
      this.isCameraOpen.set(true);
      window.setTimeout(async () => {
        const video = this.cameraVideo?.nativeElement;
        if (!video || !this.cameraStream) return;
        video.srcObject = this.cameraStream;
        await video.play();
        this.cameraMessage.set('Đưa mã QR vào giữa khung hình.');
        this.scanCamera(new Detector({ formats: ['qr_code'] }));
      });
    } catch {
      this.stopScanner();
      this.cameraMessage.set('Không mở được camera. Hãy cấp quyền camera hoặc dán nội dung QR.');
    }
  }

  stopScanner(): void {
    if (this.scanFrameId !== null) cancelAnimationFrame(this.scanFrameId);
    this.scanFrameId = null;
    this.cameraStream?.getTracks().forEach(track => track.stop());
    this.cameraStream = null;
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
    this.isSubmitting.set(true);
    this.api.verifyBookingQr(token).pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
      next: result => {
        this.verifiedBooking.set(result);
        const matchedBooking = this.staffBookings().find(item => item.id === result.bookingId) ?? null;
        this.selectedBooking.set(matchedBooking);
        this.successMessage.set('Mã hợp lệ. Hãy đối chiếu biển số trước khi cho xe vào bãi.');
      },
      error: (error: ApiError) => {
        this.verifiedBooking.set(null);
        this.errorMessage.set(error.message);
      },
    });
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
    const bookingId = this.selectedBooking()?.id ?? this.verifiedBooking()?.bookingId;
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
    this.resetMessages();
  }

  private scanCamera(detector: BarcodeDetectorInstance): void {
    const scan = async () => {
      const video = this.cameraVideo?.nativeElement;
      if (!video || !this.cameraStream) return;
      if (video.readyState >= HTMLMediaElement.HAVE_CURRENT_DATA) {
        try {
          const codes = await detector.detect(video);
          const value = codes[0]?.rawValue?.trim();
          if (value) {
            this.qrToken.set(value);
            this.stopScanner();
            this.cameraMessage.set('Đã đọc mã QR. Đang xác minh...');
            this.verifyQr();
            return;
          }
        } catch {
          this.cameraMessage.set('Camera đang hoạt động nhưng chưa đọc được mã QR.');
        }
      }
      this.scanFrameId = requestAnimationFrame(scan);
    };
    this.scanFrameId = requestAnimationFrame(scan);
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
