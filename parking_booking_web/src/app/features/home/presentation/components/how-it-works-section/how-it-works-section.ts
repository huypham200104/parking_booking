import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Step {
  number: string;
  title: string;
  description: string;
  bullets: string[];
}

/**
 * HowItWorksSectionComponent – SRP: Explains the 3-step booking flow.
 */
@Component({
  selector: 'app-how-it-works-section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './how-it-works-section.html',
  styleUrl: './how-it-works-section.scss',
})
export class HowItWorksSectionComponent {
  readonly steps: Step[] = [
    {
      number: '01',
      title: 'Tìm bãi đỗ phù hợp',
      description:
        'Nhập địa điểm hoặc bật GPS. ParkGo hiển thị tất cả bãi đỗ gần bạn với giá và số chỗ trống real-time.',
      bullets: ['Lọc theo giá, khoảng cách', 'Xem ảnh và tiện ích bãi đỗ', 'So sánh nhiều bãi cùng lúc'],
    },
    {
      number: '02',
      title: 'Đặt chỗ & thanh toán online',
      description:
        'Chọn chỗ, chọn thời gian vào/ra, thanh toán online trong vài giây. Nhận mã QR xác nhận ngay lập tức.',
      bullets: ['Đặt trước tối đa 7 ngày', 'Thanh toán MoMo, ZaloPay, thẻ ngân hàng', 'Xác nhận tức thì qua email/SMS'],
    },
    {
      number: '03',
      title: 'Quét QR – Vào bãi tự động',
      description:
        'Đến bãi đỗ, mở app quét mã QR để cổng tự mở. Ra về cũng tương tự. Không cần vé giấy, không xếp hàng.',
      bullets: ['Cổng tự động mở trong 2 giây', 'Không cần nhân viên check-in', 'Lưu lịch sử đầy đủ trong app'],
    },
  ];
}
