import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { Vehicle } from '../../../../../core/infrastructure/models/api.models';

@Component({
  selector: 'app-vehicle-management',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './vehicle-management.html',
  styleUrl: './vehicle-management.scss',
})
export class VehicleManagementComponent {
  private readonly api = inject(ParkingBookingApiService);
  readonly vehicles = signal<Vehicle[]>([]);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly deletingId = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly vehicleCount = computed(() => this.vehicles().length);

  licensePlate = '';
  vehicleType = 0;
  isDefault = false;
  editVehicleType = 0;
  editIsDefault = false;

  readonly vehicleTypes = [
    { value: 0, label: 'Sedan' },
    { value: 1, label: 'SUV' },
    { value: 2, label: 'Hatchback' },
    { value: 3, label: 'Xe máy' },
    { value: 4, label: 'Khác' },
  ];

  constructor() { this.loadVehicles(); }

  loadVehicles(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.api.getVehicles().pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: vehicles => this.vehicles.set(vehicles),
      error: error => this.errorMessage.set(error.message ?? 'Không thể tải danh sách phương tiện.'),
    });
  }

  addVehicle(): void {
    const licensePlate = this.licensePlate.trim().toUpperCase();
    if (licensePlate.length < 5) {
      this.errorMessage.set('Biển số xe phải có ít nhất 5 ký tự.');
      return;
    }

    this.isSaving.set(true);
    this.clearMessages();
    this.api.createVehicle({ licensePlate, vehicleType: this.vehicleType, isDefault: this.isDefault }).pipe(
      finalize(() => this.isSaving.set(false)),
    ).subscribe({
      next: vehicle => {
        this.vehicles.update(items => [
          vehicle,
          ...items.map(item => this.isDefault ? { ...item, isDefault: false } : item),
        ].sort((a, b) => Number(b.isDefault) - Number(a.isDefault)));
        this.licensePlate = '';
        this.isDefault = false;
        this.successMessage.set('Đã thêm phương tiện mới.');
      },
      error: error => this.errorMessage.set(error.message ?? 'Không thể thêm phương tiện.'),
    });
  }

  deleteVehicle(vehicle: Vehicle): void {
    if (!window.confirm(`Xóa phương tiện ${vehicle.licensePlate}?`)) return;
    this.deletingId.set(vehicle.id);
    this.clearMessages();
    this.api.deleteVehicle(vehicle.id).pipe(finalize(() => this.deletingId.set(null))).subscribe({
      next: () => {
        this.vehicles.update(items => items.filter(item => item.id !== vehicle.id));
        this.successMessage.set(`Đã xóa phương tiện ${vehicle.licensePlate}.`);
      },
      error: error => this.errorMessage.set(error.message ?? 'Không thể xóa phương tiện.'),
    });
  }

  startEdit(vehicle: Vehicle): void { this.editingId.set(vehicle.id); this.editVehicleType = vehicle.vehicleType; this.editIsDefault = vehicle.isDefault; this.clearMessages(); }
  cancelEdit(): void { this.editingId.set(null); }
  saveEdit(vehicle: Vehicle): void {
    this.isSaving.set(true); this.clearMessages();
    this.api.updateVehicle(vehicle.id, { vehicleType: this.editVehicleType, isDefault: this.editIsDefault }).pipe(finalize(() => this.isSaving.set(false))).subscribe({
      next: updated => {
        this.vehicles.update(items => items.map(item => item.id === updated.id ? updated : (updated.isDefault ? { ...item, isDefault: false } : item)).sort((a, b) => Number(b.isDefault) - Number(a.isDefault)));
        this.editingId.set(null); this.successMessage.set(`Đã cập nhật ${updated.licensePlate}.`);
      },
      error: error => this.errorMessage.set(error.message ?? 'Không thể cập nhật phương tiện.'),
    });
  }

  typeLabel(type: number): string { return this.vehicleTypes.find(item => item.value === type)?.label ?? 'Khác'; }
  private clearMessages(): void { this.errorMessage.set(''); this.successMessage.set(''); }
}
