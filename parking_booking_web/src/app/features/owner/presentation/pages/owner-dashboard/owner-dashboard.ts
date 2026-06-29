import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { ParkingLotSummary, StaffBooking } from '../../../../../core/infrastructure/models/api.models';

@Component({
  selector: 'app-owner-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './owner-dashboard.html',
  styleUrls: ['./owner-dashboard.scss']
})
export class OwnerDashboardComponent implements OnInit {
  private readonly api = inject(ParkingBookingApiService);
  private readonly cdr = inject(ChangeDetectorRef);

  parkingLots: ParkingLotSummary[] = [];
  bookings: StaffBooking[] = [];
  isLoading = true;
  error: string | null = null;

  get parkedVehicles(): number { return this.bookings.filter(booking => booking.status === 1).length; }
  get totalMovements(): number { return this.bookings.length; }
  get totalRevenue(): number { return this.bookings.reduce((sum, booking) => sum + (booking.totalPrice ?? 0), 0); }

  ngOnInit(): void {
    forkJoin({ lots: this.api.getOwnerParkingLots(), bookings: this.api.getOwnerBookings() }).subscribe({
      next: ({ lots, bookings }) => {
        this.parkingLots = lots;
        this.bookings = bookings;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.error = error.message ?? 'Không thể tải dữ liệu tổng quan.';
        this.isLoading = false;
        this.cdr.markForCheck();
      },
    });
  }
}
