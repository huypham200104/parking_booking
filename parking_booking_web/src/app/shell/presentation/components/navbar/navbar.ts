import { Component, HostListener, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AccountFacade } from '../../../../features/account/application/account.facade';
import { CurrentUser } from '../../../../features/account/domain/entities/current-user';
import { NotificationCenterComponent } from '../../../../shared/notification-center/notification-center';

/**
 * NavbarComponent – navigation UI.
 */
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive, NotificationCenterComponent],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class NavbarComponent {
  private readonly router = inject(Router);
  private readonly account = inject(AccountFacade);

  readonly isScrolled = signal(false);
  readonly isMobileMenuOpen = signal(false);
  readonly isEditProfileOpen = signal(false);
  readonly isSavingProfile = signal(false);
  readonly profileMessage = signal('');
  readonly profileError = signal('');
  editFullName = '';
  editPhoneNumber = '';
  readonly currentUser = signal<CurrentUser | null>(null);
  readonly isAuthenticated = this.account.isAuthenticated;
  readonly initials = computed(() => this.currentUser()?.fullName
    .split(/\s+/).filter(Boolean).slice(-2).map(part => part[0]).join('').toUpperCase() || 'PG');
  readonly isStaff = computed(() => this.currentUser()?.role === 2);
  readonly homeLink = computed(() => this.isStaff() ? '/staff' : '/');

  readonly navLinks = computed(() => this.isStaff()
    ? [{ label: 'Vận hành bãi xe', path: '/staff' }]
    : [
        { label: 'Tìm bãi đỗ', path: '/map' },
        { label: 'Đặt chỗ', path: '/booking' },
        { label: 'Lịch sử', path: '/history' },
      ]);

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
      || this.router.url.startsWith('/map')
      || this.router.url.startsWith('/staff');
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

  openEditProfile(): void {
    const user = this.currentUser();
    if (!user || this.isStaff()) return;
    this.editFullName = user.fullName;
    this.editPhoneNumber = user.phoneNumber;
    this.profileMessage.set('');
    this.profileError.set('');
    this.isEditProfileOpen.set(true);
    this.closeMobileMenu();
  }

  closeEditProfile(): void {
    if (!this.isSavingProfile()) this.isEditProfileOpen.set(false);
  }

  saveProfile(): void {
    const fullName = this.editFullName.trim();
    if (fullName.length < 2) {
      this.profileError.set('Vui lòng nhập họ tên hợp lệ.');
      return;
    }

    this.isSavingProfile.set(true);
    this.profileError.set('');
    this.profileMessage.set('');
    this.account.updateCurrentUser({ fullName }).subscribe({
      next: (user) => {
        this.currentUser.set(user);
        this.isSavingProfile.set(false);
        this.profileMessage.set('Thông tin cá nhân đã được cập nhật.');
      },
      error: (error) => {
        this.isSavingProfile.set(false);
        this.profileError.set(error?.message ?? 'Không thể cập nhật thông tin. Vui lòng thử lại.');
      },
    });
  }

  logout(): void {
    this.account.logout();
    this.closeMobileMenu();
  }
}
