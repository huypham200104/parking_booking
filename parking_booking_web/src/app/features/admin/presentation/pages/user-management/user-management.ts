import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { timeout } from 'rxjs';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { AdminUser, PaginationResponse } from '../../../../../core/infrastructure/models/api.models';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, DatePipe, FormsModule],
  templateUrl: './user-management.html',
  styleUrls: ['./user-management.scss']
})
export class UserManagementComponent implements OnInit {
  private readonly apiService = inject(ParkingBookingApiService);
  private readonly cdr = inject(ChangeDetectorRef);

  users: AdminUser[] = [];
  loading = false;
  error: string | null = null;
  pageIndex = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  filterPenalty = false;

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.error = null;
    this.cdr.markForCheck();
    
    this.apiService.getAllUsers(this.pageIndex, this.pageSize, this.filterPenalty)
      .pipe(timeout(10000))
      .subscribe({
      next: (res: PaginationResponse<AdminUser>) => {
        this.users = res.items;
        this.pageIndex = res.pageIndex;
        this.pageSize = res.pageSize;
        this.totalCount = res.totalCount;
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
      this.loadUsers(); 
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

  toggleLock(user: AdminUser): void {
    if (!confirm(`Bạn có chắc muốn ${user.isLocked ? 'mở khóa' : 'khóa'} tài khoản ${user.phoneNumber}?`)) {
      return;
    }
    
    this.apiService.toggleUserLock(user.id).subscribe({
      next: () => {
        user.isLocked = !user.isLocked;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        alert('Có lỗi xảy ra: ' + (err.error?.message || err.message));
        this.cdr.markForCheck();
      }
    });
  }

  getRoleName(role: string | number): string {
    const roleMap: Record<string, string> = {
      '0': 'Customer',
      '1': 'Owner',
      '2': 'Staff',
      '3': 'Admin'
    };
    return roleMap[role.toString()] || role.toString();
  }

  getRoleBadgeClass(role: string | number): string {
    const roleStr = role.toString();
    const map: Record<string, string> = {
      '0': 'role-customer',
      '1': 'role-owner',
      '2': 'role-staff',
      '3': 'role-admin'
    };
    return map[roleStr] || 'role-customer';
  }
}
