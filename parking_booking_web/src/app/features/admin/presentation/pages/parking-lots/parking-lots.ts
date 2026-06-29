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
    const start = Math.max(1, this.pageIndex - 2);
    const end = Math.min(this.totalPages, start + 4);
    const adjustedStart = Math.max(1, end - 4);
    return Array.from({ length: end - adjustedStart + 1 }, (_, i) => adjustedStart + i);
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
  modalMode: 'create' | 'edit' | 'view' = 'view';
  modalForm: any = null;
  modalSaving = false;
  modalError: string | null = null;

  openCreateModal() {
    this.modalMode = 'create';
    this.modalError = null;
    this.modalForm = {
      name: '', address: '', ownerId: '00000000-0000-0000-0000-000000000000', // We might need a real ownerId, or we can fetch a default owner or let admin select. For now, since admin creates it, maybe they are the owner? Wait, in ParkingLotService: owner.Role must be ParkingOwner. We need a way to select owner.
      // We will just put an empty string and admin must paste an owner ID for now, or we can query users.
      // To keep it simple, we'll add an Owner ID field.
      latitude: 10.762622, longitude: 106.660172,
      firstBlockPrice: 10000, firstBlockHours: 2,
      is24_7: true, contactPhone: ''
    };
    this.showModal = true;
  }

  openViewModal(lot: ParkingLotSummary) {
    this.modalMode = 'view';
    this.modalError = null;
    this.modalForm = { ...lot };
    this.showModal = true;
  }

  openEditModal(lot: ParkingLotSummary) {
    this.modalMode = 'edit';
    this.modalError = null;
    this.modalForm = { ...lot };
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.modalForm = null;
  }

  saveModal() {
    if (this.modalMode === 'create') {
      this.modalSaving = true;
      this.modalError = null;
      this.apiService.createParkingLot(this.modalForm).subscribe({
        next: () => {
          this.modalSaving = false;
          this.closeModal();
          this.loadData();
        },
        error: (err) => {
          this.modalSaving = false;
          this.modalError = err.error?.message || 'Có lỗi xảy ra khi tạo bãi đỗ xe';
          this.cdr.markForCheck();
        }
      });
    } else {
      // Edit is not implemented in backend yet, so just close or show error
      this.modalError = 'Tính năng cập nhật chưa được hỗ trợ.';
    }
  }
}
