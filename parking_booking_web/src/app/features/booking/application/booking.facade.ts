import { Injectable, inject } from '@angular/core';
import { forkJoin } from 'rxjs';
import { BookingRepository } from '../domain/repositories/booking.repository';

@Injectable({ providedIn: 'root' })
export class BookingFacade {
  private readonly repository = inject(BookingRepository);
  getAllParkingLots() { return this.repository.getAllParkingLots(); }
  load(parkingLotId: string) { return forkJoin({ parkingLot: this.repository.getParkingLot(parkingLotId), floors: this.repository.getFloors(parkingLotId), vehicles: this.repository.getVehicles() }); }
  getSlots(parkingLotId: string, floorId: string) { return this.repository.getSlots(parkingLotId, floorId); }
  create(parkingSlotId: string, vehicleId: string) { return this.repository.create(parkingSlotId, vehicleId); }
}
