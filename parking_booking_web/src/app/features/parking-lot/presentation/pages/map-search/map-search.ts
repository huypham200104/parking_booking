import { Component, OnInit, AfterViewInit, Inject, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { GetParkingLotsInBoundsUseCase } from '../../../application/use-cases/get-parking-lots-in-bounds.use-case';
import { ParkingLotSummary } from '../../../domain/entities/parking-lot';

interface UserLocation {
  lat: number;
  lng: number;
  source: 'browser' | 'fallback';
}

@Component({
  selector: 'app-map-search',
  imports: [RouterLink],
  templateUrl: './map-search.html',
  styleUrl: './map-search.scss',
})
export class MapSearch implements OnInit, AfterViewInit {
  private readonly getParkingLotsInBounds = inject(GetParkingLotsInBoundsUseCase);
  private readonly fallbackLocation: UserLocation = { lat: 21.0285, lng: 105.8542, source: 'fallback' };
  private map: any;
  private L: any;
  private markerLayer: any;
  private userMarker: any;
  private loadBoundsTimer: number | undefined;

  readonly parkingLots = signal<ParkingLotSummary[]>([]);
  readonly isLoading = signal(true);
  readonly isListOpen = signal(false);
  readonly locationMessage = signal('Đang lấy vị trí của bạn...');
  readonly userLocation = signal<UserLocation | null>(null);

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}

  ngOnInit(): void {}

  async ngAfterViewInit() {
    if (isPlatformBrowser(this.platformId)) {
      // Dynamic import to avoid SSR issues with leaflet
      this.L = await import('leaflet');
      this.initMap();
      this.loadNearbyFromUserLocation();
    }
  }

  private initMap(): void {
    this.map = this.L.map('map').setView([this.fallbackLocation.lat, this.fallbackLocation.lng], 15);
    this.markerLayer = this.L.layerGroup().addTo(this.map);

    this.L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '© OpenStreetMap'
    }).addTo(this.map);

    this.map.on('moveend', () => this.scheduleLoadVisibleParkingLots());
    this.fixLeafletDefaultIcon();
  }

  useMyLocation(): void {
    this.isLoading.set(true);
    this.locationMessage.set('Đang lấy vị trí của bạn...');
    this.moveToUserLocation();
  }

  moveToUserLocation(): void {
    const knownLocation = this.userLocation();
    if (knownLocation?.source === 'browser') {
      this.map.flyTo([knownLocation.lat, knownLocation.lng], 16, { duration: 0.7 });
      return;
    }

    this.loadNearbyFromUserLocation();
  }

  toggleParkingList(): void {
    this.isListOpen.update((value) => !value);
  }

  focusParkingLot(lot: ParkingLotSummary): void {
    if (!this.map) {
      return;
    }

    this.map.setView([lot.latitude, lot.longitude], 17);
  }

  private loadNearbyFromUserLocation(): void {
    if (!navigator.geolocation) {
      console.error('[MapSearch] Geolocation is not supported by this browser.');
      this.moveMapToLocation(this.fallbackLocation, 'Trình duyệt không hỗ trợ định vị. Đang dùng vị trí mặc định tại Hà Nội.');
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        this.moveMapToLocation(
          {
            lat: position.coords.latitude,
            lng: position.coords.longitude,
            source: 'browser',
          },
          'Đang hiển thị các bãi xe gần vị trí hiện tại của bạn.'
        );
      },
      (error) => {
        console.error('[MapSearch] Failed to get user geolocation.', {
          code: error.code,
          message: error.message,
          detail: error,
        });
        this.moveMapToLocation(this.fallbackLocation, 'Bạn chưa cấp quyền vị trí. Đang dùng vị trí mặc định tại Hà Nội.');
      },
      { enableHighAccuracy: true, timeout: 8000, maximumAge: 60000 }
    );
  }

  private moveMapToLocation(location: UserLocation, message: string): void {
    this.userLocation.set(location);
    this.locationMessage.set(message);
    this.renderUserLocation(location);
    this.map.flyTo([location.lat, location.lng], 16, { duration: 0.7 });
    this.loadVisibleParkingLots();
  }

  private scheduleLoadVisibleParkingLots(): void {
    window.clearTimeout(this.loadBoundsTimer);
    this.loadBoundsTimer = window.setTimeout(() => this.loadVisibleParkingLots(), 180);
  }

  private loadVisibleParkingLots(): void {
    if (!this.map || !this.markerLayer) {
      return;
    }

    const bounds = this.map.getBounds();
    const southWest = bounds.getSouthWest();
    const northEast = bounds.getNorthEast();
    const userLocation = this.userLocation();

    this.isLoading.set(true);
    this.getParkingLotsInBounds.execute({
      minLat: southWest.lat,
      maxLat: northEast.lat,
      minLng: southWest.lng,
      maxLng: northEast.lng,
      userLat: userLocation?.source === 'browser' ? userLocation.lat : undefined,
      userLng: userLocation?.source === 'browser' ? userLocation.lng : undefined,
    }).subscribe({
      next: (parkingLots) => {
        this.parkingLots.set(parkingLots);
        this.renderParkingLots(parkingLots);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('[MapSearch] Failed to load parking lots in current map bounds.', {
          requestBounds: {
            minLat: southWest.lat,
            maxLat: northEast.lat,
            minLng: southWest.lng,
            maxLng: northEast.lng,
          },
          status: error?.status,
          statusText: error?.statusText,
          message: error?.message,
          url: error?.url,
          error: error?.error,
          raw: error,
        });
        this.parkingLots.set([]);
        this.markerLayer.clearLayers();
        this.isLoading.set(false);
      },
    });
  }

  private fixLeafletDefaultIcon(): void {
    const iconRetinaUrl = 'assets/marker-icon-2x.png';
    const iconUrl = 'assets/marker-icon.png';
    const shadowUrl = 'assets/marker-shadow.png';
    const DefaultIcon = this.L.icon({
      iconUrl: iconUrl,
      iconRetinaUrl: iconRetinaUrl,
      shadowUrl: shadowUrl,
      iconSize: [25, 41],
      iconAnchor: [12, 41],
      popupAnchor: [1, -34],
      tooltipAnchor: [16, -28],
      shadowSize: [41, 41]
    });
    this.L.Marker.prototype.options.icon = DefaultIcon;
  }

  private renderUserLocation(location: UserLocation): void {
    if (this.userMarker) {
      this.userMarker.remove();
    }

    const userIcon = this.L.divIcon({
      className: 'custom-marker',
      html: '<div class="user-location-marker"></div>',
      iconSize: [22, 22],
      iconAnchor: [11, 11],
    });

    this.userMarker = this.L.marker([location.lat, location.lng], { icon: userIcon })
      .addTo(this.map)
      .bindPopup(location.source === 'browser' ? 'Vị trí của bạn' : 'Vị trí mặc định: Hồ Hoàn Kiếm');
  }

  private renderParkingLots(parkingLots: ParkingLotSummary[]): void {
    this.markerLayer.clearLayers();

    parkingLots.forEach((lot) => {
      const icon = this.createParkingIcon(lot.availableSlots > 0);
      const marker = this.L.marker([lot.latitude, lot.longitude], { icon }).addTo(this.markerLayer);
      const distance = lot.distanceKm == null ? '' : `<p><strong>${lot.distanceKm.toFixed(2)}km</strong> từ bạn</p>`;
      const statusHtml = lot.availableSlots > 0
        ? `<span class="popup-status popup-status--available">Còn ${lot.availableSlots}/${lot.totalSlots} chỗ</span>`
        : '<span class="popup-status popup-status--full">Đã hết chỗ</span>';

      marker.bindPopup(`
        <div class="parking-popup">
          <h3>${this.escapeHtml(lot.name)}</h3>
          <p>${this.escapeHtml(lot.address)}</p>
          ${distance}
          ${statusHtml}
        </div>
      `);
    });
  }

  private createParkingIcon(hasAvailableSlots: boolean): any {
    return this.L.divIcon({
      className: 'custom-marker',
      html: `<div class="parking-marker ${hasAvailableSlots ? 'parking-marker--available' : 'parking-marker--full'}">P</div>`,
      iconSize: [30, 30],
      iconAnchor: [15, 15]
    });
  }

  private escapeHtml(value: string): string {
    return value
      .replaceAll('&', '&amp;')
      .replaceAll('<', '&lt;')
      .replaceAll('>', '&gt;')
      .replaceAll('"', '&quot;')
      .replaceAll("'", '&#039;');
  }
}
