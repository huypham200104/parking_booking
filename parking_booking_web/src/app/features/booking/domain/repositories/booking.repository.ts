import { Observable } from 'rxjs';
import { BookingFloor, BookingParkingLot, BookingSlot, BookingVehicle, CreatedBooking } from '../entities/booking';

export abstract class BookingRepository {
  abstract getAllParkingLots(): Observable<BookingParkingLot[]>;
  abstract getParkingLot(id: string): Observable<BookingParkingLot>;
  abstract getFloors(parkingLotId: string): Observable<BookingFloor[]>;
  abstract getSlots(parkingLotId: string, floorId: string): Observable<BookingSlot[]>;
  abstract getVehicles(): Observable<BookingVehicle[]>;
  abstract create(parkingSlotId: string, vehicleId: string): Observable<CreatedBooking>;
}
