import { Routes } from '@angular/router';
import { MainLayoutComponent } from './shell/presentation/layouts/main-layout/main-layout';
import { authGuard } from './features/auth/application/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/home/presentation/pages/home/home').then((m) => m.HomeComponent),
        title: 'ParkGo – Đặt chỗ đậu xe thông minh',
      },
      {
        path: 'auth/login',
        loadComponent: () =>
          import('./features/auth/presentation/pages/login/login').then((m) => m.LoginComponent),
        title: 'Đăng nhập – ParkGo',
      },
      {
        path: 'auth/register',
        loadComponent: () =>
          import('./features/auth/presentation/pages/register/register').then((m) => m.RegisterComponent),
        title: 'Đăng ký – ParkGo',
      },
      {
        path: 'map',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/parking-lot/presentation/pages/map-search/map-search').then((m) => m.MapSearch),
        title: 'Tìm bãi đỗ xe – ParkGo',
      },
      {
        path: 'history',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/booking-history/presentation/pages/booking-history/booking-history').then((m) => m.BookingHistoryComponent),
        title: 'Lịch sử đặt chỗ – ParkGo',
      },
      {
        path: 'booking',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/booking/presentation/pages/create-booking/create-booking').then((m) => m.CreateBookingComponent),
        title: 'Đặt chỗ – ParkGo',
      },
    ],
  },
  // Future routes: /parking-lots, /booking, /auth, /admin
  {
    path: '**',
    redirectTo: '',
  },
];
