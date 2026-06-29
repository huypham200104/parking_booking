import { Component } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { NavbarComponent } from '../../components/navbar/navbar';
import { FooterComponent } from '../../components/footer/footer';

/**
 * MainLayoutComponent – OCP: Shell that wraps all user-facing pages.
 * Adding new pages = adding new routes, no changes needed here.
 */
@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, FooterComponent],
  template: `
    <app-navbar />
    <main>
      <router-outlet />
    </main>
    @if (shouldShowFooter()) {
      <app-footer />
    }
  `,
  styles: [`
    main {
      min-height: 100dvh;
    }
  `],
})
export class MainLayoutComponent {
  constructor(private readonly router: Router) {}

  shouldShowFooter(): boolean {
    return !this.router.url.startsWith('/auth/') && !this.router.url.startsWith('/map') && !this.router.url.startsWith('/staff');
  }
}
