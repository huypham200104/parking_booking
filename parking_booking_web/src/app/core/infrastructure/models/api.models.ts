export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string | null;
}

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

export interface ParkingLotSummary {
  id: string; name: string; address: string; latitude: number; longitude: number;
  totalSlots: number; availableSlots: number; firstBlockPrice: number; firstBlockHours: number;
  maxHeight: number | null; hasRoof: boolean; is24_7: boolean; averageRating: number;
  status: string; distanceKm: number | null;
}

export interface ParkingLotDetail extends Omit<ParkingLotSummary, 'distanceKm'> {
  ownerId: string; description: string; coverImageUrl: string; contactPhone: string;
  overnightPrice: number | null; openTime: string | null; closeTime: string | null;
}

export interface User { id: string; phoneNumber: string; fullName: string; role: string; trustScore: number; }
export interface Vehicle { id: string; userId: string; licensePlate: string; vehicleType: number; isDefault: boolean; }
export interface Booking { id: string; userId: string | null; vehicleId: string | null; guestLicensePlate: string | null; parkingLotId: string; parkingSlotId: string; bookingCode: string; bookingTimestamp: string; checkInTimestamp: string | null; checkOutTimestamp: string | null; status: string; totalPrice: number | null; }
export interface ParkingFloor { id: string; parkingLotId: string; floorName: string; templateId: string | null; customBackgroundImageUrl: string | null; }
export interface ParkingSlot { id: string; parkingFloorId: string; slotName: string; status: string; vehicleType: string; positionX: number; positionY: number; width: number; height: number; rotation: number; }
export interface LayoutTemplate { id: string; name: string; imageUrl: string; description: string; }
export interface MonthlyPass { id: string; userId: string; vehicleId: string; parkingLotId: string; startDate: string; endDate: string; price: number; status: string; }
export interface WalletTransaction { id: string; amount: number; type: string; referenceId: string; }
export interface Wallet { id: string; userId: string; balance: number; transactions: WalletTransaction[]; }
