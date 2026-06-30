export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string | null;
}

export interface PaginationResponse<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
}

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

export interface AppNotification { id: string; title: string; message: string; isRead: boolean; createdAt: string; }

export interface ParkingLotSummary {
  id: string; name: string; address: string; latitude: number; longitude: number;
  totalSlots: number; availableSlots: number; firstBlockPrice: number; firstBlockHours: number;
  maxHeight: number | null; hasRoof: boolean; is24_7: boolean; averageRating: number;
  status: string | number; distanceKm: number | null;
}

export interface ParkingLotDetail extends Omit<ParkingLotSummary, 'distanceKm'> {
  ownerId: string; description: string; coverImageUrl: string; contactPhone: string;
  overnightPrice: number | null; openTime: string | null; closeTime: string | null;
}

export interface User { id: string; phoneNumber: string; fullName: string; role: string; trustScore: number; }
export interface AdminUser extends User { isLocked: boolean; createdAt: string; penaltyCount: number; }
export interface OwnerStaffAssignment { userId: string; fullName: string; phoneNumber: string; isLocked: boolean; createdAt: string; parkingLotId: string; parkingLotName: string; }
export interface Vehicle { id: string; userId: string; licensePlate: string; vehicleType: number; isDefault: boolean; }
export interface Booking { id: string; userId: string | null; vehicleId: string | null; guestLicensePlate: string | null; parkingLotId: string; parkingSlotId: string; bookingCode: string; bookingTimestamp: string; checkInTimestamp: string | null; checkOutTimestamp: string | null; status: string; totalPrice: number | null; }
export interface BookingHistoryItem { id: string; bookingCode: string; parkingLotName: string; parkingLotAddress: string; licensePlate: string; bookingTimestamp: string; checkInTimestamp: string | null; checkOutTimestamp: string | null; status: number; totalPrice: number | null; }
export interface StaffBooking { id: string; bookingCode: string; parkingLotName: string; floorName: string; slotName: string; licensePlate: string; bookingTimestamp: string; checkInTimestamp: string | null; checkOutTimestamp: string | null; status: number; totalPrice: number | null; }
export interface VerifyBookingQr { isValid: boolean; bookingId: string | null; bookingCode: string | null; licensePlate: string | null; message: string | null; }
export interface Review { id: string; userId: string; userName: string; rating: number; comment: string | null; createdAt: string; }
export interface ParkingFloor { id: string; parkingLotId: string; floorName: string; templateId: string | null; customBackgroundImageUrl: string | null; }
export interface ParkingSlot { id: string; parkingFloorId: string; slotName: string; status: number; vehicleType: number; positionX: number; positionY: number; width: number; height: number; rotation: number; }
export interface LayoutTemplate { id: string; name: string; imageUrl: string; description: string; }
export interface WalletTransaction { id: string; amount: number; type: string; referenceId: string; }
export interface Wallet { id: string; userId: string; balance: number; transactions: WalletTransaction[]; }
export interface Voucher { id: string; code: string; discountAmount: number | null; discountPercentage: number | null; maxDiscount: number | null; expiryDate: string; usageLimit: number; }
export interface VoucherRequest { discountAmount: number | null; discountPercentage: number | null; maxDiscount: number | null; expiryDate: string; usageLimit: number; }
