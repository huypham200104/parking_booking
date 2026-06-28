import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authInterceptor } from './core/infrastructure/interceptors/auth.interceptor';
import { HttpAuthRepository } from './features/auth/data/repositories/http-auth.repository';
import { AuthRepository } from './features/auth/domain/repositories/auth.repository';
import { HttpParkingLotRepository } from './features/parking-lot/data/repositories/http-parking-lot.repository';
import { ParkingLotRepository } from './features/parking-lot/domain/repositories/parking-lot.repository';
import { HttpBookingHistoryRepository } from './features/booking-history/data/repositories/http-booking-history.repository';
import { BookingHistoryRepository } from './features/booking-history/domain/repositories/booking-history.repository';
import { HttpBookingRepository } from './features/booking/data/repositories/http-booking.repository';
import { BookingRepository } from './features/booking/domain/repositories/booking.repository';
import { HttpAccountRepository } from './features/account/data/repositories/http-account.repository';
import { AccountRepository } from './features/account/domain/repositories/account.repository';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    { provide: AuthRepository, useClass: HttpAuthRepository },
    { provide: ParkingLotRepository, useClass: HttpParkingLotRepository },
    { provide: BookingHistoryRepository, useClass: HttpBookingHistoryRepository },
    { provide: BookingRepository, useClass: HttpBookingRepository },
    { provide: AccountRepository, useClass: HttpAccountRepository },
  ],
};
