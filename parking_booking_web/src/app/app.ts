import { Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AccountFacade } from './features/account/application/account.facade';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  readonly account = inject(AccountFacade);
  readonly router = inject(Router);

  closeLogoutModal(): void {
    this.account.showLogoutModal.set(false);
    void this.router.navigateByUrl('/');
  }
}
