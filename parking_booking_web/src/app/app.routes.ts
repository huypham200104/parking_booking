import { Routes } from '@angular/router';
import { MainLayoutComponent } from './shell/presentation/layouts/main-layout/main-layout';
import { authGuard } from './features/auth/application/guards/auth.guard';
import { roleGuard } from './features/auth/application/guards/role.guard';
import { guestOnlyGuard, landingGuard } from './features/auth/application/guards/entry.guard';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      {
        path: '',
        canActivate: [landingGuard],
        loadComponent: () =>
          import('./features/home/presentation/pages/home/home').then((m) => m.HomeComponent),
        title: 'ParkGo – Đặt chỗ đậu xe thông minh',
      },
      {
        path: 'auth/login',
        canActivate: [guestOnlyGuard],
        loadComponent: () =>
          import('./features/auth/presentation/pages/login/login').then((m) => m.LoginComponent),
        title: 'Đăng nhập – ParkGo',
      },
      {
        path: 'auth/register',
        canActivate: [guestOnlyGuard],
        loadComponent: () =>
          import('./features/auth/presentation/pages/register/register').then((m) => m.RegisterComponent),
        title: 'Đăng ký – ParkGo',
      },
      {
        path: 'staff',
        canActivate: [authGuard, roleGuard],
        data: { role: 2 },
        loadComponent: () =>
          import('./features/staff/presentation/pages/staff-dashboard/staff-dashboard').then((m) => m.StaffDashboardComponent),
        title: 'Vận hành bãi xe – ParkGo',
      },
      {
        path: 'map',
        canActivate: [authGuard, roleGuard],
        data: { role: 0 },
        loadComponent: () =>
          import('./features/parking-lot/presentation/pages/map-search/map-search').then((m) => m.MapSearch),
        title: 'Tìm bãi đỗ xe – ParkGo',
      },
      {
        path: 'history',
        canActivate: [authGuard, roleGuard],
        data: { role: 0 },
        loadComponent: () =>
          import('./features/booking-history/presentation/pages/booking-history/booking-history').then((m) => m.BookingHistoryComponent),
        title: 'Lịch sử đặt chỗ – ParkGo',
      },
      {
        path: 'booking',
        canActivate: [authGuard, roleGuard],
        data: { role: 0 },
        loadComponent: () =>
          import('./features/booking/presentation/pages/create-booking/create-booking').then((m) => m.CreateBookingComponent),
        title: 'Đặt chỗ – ParkGo',
      },
      {
        path: 'bookings/:id',
        canActivate: [authGuard, roleGuard],
        data: { role: 0 },
        loadComponent: () =>
          import('./features/booking/presentation/pages/booking-detail/booking-detail').then((m) => m.BookingDetailComponent),
        title: 'Mã QR đặt chỗ – ParkGo',
      },
      {
        path: 'vehicles',
        canActivate: [authGuard, roleGuard],
        data: { role: 0 },
        loadComponent: () =>
          import('./features/vehicle/presentation/pages/vehicle-management/vehicle-management').then((m) => m.VehicleManagementComponent),
        title: 'Phương tiện của tôi – ParkGo',
      },
      { path: 'wallet', canActivate: [authGuard, roleGuard], data: { role: 0 }, loadComponent: () => import('./features/wallet/presentation/pages/wallet/wallet').then(m => m.WalletComponent), title: 'Ví ParkGo' },
      { path: 'vouchers', canActivate: [authGuard, roleGuard], data: { role: 0 }, loadComponent: () => import('./features/voucher/presentation/pages/voucher-list/voucher-list').then(m => m.VoucherListComponent), title: 'Ưu đãi ParkGo' },
    ],
  },
  // Admin Routes (Role = 3)
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard],
    data: { role: 3 },
    loadComponent: () => import('./shell/presentation/layouts/admin-layout/admin-layout').then(m => m.AdminLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/admin/presentation/pages/admin-dashboard/admin-dashboard').then(m => m.AdminDashboardComponent),
        title: 'Bảng điều khiển – ParkGo',
      },
      { path: 'parking-lots', loadComponent: () => import('./features/admin/presentation/pages/parking-lots/parking-lots').then(m => m.ParkingLots), title: 'Quản lý bãi đỗ xe – ParkGo' },
      { path: 'bookings', loadComponent: () => import('./features/admin/presentation/pages/bookings/bookings').then(m => m.Bookings), title: 'Quản lý lượt đặt chỗ – ParkGo' },
      { path: 'users', loadComponent: () => import('./features/admin/presentation/pages/user-management/user-management').then(m => m.UserManagementComponent), title: 'Quản lý người dùng – ParkGo' },
      { path: 'vouchers', loadComponent: () => import('./features/admin/presentation/pages/voucher-management/voucher-management').then(m => m.VoucherManagementComponent), title: 'Quản lý voucher – ParkGo' },
      { path: 'settings', loadComponent: () => import('./features/admin/presentation/pages/settings/settings').then(m => m.Settings), title: 'Cài đặt hệ thống – ParkGo' }
    ]
  },
  {
    path: 'owner',
    canActivate: [authGuard, roleGuard],
    data: { role: 1 },
    loadComponent: () => import('./shell/presentation/layouts/owner-layout/owner-layout').then(m => m.OwnerLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/owner/presentation/pages/owner-dashboard/owner-dashboard').then(m => m.OwnerDashboardComponent),
        title: 'Quản lý bãi đỗ – ParkGo',
      },
      {
        path: 'lots',
        loadComponent: () => import('./features/owner/presentation/pages/owner-parking-lots/owner-parking-lots').then(m => m.OwnerParkingLots),
        title: 'Bãi đỗ của tôi – ParkGo',
      },
      {
        path: 'bookings',
        loadComponent: () => import('./features/owner/presentation/pages/owner-bookings/owner-bookings').then(m => m.OwnerBookings),
        title: 'Lượt đặt chỗ – ParkGo',
      },
      {
        path: 'staff',
        loadComponent: () => import('./features/owner/presentation/pages/owner-staff/owner-staff').then(m => m.OwnerStaff),
        title: 'Nhân sự – ParkGo',
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/owner/presentation/pages/owner-reports/owner-reports').then(m => m.OwnerReports),
        title: 'Báo cáo doanh thu – ParkGo',
      }
    ]
  },

  {
    path: '**',
    redirectTo: '',
  },
];
