import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { Wallet } from '../../../../../core/infrastructure/models/api.models';

@Component({ selector: 'app-wallet', standalone: true, imports: [FormsModule], templateUrl: './wallet.html', styleUrl: './wallet.scss' })
export class WalletComponent {
  private readonly api = inject(ParkingBookingApiService);
  readonly wallet = signal<Wallet | null>(null); readonly isLoading = signal(true); readonly isDepositing = signal(false); readonly errorMessage = signal(''); readonly depositQrUrl = signal('');
  amount = 100000;
  constructor() { this.load(); }
  load(): void { this.isLoading.set(true); this.api.getWallet().pipe(finalize(() => this.isLoading.set(false))).subscribe({ next: value => this.wallet.set(value), error: e => this.errorMessage.set(e.message) }); }
  deposit(): void { if (this.amount < 10000 || this.amount > 50000000) { this.errorMessage.set('Số tiền phải từ 10.000đ đến 50.000.000đ.'); return; } this.isDepositing.set(true); this.errorMessage.set(''); this.api.deposit(this.amount).pipe(finalize(() => this.isDepositing.set(false))).subscribe({ next: result => this.depositQrUrl.set(result.vietQrUrl), error: e => this.errorMessage.set(e.message) }); }
  money(value: number): string { return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value); }
  typeLabel(type: string | number): string { return ['Nạp tiền', 'Rút tiền', 'Thanh toán'][Number(type)] ?? String(type); }
}
