export interface ParkingLotSummary {
  id: string; name: string; address: string; latitude: number; longitude: number;
  totalSlots: number; availableSlots: number; firstBlockPrice: number; firstBlockHours: number;
  maxHeight: number | null; hasRoof: boolean; is24_7: boolean; averageRating: number;
  status: string; distanceKm: number | null;
}

export interface ParkingBounds {
  minLat: number; maxLat: number; minLng: number; maxLng: number;
  userLat?: number; userLng?: number;
}
