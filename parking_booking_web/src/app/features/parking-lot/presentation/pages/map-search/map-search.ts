import { Component, OnInit, AfterViewInit, Inject, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { GetParkingLotsInBoundsUseCase } from '../../../application/use-cases/get-parking-lots-in-bounds.use-case';
import { SearchParkingLotsUseCase } from '../../../application/use-cases/search-parking-lots.use-case';
import { MapBounds, ParkingLotSummary } from '../../../domain/entities/parking-lot';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';

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
  private readonly searchParkingLots = inject(SearchParkingLotsUseCase);
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ParkingBookingApiService);
  private readonly fallbackLocation: UserLocation = { lat: 21.0285, lng: 105.8542, source: 'fallback' };
  private map: any;
  private L: any;
  private markerLayer: any;
  private userMarker: any;
  private loadBoundsTimer: number | undefined;

  readonly parkingLots = signal<ParkingLotSummary[]>([]);
  readonly favouriteIds = signal<Set<string>>(new Set());
  readonly searchInput = signal('');
  readonly searchQuery = signal('');
  readonly searchMessage = signal('');
  readonly filteredParkingLots = computed(() => {
    const parkingLots = this.parkingLots() ?? [];
    const queryTokens = this.searchTokens(this.searchQuery());
    const filtered = !queryTokens.length ? parkingLots : parkingLots.filter(lot => {
      const searchable = this.expandVietnameseAliases(this.normalizeSearch(`${lot.name} ${lot.address}`));
      return queryTokens.every(token => searchable.includes(token));
    });
    const favourites = this.favouriteIds();
    return [...filtered].sort((a, b) => Number(favourites.has(b.id)) - Number(favourites.has(a.id)));
  });
  readonly selectedParkingLotId = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly isListOpen = signal(false);
  readonly locationMessage = signal('Đang lấy vị trí của bạn...');
  readonly userLocation = signal<UserLocation | null>(null);

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}

  ngOnInit(): void {
    this.api.getFavouriteParkingLots().subscribe({ next: lots => this.favouriteIds.set(new Set(lots.map(lot => lot.id))) });
  }

  isFavourite(id: string): boolean { return this.favouriteIds().has(id); }
  toggleFavourite(lot: ParkingLotSummary, event: Event): void {
    event.stopPropagation();
    const isFavourite = this.isFavourite(lot.id);
    const request = isFavourite ? this.api.removeFavouriteParkingLot(lot.id) : this.api.addFavouriteParkingLot(lot.id);
    request.subscribe({ next: () => this.favouriteIds.update(ids => { const next = new Set(ids); isFavourite ? next.delete(lot.id) : next.add(lot.id); return next; }) });
  }

  async ngAfterViewInit() {
    if (isPlatformBrowser(this.platformId)) {
      // Dynamic import to avoid SSR issues with leaflet
      this.L = await import('leaflet');
      this.initMap();
      const initialQuery = this.route.snapshot.queryParamMap.get('q')?.trim();
      if (initialQuery) {
        this.searchInput.set(initialQuery);
        this.performSearch();
      } else {
        this.loadNearbyFromUserLocation();
      }
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

  updateSearch(value: string): void {
    this.searchInput.set(value);
    if (!value.trim() && this.searchQuery()) this.clearSearch();
  }

  performSearch(): void {
    const query = this.searchInput().trim();
    if (!query) {
      this.searchMessage.set('Vui lòng nhập tên hoặc địa chỉ bãi đỗ cần tìm.');
      return;
    }
    this.searchQuery.set(query);
    this.isLoading.set(true);
    this.isListOpen.set(true);
    this.searchMessage.set('Đang tìm kiếm bãi đỗ...');

    this.searchParkingLots.execute(query).subscribe({
      next: (response) => {
        const results = Array.isArray(response?.results) ? response.results : [];
        const mapBounds = response?.mapBounds ?? null;

        if (!Array.isArray(response?.results)) {
          console.error('[MapSearch] Search response does not contain a valid results array.', response);
        }

        this.parkingLots.set(results);
        this.renderParkingLots(results);
        this.isLoading.set(false);
        this.searchMessage.set(results.length
          ? `Tìm thấy ${results.length} bãi đỗ phù hợp với “${query}”.`
          : `Không tìm thấy bãi đỗ phù hợp với “${query}”.`);

        if (mapBounds) this.fitMapToSearchResults(mapBounds);
      },
      error: (error) => {
        console.error('[MapSearch] Failed to search parking lots.', error);
        this.isLoading.set(false);
        this.searchMessage.set('Không thể tìm kiếm bãi đỗ. Vui lòng thử lại.');
      },
    });
  }

  openParkingList(): void {
    this.isListOpen.set(true);
  }

  clearSearch(): void {
    this.searchInput.set('');
    this.searchQuery.set('');
    this.searchMessage.set('');
    this.loadVisibleParkingLots();
  }

  focusParkingLot(lot: ParkingLotSummary): void {
    if (!this.map) {
      return;
    }

    this.selectedParkingLotId.set(lot.id);
    this.map.setView([lot.latitude, lot.longitude], 17);
  }

  googleMapsUrl(lot: ParkingLotSummary): string {
    const destination = `${lot.latitude},${lot.longitude}`;
    return `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(destination)}`;
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
    if (this.searchQuery()) return;
    window.clearTimeout(this.loadBoundsTimer);
    this.loadBoundsTimer = window.setTimeout(() => this.loadVisibleParkingLots(), 180);
  }

  private fitMapToSearchResults(bounds: MapBounds): void {
    const leafletBounds = this.L.latLngBounds(
      [bounds.minLat, bounds.minLng],
      [bounds.maxLat, bounds.maxLng],
    );
    this.map.fitBounds(leafletBounds, { padding: [50, 50], maxZoom: 17 });
  }

  private loadVisibleParkingLots(): void {
    if (!this.map || !this.markerLayer || this.searchQuery()) {
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
        this.renderParkingLots(this.filteredParkingLots());
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
      marker.on('click', () => {
        this.selectedParkingLotId.set(lot.id);
        this.isListOpen.set(true);
      });
      const statusHtml = lot.availableSlots > 0
        ? `<span class="popup-status popup-status--available">Còn ${lot.availableSlots}/${lot.totalSlots} chỗ</span>`
        : '<span class="popup-status popup-status--full">Đã hết chỗ</span>';

      marker.bindPopup(`
        <div class="parking-popup">
          <h3>${this.escapeHtml(lot.name)}</h3>
          <p>${this.escapeHtml(lot.address)}</p>
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

  private normalizeSearch(value: string): string {
    return value.trim().toLocaleLowerCase('vi-VN').normalize('NFD').replace(/[\u0300-\u036f]/g, '');
  }

  private searchTokens(value: string): string[] {
    const ignoredWords = new Set(['bai', 'xe', 'do', 'giu', 'ham', 'tai', 'gan', 'duong', 'khu']);
    const normalized = this.replaceVietnameseAliases(this.normalizeSearch(value));
    return normalized.split(/\s+/).filter(token => token && !ignoredWords.has(token));
  }

  private readonly searchAliases: ReadonlyArray<readonly [string, string]> = [
    ['nha tho duc ba', 'notre dame'],
    ['dinh doc lap', 'independence palace'],
    ['nha hat thanh pho', 'opera house'],
    ['cong vien tao dan', 'tao dan park'],
    ['cong vien 23 thang 9', 'september 23 park'],
    ['cho ben thanh', 'ben thanh market'],
    ['pho di bo nguyen hue', 'nguyen hue walking street'],
    ['toa nha bitexco', 'bitexco financial tower'],
  ];

  private replaceVietnameseAliases(value: string): string {
    return this.searchAliases.reduce((result, [vietnamese, english]) =>
      result.replaceAll(vietnamese, english), value);
  }

  private expandVietnameseAliases(value: string): string {
    return this.searchAliases.reduce((result, [vietnamese, english]) =>
      result.includes(vietnamese) ? `${result} ${english}` : result, value);
  }
}
