export interface BookingParkingLot { id: string; name: string; address: string; availableSlots: number; totalSlots: number; firstBlockPrice: number; firstBlockHours: number; }
export interface BookingFloor { id: string; parkingLotId: string; floorName: string; }
export interface BookingSlot { id: string; parkingFloorId: string; slotName: string; status: number; vehicleType: number; }
export interface BookingVehicle { id: string; licensePlate: string; vehicleType: number; isDefault: boolean; }
export interface CreatedBooking { id: string; bookingCode: string; bookingTimestamp: string; status: number; }
