import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { GetBookingHistoryUseCase } from '../../../application/use-cases/get-booking-history.use-case';
import { BookingHistory, BookingStatus } from '../../../domain/entities/booking-history';

type HistoryFilter = 'all' | 'active' | 'completed' | 'cancelled';

@Component({
  selector: 'app-booking-history',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './booking-history.html',
  styleUrl: './booking-history.scss',
})
export class BookingHistoryComponent {
  private readonly getHistory = inject(GetBookingHistoryUseCase);
  readonly bookings = signal<BookingHistory[]>([]);
  readonly filter = signal<HistoryFilter>('all');
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  readonly filteredBookings = computed(() => this.bookings().filter(booking => {
    const filter = this.filter();
    if (filter === 'active') return booking.status === BookingStatus.Pending || booking.status === BookingStatus.CheckedIn;
    if (filter === 'completed') return booking.status === BookingStatus.Completed;
    if (filter === 'cancelled') return booking.status === BookingStatus.Cancelled || booking.status === BookingStatus.NoShow;
    return true;
  }));

  readonly completedCount = computed(() => this.bookings().filter(item => item.status === BookingStatus.Completed).length);
  readonly totalSpent = computed(() => this.bookings().reduce((total, item) => total + (item.totalPrice ?? 0), 0));

  constructor() { this.load(); }

  load(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.getHistory.execute().pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: bookings => this.bookings.set(bookings),
      error: error => this.errorMessage.set(error.message ?? 'Không thể tải lịch sử đặt chỗ.'),
    });
  }

  setFilter(filter: HistoryFilter): void { this.filter.set(filter); }
  statusLabel(status: BookingStatus): string {
    return ['Chờ nhận chỗ', 'Đang gửi xe', 'Hoàn thành', 'Đã hủy', 'Không đến'][status] ?? 'Không xác định';
  }
  statusClass(status: BookingStatus): string { return `status-${status}`; }
  formatMoney(value: number | null): string { return value == null ? 'Chưa thanh toán' : new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value); }
}
