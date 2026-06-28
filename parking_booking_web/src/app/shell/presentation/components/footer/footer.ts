import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * FooterComponent – SRP: Only renders footer content and links.
 */
@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './footer.html',
  styleUrl: './footer.scss',
})
export class FooterComponent {
  readonly currentYear = new Date().getFullYear();

  readonly footerLinks = [
    {
      group: 'Dịch vụ',
      links: [
        { label: 'Tìm bãi đỗ xe', path: '/parking-lots' },
        { label: 'Đặt chỗ online', path: '/booking' },
        { label: 'Vào/Ra bằng QR', path: '/qr-gate' },
        { label: 'Thanh toán', path: '/payment' },
      ],
    },
    {
      group: 'Công ty',
      links: [
        { label: 'Về chúng tôi', path: '/about' },
        { label: 'Đối tác bãi đỗ', path: '/partners' },
        { label: 'Tuyển dụng', path: '/careers' },
        { label: 'Blog', path: '/blog' },
      ],
    },
    {
      group: 'Hỗ trợ',
      links: [
        { label: 'Trung tâm trợ giúp', path: '/help' },
        { label: 'Liên hệ', path: '/contact' },
        { label: 'Điều khoản dịch vụ', path: '/terms' },
        { label: 'Chính sách bảo mật', path: '/privacy' },
      ],
    },
  ] as const;
}
