import { HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { ApiClient } from './api-client.service';
import { Booking, LayoutTemplate, MonthlyPass, ParkingFloor, ParkingLotDetail, ParkingSlot, User, Vehicle, Wallet } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ParkingBookingApiService {
  private readonly api = inject(ApiClient);

  getCurrentUser() { return this.api.get<User>('/users/me'); }
  getVehicles() { return this.api.get<Vehicle[]>('/vehicles'); }
  createVehicle(request: { licensePlate: string; vehicleType: number; isDefault: boolean }) { return this.api.post<Vehicle>('/vehicles', request); }
  getWallet() { return this.api.get<Wallet>('/wallets/me'); }
  deposit(amount: number) { return this.api.post<{ amount: number; vietQrUrl: string }>('/wallets/deposit', { amount }); }
  getMonthlyPasses() { return this.api.get<MonthlyPass[]>('/monthly-passes/me'); }
  createMonthlyPass(request: { vehicleId: string; parkingLotId: string; durationDays: number }) { return this.api.post<MonthlyPass>('/monthly-passes', request); }

  getParkingLot(id: string) { return this.api.get<ParkingLotDetail>(`/parking-lots/${id}`); }
  reportParkingLot(id: string, request: { status: number; currentLat: number; currentLng: number }) { return this.api.post<{ reported: boolean }>(`/parking-lots/${id}/report`, request); }
  addParkingLotStaff(id: string, userId: string) { return this.api.post<{ added: boolean }>(`/parking-lots/${id}/staff`, { userId }); }
  getFloors(parkingLotId: string) { return this.api.get<ParkingFloor[]>(`/parking-lots/${parkingLotId}/floors`); }
  createFloor(parkingLotId: string, request: { floorName: string; templateId?: string | null; customBackgroundImageUrl?: string | null }) { return this.api.post<ParkingFloor>(`/parking-lots/${parkingLotId}/floors`, request); }
  getSlots(parkingLotId: string, floorId: string) { return this.api.get<ParkingSlot[]>(`/parking-lots/${parkingLotId}/floors/${floorId}/slots`); }
  saveSlots(parkingLotId: string, floorId: string, slots: ReadonlyArray<Omit<ParkingSlot, 'parkingFloorId'>>) { return this.api.put<ParkingSlot[]>(`/parking-lots/${parkingLotId}/floors/${floorId}/slots`, slots); }
  getLayoutTemplates() { return this.api.get<LayoutTemplate[]>('/layout-templates'); }

  createBooking(request: { parkingSlotId: string; vehicleId?: string | null; guestLicensePlate?: string | null }) { return this.api.post<Booking>('/bookings', request); }
  checkIn(id: string) { return this.api.post<Booking>(`/bookings/${id}/check-in`); }
  applyVoucher(id: string, voucherCode: string) { return this.api.post<Booking>(`/bookings/${id}/apply-voucher`, { voucherCode }); }
  checkOut(id: string, useWallet: boolean) { return this.api.post<{ totalPrice: number; vietQrUrl: string | null; status: string }>(`/bookings/${id}/check-out`, { useWallet }); }
  cancelBooking(id: string) { return this.api.post<{ cancelled: boolean }>(`/bookings/${id}/cancel`); }

  processPaymentWebhook(request: { code: string; data: { amount: number; description: string; transactionDateTime: string } }) { return this.api.post<{ matched: boolean }>('/payments/webhook', request); }

  seedDevelopmentData(recreate = false) { return this.api.post<unknown>('/dev/data/seed?' + new HttpParams().set('recreate', recreate).toString()); }
}
