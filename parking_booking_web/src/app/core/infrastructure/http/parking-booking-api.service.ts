import { HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { ApiClient } from './api-client.service';
import { Booking, BookingHistoryItem, LayoutTemplate, OwnerStaffAssignment, PaginationResponse, ParkingFloor, ParkingLotDetail, ParkingLotSummary, ParkingSlot, Review, StaffBooking, User, AdminUser, Vehicle, VerifyBookingQr, Voucher, VoucherRequest, Wallet } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ParkingBookingApiService {
  private readonly api = inject(ApiClient);

  getCurrentUser() { return this.api.get<User>('/users/me'); }
  updateMe(request: { fullName: string }) { return this.api.put<User>('/users/me', request); }
  getAllUsers(page = 1, size = 10, hasPenalty?: boolean) {
    let params = new HttpParams().set('page', page).set('size', size);
    if (hasPenalty) params = params.set('hasPenalty', hasPenalty);
    return this.api.get<PaginationResponse<AdminUser>>('/users', params);
  }
  toggleUserLock(id: string) { return this.api.put<{ success: boolean }>(`/users/${id}/lock`, {}); }
  getAllAdminParkingLots(page = 1, size = 10, keyword?: string) {
    let params = new HttpParams().set('page', page).set('size', size);
    if (keyword) params = params.set('keyword', keyword);
    return this.api.get<PaginationResponse<ParkingLotSummary>>('/parking-lots/admin/all', params);
  }
  createParkingLot(request: { name: string; address: string; ownerId?: string | null; latitude: number; longitude: number; firstBlockPrice: number; firstBlockHours: number; is24_7: boolean; contactPhone: string }) {
    return this.api.post<ParkingLotDetail>('/parking-lots', request);
  }
  updateParkingLot(id: string, request: { name: string; address: string; latitude: number; longitude: number; firstBlockPrice: number; firstBlockHours: number; is24_7: boolean; contactPhone: string }) { return this.api.put<ParkingLotDetail>(`/parking-lots/${id}`, request); }
  getVehicles() { return this.api.get<Vehicle[]>('/vehicles'); }
  createVehicle(request: { licensePlate: string; vehicleType: number; isDefault: boolean }) { return this.api.post<Vehicle>('/vehicles', request); }
  updateVehicle(id: string, request: { vehicleType: number; isDefault: boolean }) { return this.api.put<Vehicle>(`/vehicles/${id}`, request); }
  deleteVehicle(id: string) { return this.api.delete<{ deleted: boolean }>(`/vehicles/${id}`); }
  getWallet() { return this.api.get<Wallet>('/wallets/me'); }
  deposit(amount: number) { return this.api.post<{ amount: number; vietQrUrl: string }>('/wallets/deposit', { amount }); }
  getValidVouchers() { return this.api.get<Voucher[]>('/vouchers/valid'); }
  createVoucher(request: VoucherRequest & { code: string }) { return this.api.post<Voucher>('/vouchers', request); }
  updateVoucher(id: string, request: VoucherRequest) { return this.api.put<Voucher>(`/vouchers/${id}`, request); }
  deleteVoucher(id: string) { return this.api.delete<{ deleted: boolean }>(`/vouchers/${id}`); }
  getAllParkingLots() { const params = new HttpParams().set('minLat', -90).set('maxLat', 90).set('minLng', -180).set('maxLng', 180); return this.api.get<ParkingLotSummary[]>('/parking-lots/bounds', params); }

  getParkingLot(id: string) { return this.api.get<ParkingLotDetail>(`/parking-lots/${id}`); }
  getParkingLotReviews(id: string) { return this.api.get<Review[]>(`/parking-lots/${id}/reviews`); }
  createReview(request: { bookingId: string; rating: number; comment: string | null }) { return this.api.post<Review>('/reviews', request); }
  getMyStaffParkingLots() { return this.api.get<ParkingLotSummary[]>('/parking-lots/staff/me'); }
  getOwnerParkingLots() { return this.api.get<ParkingLotSummary[]>('/parking-lots/owner/me'); }
  getOwnerStaff() { return this.api.get<OwnerStaffAssignment[]>('/parking-lots/owner/staff'); }
  getOwnerBookings() { return this.api.get<StaffBooking[]>('/bookings/owner'); }
  reportParkingLot(id: string, request: { status: number; currentLat: number; currentLng: number }) { return this.api.post<{ reported: boolean }>(`/parking-lots/${id}/report`, request); }
  addParkingLotStaff(id: string, userId: string) { return this.api.post<{ added: boolean }>(`/parking-lots/${id}/staff`, { userId }); }
  addParkingLotStaffByPhone(id: string, phoneNumber: string) { return this.api.post<{ added: boolean }>(`/parking-lots/${id}/staff/by-phone`, { phoneNumber }); }
  createParkingLotStaff(id: string, fullName: string, phoneNumber: string) { return this.api.post<{ created: boolean }>(`/parking-lots/${id}/staff/create`, { fullName, phoneNumber }); }
  removeParkingLotStaff(id: string, userId: string) { return this.api.delete<{ removed: boolean }>(`/parking-lots/${id}/staff/${userId}`); }
  getFloors(parkingLotId: string) { return this.api.get<ParkingFloor[]>(`/parking-lots/${parkingLotId}/floors`); }
  createFloor(parkingLotId: string, request: { floorName: string; templateId?: string | null; customBackgroundImageUrl?: string | null }) { return this.api.post<ParkingFloor>(`/parking-lots/${parkingLotId}/floors`, request); }
  getSlots(parkingLotId: string, floorId: string) { return this.api.get<ParkingSlot[]>(`/parking-lots/${parkingLotId}/floors/${floorId}/slots`); }
  saveSlots(parkingLotId: string, floorId: string, slots: ReadonlyArray<{ id: string | null; slotName: string; status: number; vehicleType: number; positionX: number; positionY: number; width: number; height: number; rotation: number }>) { return this.api.put<ParkingSlot[]>(`/parking-lots/${parkingLotId}/floors/${floorId}/slots`, slots); }
  getLayoutTemplates() { return this.api.get<LayoutTemplate[]>('/layout-templates'); }

  createBooking(request: { parkingSlotId: string; vehicleId?: string | null; guestLicensePlate?: string | null }) { return this.api.post<Booking>('/bookings', request); }
  getMyBookings() { return this.api.get<BookingHistoryItem[]>('/bookings/me'); }
  getRecentCompletedParkingLots() { return this.api.get<ParkingLotSummary[]>('/bookings/recent-parking-lots'); }
  getStaffBookings() { return this.api.get<StaffBooking[]>('/bookings/staff'); }
  getAllAdminBookings(page = 1, size = 10) { return this.api.get<PaginationResponse<StaffBooking>>('/bookings/admin/all', new HttpParams().set('page', page).set('size', size)); }
  getBookingQr(id: string) { return this.api.get<{ qrToken: string }>(`/bookings/${id}/qr`); }
  verifyBookingQr(qrToken: string) { return this.api.post<VerifyBookingQr>('/bookings/verify-qr', { qrToken }); }
  checkIn(id: string) { return this.api.post<Booking>(`/bookings/${id}/check-in`); }
  applyVoucher(id: string, voucherCode: string) { return this.api.post<Booking>(`/bookings/${id}/apply-voucher`, { voucherCode }); }
  checkOut(id: string, useWallet: boolean, collectCash = false) { return this.api.post<{ totalPrice: number; vietQrUrl: string | null; status: string; checkOutTimestamp: string }>(`/bookings/${id}/check-out`, { useWallet, collectCash }); }
  cancelBooking(id: string) { return this.api.post<{ cancelled: boolean }>(`/bookings/${id}/cancel`); }

  processPaymentWebhook(request: { code: string; data: { amount: number; description: string; transactionDateTime: string } }) { return this.api.post<{ matched: boolean }>('/payments/webhook', request); }

  seedDevelopmentData(recreate = false) { return this.api.post<unknown>('/dev/data/seed?' + new HttpParams().set('recreate', recreate).toString()); }
}
