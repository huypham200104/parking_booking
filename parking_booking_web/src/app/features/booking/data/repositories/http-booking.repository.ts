import { Injectable, inject } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { ApiClient } from '../../../../core/infrastructure/http/api-client.service';
import { BookingFloor, BookingParkingLot, BookingSlot, BookingVehicle, CreatedBooking } from '../../domain/entities/booking';
import { BookingRepository } from '../../domain/repositories/booking.repository';

@Injectable()
export class HttpBookingRepository extends BookingRepository {
  private readonly api = inject(ApiClient);
  getAllParkingLots() {
    const params = new HttpParams()
      .set('minLat', -90).set('maxLat', 90)
      .set('minLng', -180).set('maxLng', 180);
    return this.api.get<BookingParkingLot[]>('/parking-lots/bounds', params);
  }
  getParkingLot(id: string) { return this.api.get<BookingParkingLot>(`/parking-lots/${id}`); }
  getFloors(parkingLotId: string) { return this.api.get<BookingFloor[]>(`/parking-lots/${parkingLotId}/floors`); }
  getSlots(parkingLotId: string, floorId: string) { return this.api.get<BookingSlot[]>(`/parking-lots/${parkingLotId}/floors/${floorId}/slots`); }
  getVehicles() { return this.api.get<BookingVehicle[]>('/vehicles'); }
  create(parkingSlotId: string, vehicleId: string) { return this.api.post<CreatedBooking>('/bookings', { parkingSlotId, vehicleId, guestLicensePlate: null }); }
}
