import { Injectable, inject } from '@angular/core';
import { ParkingBounds } from '../../domain/entities/parking-lot';
import { ParkingLotRepository } from '../../domain/repositories/parking-lot.repository';

@Injectable({ providedIn: 'root' })
export class GetParkingLotsInBoundsUseCase {
  private readonly repository = inject(ParkingLotRepository);
  execute(bounds: ParkingBounds) { return this.repository.getInBounds(bounds); }
}
