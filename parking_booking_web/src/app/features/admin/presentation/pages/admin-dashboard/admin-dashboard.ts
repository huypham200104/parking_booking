import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-dashboard.html',
  styleUrls: ['./admin-dashboard.scss']
})
export class AdminDashboardComponent {
  private readonly fb = inject(FormBuilder);
  private readonly apiService = inject(ParkingBookingApiService);

  showNotificationModal = false;
  submittingNotification = false;
  notificationError = '';
  notificationSuccess = false;

  notificationForm = this.fb.group({
    phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{10,11}$/)]],
    title: ['', Validators.required],
    message: ['', Validators.required]
  });

  openNotificationModal() {
    this.showNotificationModal = true;
    this.notificationSuccess = false;
    this.notificationError = '';
    this.notificationForm.reset();
  }

  closeNotificationModal() {
    this.showNotificationModal = false;
  }

  submitNotification() {
    if (this.notificationForm.invalid) {
      this.notificationForm.markAllAsTouched();
      return;
    }

    this.submittingNotification = true;
    this.notificationError = '';
    this.notificationSuccess = false;

    const val = this.notificationForm.value;
    this.apiService.sendNotificationToUser({
      phoneNumber: val.phoneNumber!,
      title: val.title!,
      message: val.message!
    }).subscribe({
      next: () => {
        this.submittingNotification = false;
        this.notificationSuccess = true;
        setTimeout(() => this.closeNotificationModal(), 2000);
      },
      error: (err) => {
        this.submittingNotification = false;
        this.notificationError = err?.error?.message || 'Có lỗi xảy ra khi gửi thông báo.';
      }
    });
  }
}
