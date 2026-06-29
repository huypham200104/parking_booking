import { Component, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { Voucher } from '../../../../../core/infrastructure/models/api.models';
@Component({selector:'app-voucher-list',standalone:true,templateUrl:'./voucher-list.html',styleUrl:'./voucher-list.scss'})
export class VoucherListComponent { private api=inject(ParkingBookingApiService); readonly vouchers=signal<Voucher[]>([]); readonly loading=signal(true); readonly error=signal(''); readonly copied=signal(''); constructor(){this.load()} load(){this.loading.set(true);this.api.getValidVouchers().pipe(finalize(()=>this.loading.set(false))).subscribe({next:v=>this.vouchers.set(v),error:e=>this.error.set(e.message)})} copy(code:string){navigator.clipboard.writeText(code);this.copied.set(code);window.setTimeout(()=>this.copied.set(''),1500)} money(v:number){return new Intl.NumberFormat('vi-VN').format(v)+'đ'} }
