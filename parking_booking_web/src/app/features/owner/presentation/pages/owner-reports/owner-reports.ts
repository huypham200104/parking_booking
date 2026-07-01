import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ParkingBookingApiService } from '../../../../../core/infrastructure/http/parking-booking-api.service';
import { StaffBooking } from '../../../../../core/infrastructure/models/api.models';
@Component({selector:'app-owner-reports',standalone:true,imports:[CommonModule],templateUrl:'./owner-reports.html',styleUrl:'./owner-reports.scss'})
export class OwnerReports implements OnInit{private api=inject(ParkingBookingApiService);private cdr=inject(ChangeDetectorRef);bookings:StaffBooking[]=[];loading=true;error='';get revenue(){return this.bookings.reduce((sum,x)=>sum+(x.totalPrice??0),0)}get completed(){return this.bookings.filter(x=>x.status===2).length}get recentTransactions(){return this.bookings.filter(x=>x.status===2).slice(0,8)}ngOnInit(){this.api.getOwnerBookings(1,100).subscribe({next:data=>{this.bookings=data.items;this.loading=false;this.cdr.markForCheck()},error:e=>{this.error=e.message;this.loading=false;this.cdr.markForCheck()}})}}
