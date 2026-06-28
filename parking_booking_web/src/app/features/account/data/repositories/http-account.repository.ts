import { Injectable, inject } from '@angular/core';
import { ApiClient } from '../../../../core/infrastructure/http/api-client.service';
import { CurrentUser } from '../../domain/entities/current-user';
import { AccountRepository } from '../../domain/repositories/account.repository';

@Injectable()
export class HttpAccountRepository extends AccountRepository {
  private readonly api = inject(ApiClient);
  getCurrentUser() { return this.api.get<CurrentUser>('/users/me'); }
}
