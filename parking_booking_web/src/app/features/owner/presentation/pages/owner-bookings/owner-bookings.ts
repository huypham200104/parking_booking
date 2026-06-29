import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { StaffBooking } from '../../../../../core/infrastructure/models/api.models';

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

  ngOnInit(): void {
    this.api.getOwnerBookings().subscribe({
      next: (data) => {
        this.bookings = data;
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
