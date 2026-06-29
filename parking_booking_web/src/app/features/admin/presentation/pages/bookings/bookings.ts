import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { timeout } from 'rxjs';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { PaginationResponse, StaffBooking } from '../../../../../core/infrastructure/models/api.models';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule, DatePipe, CurrencyPipe],
  templateUrl: './bookings.html',
  styleUrls: ['./bookings.scss'],
})
export class Bookings implements OnInit {
  private readonly apiService = inject(ParkingBookingApiService);
  private readonly cdr = inject(ChangeDetectorRef);

  bookings: StaffBooking[] = [];
  loading = false;
  error: string | null = null;
  pageIndex = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.error = null;
    this.cdr.markForCheck();
    
    this.apiService.getAllAdminBookings(this.pageIndex, this.pageSize)
      .pipe(timeout(10000))
      .subscribe({
      next: (res: PaginationResponse<StaffBooking>) => {
        this.bookings = res.items;
        this.totalCount = res.totalCount;
        this.pageIndex = res.pageIndex;
        this.pageSize = res.pageSize;
        this.totalPages = res.totalPages;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        if (err.name === 'TimeoutError') {
          this.error = 'Yêu cầu quá hạn (Timeout). Vui lòng kiểm tra lại kết nối hoặc server.';
        } else {
          this.error = `Lỗi hệ thống: ${err.message || err.statusText || 'Không xác định'}`;
        }
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.pageIndex) {
      this.pageIndex = page;
      this.loadData();
    }
  }

  nextPage(): void { if (this.pageIndex < this.totalPages) this.goToPage(this.pageIndex + 1); }
  prevPage(): void { if (this.pageIndex > 1) this.goToPage(this.pageIndex - 1); }

  getPages(): number[] {
    const start = Math.max(1, this.pageIndex - 2);
    const end = Math.min(this.totalPages, start + 4);
    const adjustedStart = Math.max(1, end - 4);
    return Array.from({ length: end - adjustedStart + 1 }, (_, i) => adjustedStart + i);
  }

  statusLabel(status: string | number): string {
    const value = typeof status === 'number' ? status : { Pending: 0, CheckedIn: 1, Completed: 2, Cancelled: 3, NoShow: 4 }[status] ?? -1;
    return ['Đang chờ', 'Đang đỗ', 'Hoàn thành', 'Đã hủy', 'Không đến'][value] ?? 'Không xác định';
  }

  statusClass(status: string | number): string {
    const value = typeof status === 'number' ? status : { Pending: 0, CheckedIn: 1, Completed: 2, Cancelled: 3, NoShow: 4 }[status] ?? -1;
    return ['status-pending', 'status-checkedin', 'status-completed', 'status-cancelled', 'status-noshow'][value] ?? 'status-unknown';
  }
}
