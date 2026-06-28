import { Injectable, inject } from '@angular/core';
import { ApiClient } from '../../../../core/infrastructure/http/api-client.service';
import { BookingHistory } from '../../domain/entities/booking-history';
import { BookingHistoryRepository } from '../../domain/repositories/booking-history.repository';

@Injectable()
export class HttpBookingHistoryRepository extends BookingHistoryRepository {
  private readonly api = inject(ApiClient);
  getMine() { return this.api.get<BookingHistory[]>('/bookings/me'); }
}
