import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Feature {
  id: string;
  icon: string; // SVG path data
  title: string;
  description: string;
  accent: string;
  span?: 'wide' | 'tall';
}

/**
 * FeaturesSectionComponent – SRP: Renders bento grid of product features.
 * OCP: Add features by extending the data array, not modifying the template.
 */
@Component({
  selector: 'app-features-section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './features-section.html',
  styleUrl: './features-section.scss',
})
export class FeaturesSectionComponent {
  readonly features: Feature[] = [
    {
      id: 'search',
      icon: 'M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z M12 10m-3 0a3 3 0 1 0 6 0 3 3 0 1 0-6 0',
      title: 'Tìm kiếm thông minh',
      description: 'Tìm bãi đỗ gần nhất theo địa điểm, giá, hoặc tiện ích. Bản đồ real-time cập nhật liên tục.',
      accent: '#3b82f6',
      span: 'wide',
    },
    {
      id: 'qr',
      icon: 'M3 3h7v7H3z M14 3h7v7h-7z M3 14h7v7H3z M14 14h3v3h-3z M17 17h3v3h-3z',
      title: 'Vào bãi bằng QR',
      description: 'Quét mã QR để mở cổng tự động. Không cần vé giấy, không cần chờ đợi.',
      accent: '#10b981',
    },
    {
      id: 'realtime',
      icon: 'M22 12h-4l-3 9L9 3l-3 9H2',
      title: 'Trạng thái real-time',
      description: 'Xem chỗ trống, chỗ đã đặt ngay tức thì qua SignalR.',
      accent: '#f59e0b',
    },
    {
      id: 'payment',
      icon: 'M2 10h20 M6 6h.01 M10 6h.01 M2 6a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V6z',
      title: 'Thanh toán online',
      description: 'Thanh toán qua ví điện tử, thẻ ngân hàng, hoặc QR ngân hàng. Lịch sử đầy đủ.',
      accent: '#8b5cf6',
      span: 'wide',
    },
    {
      id: 'booking',
      icon: 'M8 2v4 M16 2v4 M3 10h18 M5 4h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2z',
      title: 'Đặt trước linh hoạt',
      description: 'Đặt chỗ theo giờ, theo ngày. Huỷ dễ dàng trước giờ vào.',
      accent: '#0ea5e9',
    },
  ];

  /** Mock slot states for the real-time visual: available | occupied | pending */
  readonly slotStates = [
    'available', 'available', 'occupied', 'available', 'pending',
    'occupied',  'available', 'available', 'occupied', 'available',
    'available', 'pending',
  ] as const;
}
