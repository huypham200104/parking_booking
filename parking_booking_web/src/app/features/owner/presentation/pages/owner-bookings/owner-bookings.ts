import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { PaginationResponse, StaffBooking } from '../../../../../core/infrastructure/models/api.models';

@Component({
  selector: 'app-owner-bookings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './owner-bookings.html',
  styleUrl: './owner-bookings.scss',
})
export class OwnerBookings implements OnInit {
  private readonly api = inject(ParkingBookingApiService);
  private readonly cdr = inject(ChangeDetectorRef);
  
  bookings: StaffBooking[] = [];
  isLoading = true;
  error: string | null = null;
  pageIndex = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  ngOnInit(): void {
    this.loadBookings();
  }

  loadBookings(): void {
    this.isLoading = true;
    this.error = null;
    this.api.getOwnerBookings(this.pageIndex, this.pageSize).subscribe({
      next: (data: PaginationResponse<StaffBooking>) => {
        this.bookings = data.items;
        this.pageIndex = data.pageIndex;
        this.pageSize = data.pageSize;
        this.totalCount = data.totalCount;
        this.totalPages = data.totalPages;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err.message;
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.pageIndex) return;
    this.pageIndex = page;
    this.loadBookings();
  }

  getPages(): number[] {
    const start = Math.max(1, Math.min(this.pageIndex - 2, this.totalPages - 4));
    const end = Math.min(this.totalPages, start + 4);
    return Array.from({ length: Math.max(0, end - start + 1) }, (_, index) => start + index);
  }

  getStatusText(status: number): string {
    const statuses: { [key: number]: string } = {
      0: 'Chờ nhận xe',
      1: 'Đã nhận xe',
      2: 'Hoàn thành',
      3: 'Đã hủy',
      4: 'Quá hạn'
    };
    return statuses[status] || 'Không rõ';
  }
}
