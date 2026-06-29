import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { User } from '../../../../../core/infrastructure/models/api.models';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.html',
  styleUrls: ['./settings.scss']
})
export class Settings implements OnInit {
  private readonly apiService = inject(ParkingBookingApiService);
  private readonly cdr = inject(ChangeDetectorRef);

  user: User | null = null;
  loading = false;
  saving = false;
  error: string | null = null;
  successMsg: string | null = null;

  formData = { fullName: '', phoneNumber: '' };

  ngOnInit() {
    this.loadUser();
  }

  loadUser() {
    this.loading = true;
    this.apiService.getCurrentUser().subscribe({
      next: (u) => {
        this.user = u;
        this.formData = { fullName: u.fullName || '', phoneNumber: u.phoneNumber || '' };
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = 'Không thể tải thông tin admin.';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  saveSettings() {
    this.saving = true;
    this.error = null;
    this.successMsg = null;
    this.apiService.updateMe({ fullName: this.formData.fullName }).subscribe({
      next: (u) => {
        this.user = u;
        this.successMsg = 'Lưu thông tin thành công!';
        this.saving = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err.error?.message || 'Có lỗi xảy ra khi lưu thông tin.';
        this.saving = false;
        this.cdr.markForCheck();
      }
    });
  }
}
