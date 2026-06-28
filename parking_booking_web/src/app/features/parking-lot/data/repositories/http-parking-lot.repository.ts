import { HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { ApiClient } from '../../../../core/infrastructure/http/api-client.service';
import { ParkingBounds, ParkingLotSummary } from '../../domain/entities/parking-lot';
import { ParkingLotRepository } from '../../domain/repositories/parking-lot.repository';

@Injectable({ providedIn: 'root' })
export class HttpParkingLotRepository extends ParkingLotRepository {
  private readonly api = inject(ApiClient);

  getNearby(lat: number, lng: number, radiusKm = 5) {
    const params = new HttpParams()
      .set('lat', lat)
      .set('lng', lng)
      .set('radiusKm', radiusKm);

    return this.api.get<ParkingLotSummary[]>('/parking-lots/nearby', params);
  }

  getInBounds(bounds: ParkingBounds) {
    let params = new HttpParams()
      .set('minLat', bounds.minLat)
      .set('maxLat', bounds.maxLat)
      .set('minLng', bounds.minLng)
      .set('maxLng', bounds.maxLng);

    if (bounds.userLat != null && bounds.userLng != null) {
      params = params.set('userLat', bounds.userLat).set('userLng', bounds.userLng);
    }

    return this.api.get<ParkingLotSummary[]>('/parking-lots/bounds', params);
  }
}
