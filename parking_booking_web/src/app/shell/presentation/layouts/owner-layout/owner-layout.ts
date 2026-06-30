import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { inject } from '@angular/core';
import { AccountFacade } from '../../../../features/account/application/account.facade';
import { NotificationCenterComponent } from '../../../../shared/notification-center/notification-center';

@Component({
  selector: 'app-owner-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificationCenterComponent],
  templateUrl: './owner-layout.html',
  styleUrls: ['./owner-layout.scss']
})
export class OwnerLayoutComponent {
  private readonly account = inject(AccountFacade);
  private readonly router = inject(Router);

  logout(): void {
    this.account.logout();
  }
}
