import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { ParkingFloor, ParkingLotSummary, ParkingSlot } from '../../../../../core/infrastructure/models/api.models';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-owner-parking-lots',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './owner-parking-lots.html',
  styleUrl: './owner-parking-lots.scss',
})
export class OwnerParkingLots implements OnInit {
  private readonly api = inject(ParkingBookingApiService);
  private readonly cdr = inject(ChangeDetectorRef);
  
  parkingLots: ParkingLotSummary[] = [];
  isLoading = true;
  error: string | null = null;
  isFormOpen = false;
  isSaving = false;
  editingId: string | null = null;
  form = this.emptyForm();
  isLayoutOpen = false;
  layoutLot: ParkingLotSummary | null = null;
  floors: ParkingFloor[] = [];
  selectedFloor: ParkingFloor | null = null;
  slots: ParkingSlot[] = [];
  newFloorName = '';
  slotCount = 0;
  isLayoutLoading = false;
  isLayoutSaving = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api.getOwnerParkingLots().subscribe({
      next: (data) => {
        this.parkingLots = data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err.message;
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  openCreate(): void { this.editingId = null; this.form = this.emptyForm(); this.error = null; this.isFormOpen = true; }
  openEdit(id: string): void {
    this.error = null;
    this.api.getParkingLot(id).subscribe({
      next: lot => {
        this.editingId = lot.id;
        this.form = { name: lot.name, address: lot.address, latitude: lot.latitude, longitude: lot.longitude, firstBlockPrice: lot.firstBlockPrice, firstBlockHours: lot.firstBlockHours, is24_7: lot.is24_7, contactPhone: lot.contactPhone };
        this.isFormOpen = true;
        this.cdr.markForCheck();
      },
      error: err => { this.error = err.message; this.cdr.markForCheck(); },
    });
  }
  closeForm(): void { if (!this.isSaving) this.isFormOpen = false; }
  save(): void {
    if (!this.form.name.trim() || !this.form.address.trim()) return;
    this.isSaving = true;
    const request = { ...this.form, name: this.form.name.trim(), address: this.form.address.trim(), contactPhone: this.form.contactPhone.trim() };
    const call = this.editingId ? this.api.updateParkingLot(this.editingId, request) : this.api.createParkingLot({ ...request, ownerId: null });
    call.pipe(finalize(() => { this.isSaving = false; this.cdr.markForCheck(); })).subscribe({
      next: () => { this.isFormOpen = false; this.load(); },
      error: err => { this.error = err.message; this.cdr.markForCheck(); },
    });
  }

  openLayout(lot: ParkingLotSummary): void {
    this.layoutLot = lot; this.isLayoutOpen = true; this.error = null; this.isLayoutLoading = true;
    this.api.getFloors(lot.id).pipe(finalize(() => { this.isLayoutLoading = false; this.cdr.markForCheck(); })).subscribe({
      next: floors => { this.floors = floors; floors[0] ? this.selectFloor(floors[0]) : this.resetFloorSelection(); },
      error: err => { this.error = err.message; this.cdr.markForCheck(); },
    });
  }
  closeLayout(): void { if (!this.isLayoutSaving) this.isLayoutOpen = false; }
  createFloor(): void {
    const lot = this.layoutLot; const name = this.newFloorName.trim(); if (!lot || !name) return;
    this.isLayoutSaving = true;
    this.api.createFloor(lot.id, { floorName: name }).pipe(finalize(() => { this.isLayoutSaving = false; this.cdr.markForCheck(); })).subscribe({
      next: floor => { this.floors = [...this.floors, floor]; this.newFloorName = ''; this.selectFloor(floor); },
      error: err => { this.error = err.message; this.cdr.markForCheck(); },
    });
  }
  selectFloor(floor: ParkingFloor): void {
    if (!this.layoutLot) return; this.selectedFloor = floor; this.isLayoutLoading = true;
    this.api.getSlots(this.layoutLot.id, floor.id).pipe(finalize(() => { this.isLayoutLoading = false; this.cdr.markForCheck(); })).subscribe({
      next: slots => { this.slots = slots; this.slotCount = slots.length; }, error: err => { this.error = err.message; this.cdr.markForCheck(); },
    });
  }
  saveSlotCount(): void {
    const lot = this.layoutLot; const floor = this.selectedFloor; if (!lot || !floor) return;
    const count = Math.max(0, Math.min(500, Math.floor(this.slotCount)));
    const request = Array.from({ length: count }, (_, index) => {
      const current = this.slots[index];
      return { id: current?.id ?? null, slotName: current?.slotName ?? `${floor.floorName}-${(index + 1).toString().padStart(2, '0')}`, status: current ? Number(current.status) : 0, vehicleType: current ? Number(current.vehicleType) : 0, positionX: (index % 5) * 110, positionY: Math.floor(index / 5) * 70, width: 100, height: 60, rotation: 0 };
    });
    this.isLayoutSaving = true;
    this.api.saveSlots(lot.id, floor.id, request).pipe(finalize(() => { this.isLayoutSaving = false; this.cdr.markForCheck(); })).subscribe({
      next: slots => { this.slots = slots; this.slotCount = slots.length; this.load(); }, error: err => { this.error = err.message; this.cdr.markForCheck(); },
    });
  }
  toggleSlot(slot: ParkingSlot): void {
    const status = Number(slot.status);
    if (status === 1) return;
    slot.status = status === 0 ? 2 : 0;
  }
  slotStatusLabel(slot: ParkingSlot): string {
    return ['Còn trống', 'Đang có xe', 'Bảo trì'][Number(slot.status)] ?? 'Không xác định';
  }
  private resetFloorSelection(): void { this.selectedFloor = null; this.slots = []; this.slotCount = 0; }

  private emptyForm() { return { name: '', address: '', latitude: 10.7769, longitude: 106.7009, firstBlockPrice: 20000, firstBlockHours: 1, is24_7: true, contactPhone: '' }; }
}
