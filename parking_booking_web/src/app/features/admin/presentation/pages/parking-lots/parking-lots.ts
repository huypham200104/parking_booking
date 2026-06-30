import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { timeout } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { PaginationResponse, ParkingLotSummary } from '../../../../../core/infrastructure/models/api.models';

@Component({
  selector: 'app-parking-lots',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './parking-lots.html',
  styleUrls: ['./parking-lots.scss'],
})
export class ParkingLots implements OnInit {
  private readonly apiService = inject(ParkingBookingApiService);
  private readonly cdr = inject(ChangeDetectorRef);

  parkingLots: ParkingLotSummary[] = [];
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
    
    this.apiService.getAllAdminParkingLots(this.pageIndex, this.pageSize)
      .pipe(timeout(10000))
      .subscribe({
      next: (res: PaginationResponse<ParkingLotSummary>) => {
        this.parkingLots = res.items;
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
    const total = this.totalPages || 0;
    if (total === 0) return [];
    
    if (total <= 10) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }
    
    let start = Math.max(1, this.pageIndex - 2);
    let end = Math.min(total, start + 4);
    
    if (end - start < 4) {
      start = Math.max(1, end - 4);
    }
    
    return Array.from({ length: end - start + 1 }, (_, i) => start + i);
  }

  statusLabel(status: string | number): string {
    const value = typeof status === 'number' ? status : { PendingApproval: 0, Active: 1, Suspended: 2 }[status] ?? -1;
    return ['Chờ duyệt', 'Hoạt động', 'Tạm ngưng'][value] ?? 'Không xác định';
  }

  statusClass(status: string | number): string {
    const value = typeof status === 'number' ? status : { PendingApproval: 0, Active: 1, Suspended: 2 }[status] ?? -1;
    return ['status-pending', 'status-active', 'status-suspended'][value] ?? 'status-unknown';
  }

  // Modal logic
  showModal = false;
  modalMode: 'view' = 'view';
  modalForm: any = null;

  approveLot(lot: ParkingLotSummary) {
    if (confirm(`Bạn có chắc muốn duyệt bãi đỗ xe "${lot.name}"?`)) {
      this.apiService.approveParkingLot(lot.id).subscribe({
        next: () => {
          this.loadData();
          // Optionally show a toast here
        },
        error: (err) => {
          alert('Có lỗi xảy ra khi duyệt: ' + (err.error?.message || err.message));
        }
      });
    }
  }

  openViewModal(lot: ParkingLotSummary) {
    this.modalMode = 'view';
    this.modalForm = { ...lot };
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.modalForm = null;
  }
}
