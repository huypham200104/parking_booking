import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { ParkingBookingApiService } from '../../core/infrastructure/http/parking-booking-api.service';
import { AppNotification } from '../../core/infrastructure/models/api.models';

@Component({ selector: 'app-notification-center', standalone: true, imports: [DatePipe], templateUrl: './notification-center.html', styleUrl: './notification-center.scss' })
export class NotificationCenterComponent implements OnInit, OnDestroy {
  private readonly api = inject(ParkingBookingApiService);
  readonly notifications = signal<AppNotification[]>([]);
  readonly isOpen = signal(false);
  readonly isLoading = signal(false);
  readonly unreadCount = computed(() => this.notifications().filter(item => !item.isRead).length);
  private pollTimer: number | undefined;
  ngOnInit(): void { this.load(); this.pollTimer = window.setInterval(() => this.load(false), 30_000); }
  ngOnDestroy(): void { window.clearInterval(this.pollTimer); }
  toggle(): void { this.isOpen.update(value => !value); if (this.isOpen()) this.load(); }
  close(): void { this.isOpen.set(false); }
  load(showLoading = true): void {
    if (showLoading) this.isLoading.set(true);
    this.api.getNotifications().subscribe({ next: items => { this.notifications.set(items); this.isLoading.set(false); }, error: () => this.isLoading.set(false) });
  }
  markRead(item: AppNotification): void {
    if (item.isRead) return;
    this.api.markNotificationRead(item.id).subscribe({ next: () => this.notifications.update(items => items.map(value => value.id === item.id ? { ...value, isRead: true } : value)) });
  }
  markAllRead(): void { this.api.markAllNotificationsRead().subscribe({ next: () => this.notifications.update(items => items.map(value => ({ ...value, isRead: true }))) }); }
}
