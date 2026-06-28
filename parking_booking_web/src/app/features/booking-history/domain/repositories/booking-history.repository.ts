import { Observable } from 'rxjs';
import { BookingHistory } from '../entities/booking-history';

export abstract class BookingHistoryRepository {
  abstract getMine(): Observable<BookingHistory[]>;
}
