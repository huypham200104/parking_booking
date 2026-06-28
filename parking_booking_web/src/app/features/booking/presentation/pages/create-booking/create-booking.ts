import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { BookingFacade } from '../../../application/booking.facade';
import { BookingFloor, BookingParkingLot, BookingSlot, BookingVehicle, CreatedBooking } from '../../../domain/entities/booking';

@Component({ selector: 'app-create-booking', standalone: true, imports: [CommonModule, RouterLink], templateUrl: './create-booking.html', styleUrl: './create-booking.scss' })
export class CreateBookingComponent {
  private readonly facade = inject(BookingFacade);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly parkingLotId = signal(this.route.snapshot.queryParamMap.get('parkingLotId'));
  readonly parkingLots = signal<BookingParkingLot[]>([]);
  readonly page = signal(1);
  readonly pageSize = 6;
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.parkingLots().length / this.pageSize)));
  readonly pagedParkingLots = computed(() => this.parkingLots().slice((this.page() - 1) * this.pageSize, this.page() * this.pageSize));
  readonly parkingLot = signal<BookingParkingLot | null>(null);
  readonly floors = signal<BookingFloor[]>([]);
  readonly slots = signal<BookingSlot[]>([]);
  readonly vehicles = signal<BookingVehicle[]>([]);
  readonly selectedFloor = signal<BookingFloor | null>(null);
  readonly selectedSlot = signal<BookingSlot | null>(null);
  readonly selectedVehicle = signal<BookingVehicle | null>(null);
  readonly isLoading = signal(false);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');
  readonly createdBooking = signal<CreatedBooking | null>(null);

  constructor() { const id = this.parkingLotId(); id ? this.load(id) : this.loadParkingLots(); }
  loadParkingLots(): void {
    this.isLoading.set(true); this.errorMessage.set('');
    this.facade.getAllParkingLots().pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: lots => this.parkingLots.set(lots),
      error: error => this.errorMessage.set(error.message ?? 'Không thể tải danh sách bãi đỗ xe.'),
    });
  }
  chooseParkingLot(lot: BookingParkingLot): void {
    this.parkingLotId.set(lot.id);
    void this.router.navigate([], { relativeTo: this.route, queryParams: { parkingLotId: lot.id }, replaceUrl: true });
    this.load(lot.id);
  }
  changeParkingLot(): void {
    this.parkingLotId.set(null); this.parkingLot.set(null); this.selectedFloor.set(null); this.selectedSlot.set(null); this.slots.set([]);
    void this.router.navigate([], { relativeTo: this.route, queryParams: {}, replaceUrl: true });
    if (this.parkingLots().length === 0) this.loadParkingLots();
  }
  goToPage(page: number): void { if (page >= 1 && page <= this.totalPages()) this.page.set(page); }
  load(id: string): void {
    this.isLoading.set(true); this.errorMessage.set('');
    this.facade.load(id).pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: data => { this.parkingLot.set(data.parkingLot); this.floors.set(data.floors); this.vehicles.set(data.vehicles); this.selectedVehicle.set(data.vehicles.find(v => v.isDefault) ?? data.vehicles[0] ?? null); if (data.floors[0]) this.selectFloor(data.floors[0]); },
      error: error => this.errorMessage.set(error.message ?? 'Không thể tải thông tin đặt chỗ.'),
    });
  }
  selectFloor(floor: BookingFloor): void { this.selectedFloor.set(floor); this.selectedSlot.set(null); this.facade.getSlots(floor.parkingLotId, floor.id).subscribe({ next: slots => this.slots.set(slots), error: error => this.errorMessage.set(error.message) }); }
  selectSlot(slot: BookingSlot): void { if (slot.status === 0) this.selectedSlot.set(slot); }
  selectVehicle(vehicle: BookingVehicle): void { this.selectedVehicle.set(vehicle); }
  submit(): void {
    const slot = this.selectedSlot(); const vehicle = this.selectedVehicle();
    if (!slot || !vehicle) { this.errorMessage.set('Vui lòng chọn chỗ đỗ và phương tiện.'); return; }
    this.isSubmitting.set(true); this.errorMessage.set('');
    this.facade.create(slot.id, vehicle.id).pipe(finalize(() => this.isSubmitting.set(false))).subscribe({ next: booking => this.createdBooking.set(booking), error: error => this.errorMessage.set(error.message ?? 'Không thể tạo đặt chỗ.') });
  }
  money(value: number): string { return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value); }
}
