import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';

@Component({ selector: 'app-admin-notifications', standalone: true, imports: [CommonModule, ReactiveFormsModule], templateUrl: './notifications.html', styleUrl: './notifications.scss' })
export class AdminNotifications {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ParkingBookingApiService);
  private readonly cdr = inject(ChangeDetectorRef);
  saving = false; error = ''; success = '';
  form = this.fb.group({ phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{10,11}$/)]], title: ['', [Validators.required, Validators.maxLength(200)]], message: ['', [Validators.required, Validators.maxLength(1000)]] });
  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true; this.error = ''; this.success = '';
    const value = this.form.getRawValue();
    this.api.sendNotificationToUser({ phoneNumber: value.phoneNumber!, title: value.title!, message: value.message! }).subscribe({
      next: () => { this.saving = false; this.success = 'Đã gửi thông báo thành công.'; this.form.reset(); this.cdr.markForCheck(); },
      error: error => { this.saving = false; this.error = error.message; this.cdr.markForCheck(); }
    });
  }
}
