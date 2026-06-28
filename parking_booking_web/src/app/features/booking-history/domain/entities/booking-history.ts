export enum BookingStatus {
  Pending = 0,
  CheckedIn = 1,
  Completed = 2,
  Cancelled = 3,
  NoShow = 4,
}

export interface BookingHistory {
  id: string;
  bookingCode: string;
  parkingLotName: string;
  parkingLotAddress: string;
  licensePlate: string;
  bookingTimestamp: string;
  checkInTimestamp: string | null;
  checkOutTimestamp: string | null;
  status: BookingStatus;
  totalPrice: number | null;
}
