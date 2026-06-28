import { Observable } from 'rxjs';
import { ParkingBounds, ParkingLotSummary } from '../entities/parking-lot';

export abstract class ParkingLotRepository {
  abstract getNearby(lat: number, lng: number, radiusKm?: number): Observable<ParkingLotSummary[]>;
  abstract getInBounds(bounds: ParkingBounds): Observable<ParkingLotSummary[]>;
}
