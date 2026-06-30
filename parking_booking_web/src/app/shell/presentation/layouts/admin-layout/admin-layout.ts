import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { inject } from '@angular/core';
import { AccountFacade } from '../../../../features/account/application/account.facade';
import { NotificationCenterComponent } from '../../../../shared/notification-center/notification-center';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificationCenterComponent],
  templateUrl: './admin-layout.html',
  styleUrls: ['./admin-layout.scss']
})
export class AdminLayoutComponent {
  private readonly account = inject(AccountFacade);
  private readonly router = inject(Router);

  logout(): void {
    this.account.logout();
  }
}
