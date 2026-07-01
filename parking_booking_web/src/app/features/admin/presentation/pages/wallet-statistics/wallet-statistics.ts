import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { AdminUserWallet, AdminWalletStats } from '../../../../../core/infrastructure/models/api.models';

@Component({ selector: 'app-wallet-statistics', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './wallet-statistics.html', styleUrl: './wallet-statistics.scss' })
export class WalletStatistics implements OnInit {
  private readonly api = inject(ParkingBookingApiService); private readonly cdr = inject(ChangeDetectorRef);
  stats: AdminWalletStats | null = null; wallets: AdminUserWallet[] = []; pageIndex=1; pageSize=10; totalPages=0; totalCount=0; keyword=''; loading=true; error='';
  ngOnInit(): void { this.load(); }
  load(): void { this.loading=true; this.error=''; this.api.getAdminWalletStats().subscribe({next:stats=>{this.stats=stats;this.cdr.markForCheck()},error:e=>{this.error=e.message;this.loading=false;this.cdr.markForCheck()}}); this.api.getAdminUserWallets(this.pageIndex,this.pageSize,this.keyword).subscribe({next:data=>{this.wallets=data.items;this.pageIndex=data.pageIndex;this.totalPages=data.totalPages;this.totalCount=data.totalCount;this.loading=false;this.cdr.markForCheck()},error:e=>{this.error=e.message;this.loading=false;this.cdr.markForCheck()}}); }
  search():void{this.pageIndex=1;this.load()} go(page:number):void{if(page<1||page>this.totalPages||page===this.pageIndex)return;this.pageIndex=page;this.load()}
}
