import { Injectable, inject } from '@angular/core';
import { BookingHistoryRepository } from '../../domain/repositories/booking-history.repository';

@Injectable({ providedIn: 'root' })
export class GetBookingHistoryUseCase {
  private readonly repository = inject(BookingHistoryRepository);
  execute() { return this.repository.getMine(); }
}
