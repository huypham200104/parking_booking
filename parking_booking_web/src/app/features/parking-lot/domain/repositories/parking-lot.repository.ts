import { Observable } from 'rxjs';
import { ParkingBounds, ParkingLotSearchResult, ParkingLotSummary } from '../entities/parking-lot';

export abstract class ParkingLotRepository {
  abstract getNearby(lat: number, lng: number, radiusKm?: number): Observable<ParkingLotSummary[]>;
  abstract getInBounds(bounds: ParkingBounds): Observable<ParkingLotSummary[]>;
  abstract search(keyword: string): Observable<ParkingLotSearchResult>;
}
