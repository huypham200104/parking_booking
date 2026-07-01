import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { AdminDashboard, AdminWalletStats } from '../../../../../core/infrastructure/models/api.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-dashboard.html',
  styleUrls: ['./admin-dashboard.scss']
})
export class AdminDashboardComponent implements OnInit {
  private readonly apiService = inject(ParkingBookingApiService);
  private readonly cdr = inject(ChangeDetectorRef);

  walletStats: AdminWalletStats | null = null;
  walletStatsLoading = true;
  walletStatsError = '';
  dashboard: AdminDashboard | null = null;
  isLoading = true;
  error = '';

  ngOnInit(): void {
    forkJoin({ dashboard: this.apiService.getAdminDashboard(), wallet: this.apiService.getAdminWalletStats() }).subscribe({
      next: ({ dashboard, wallet }) => {
        this.dashboard = dashboard;
        const stats = wallet;
        this.walletStats = stats;
        this.walletStatsLoading = false;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        this.error = error.message;
        this.walletStatsError = error.message;
        this.walletStatsLoading = false;
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  statusLabel(status: number): string {
    return ['Đang chờ', 'Đang đỗ', 'Hoàn thành', 'Đã hủy', 'Không đến'][status] ?? 'Không xác định';
  }
}
