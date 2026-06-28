import { Observable } from 'rxjs';
import { CurrentUser } from '../entities/current-user';

export abstract class AccountRepository {
  abstract getCurrentUser(): Observable<CurrentUser>;
}
