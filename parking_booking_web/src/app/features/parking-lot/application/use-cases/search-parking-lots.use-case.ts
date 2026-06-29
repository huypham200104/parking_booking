import { Injectable, inject } from '@angular/core';
import { ParkingLotRepository } from '../../domain/repositories/parking-lot.repository';

@Injectable({ providedIn: 'root' })
export class SearchParkingLotsUseCase {
  private readonly repository = inject(ParkingLotRepository);

  execute(keyword: string) {
    return this.repository.search(keyword);
  }
}
