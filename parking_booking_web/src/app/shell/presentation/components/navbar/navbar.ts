import { Component, HostListener, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AccountFacade } from '../../../../features/account/application/account.facade';
import { CurrentUser } from '../../../../features/account/domain/entities/current-user';

/**
 * NavbarComponent – navigation UI.
 */
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class NavbarComponent {
  private readonly router = inject(Router);
  private readonly account = inject(AccountFacade);

  readonly isScrolled = signal(false);
  readonly isMobileMenuOpen = signal(false);
  readonly showLogoutToast = signal(false);
  readonly currentUser = signal<CurrentUser | null>(null);
  readonly isAuthenticated = this.account.isAuthenticated;
  readonly initials = computed(() => this.currentUser()?.fullName
    .split(/\s+/).filter(Boolean).slice(-2).map(part => part[0]).join('').toUpperCase() || 'PG');

  readonly navLinks = [
    { label: 'Tìm bãi đỗ', path: '/map' },
    { label: 'Đặt chỗ', path: '/booking' },
    { label: 'Lịch sử', path: '/history' },
  ] as const;

  constructor() {
    effect(() => {
      if (!this.isAuthenticated()) {
        this.currentUser.set(null);
        return;
      }
      if (!this.currentUser()) {
        this.account.getCurrentUser().subscribe({ next: user => this.currentUser.set(user) });
      }
    });
  }

  isSolid(): boolean {
    return this.isScrolled()
      || this.router.url.startsWith('/auth/')
      || this.router.url.startsWith('/map');
  }

  @HostListener('window:scroll')
  onScroll(): void {
    this.isScrolled.set(window.scrollY > 20);
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen.update((v) => !v);
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen.set(false);
  }

  logout(): void {
    this.account.logout();
    this.closeMobileMenu();
    this.showLogoutToast.set(true);
    window.setTimeout(() => this.showLogoutToast.set(false), 3000);
    void this.router.navigateByUrl('/');
  }
}
