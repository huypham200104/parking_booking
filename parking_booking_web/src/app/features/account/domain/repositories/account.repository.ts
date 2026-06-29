import { Observable } from 'rxjs';
import { CurrentUser, UpdateCurrentUser } from '../entities/current-user';

export abstract class AccountRepository {
  abstract getCurrentUser(): Observable<CurrentUser>;
  abstract updateCurrentUser(request: UpdateCurrentUser): Observable<CurrentUser>;
}
