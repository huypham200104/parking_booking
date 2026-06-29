import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { OwnerStaffAssignment, ParkingLotSummary } from '../../../../../core/infrastructure/models/api.models';

@Component({ selector: 'app-owner-staff', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './owner-staff.html', styleUrl: './owner-staff.scss' })
export class OwnerStaff implements OnInit {
  private readonly api = inject(ParkingBookingApiService);
  private readonly cdr = inject(ChangeDetectorRef);
  staffMembers: OwnerStaffAssignment[] = [];
  parkingLots: ParkingLotSummary[] = [];
  isLoading = true;
  isSaving = false;
  error: string | null = null;
  isFormOpen = false;
  phoneNumber = '';
  fullName = '';
  parkingLotId = '';

  ngOnInit(): void { this.load(); }
  load(): void {
    this.isLoading = true; this.error = null;
    forkJoin({ staff: this.api.getOwnerStaff(), lots: this.api.getOwnerParkingLots() }).subscribe({
      next: ({ staff, lots }) => { this.staffMembers = staff; this.parkingLots = lots; this.parkingLotId ||= lots[0]?.id ?? ''; this.isLoading = false; this.cdr.markForCheck(); },
      error: err => { this.error = err.message; this.isLoading = false; this.cdr.markForCheck(); },
    });
  }
  openForm(): void { this.fullName = ''; this.phoneNumber = ''; this.parkingLotId = this.parkingLots[0]?.id ?? ''; this.error = null; this.isFormOpen = true; }
  closeForm(): void { if (!this.isSaving) this.isFormOpen = false; }
  addStaff(): void {
    if (!this.fullName.trim() || !this.phoneNumber.trim() || !this.parkingLotId) return;
    this.isSaving = true;
    this.api.createParkingLotStaff(this.parkingLotId, this.fullName.trim(), this.phoneNumber.trim()).pipe(finalize(() => { this.isSaving = false; this.cdr.markForCheck(); })).subscribe({
      next: () => { this.isFormOpen = false; this.load(); }, error: err => { this.error = err.message; this.cdr.markForCheck(); },
    });
  }
  removeStaff(item: OwnerStaffAssignment): void {
    if (!confirm(`Xóa ${item.fullName} khỏi ${item.parkingLotName}?`)) return;
    this.api.removeParkingLotStaff(item.parkingLotId, item.userId).subscribe({ next: () => { this.staffMembers = this.staffMembers.filter(x => x !== item); this.cdr.markForCheck(); }, error: err => { this.error = err.message; this.cdr.markForCheck(); } });
  }
}
