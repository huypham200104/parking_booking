import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Testimonial {
  quote: string;
  name: string;
  role: string;
  initials: string;
  color: string;
}

/**
 * TestimonialsSectionComponent – SRP: Displays user reviews.
 * <!-- mock: testimonials below are sample data -->
 */
@Component({
  selector: 'app-testimonials-section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './testimonials-section.html',
  styleUrl: './testimonials-section.scss',
})
export class TestimonialsSectionComponent {
  /* mock – replace with API response */
  readonly testimonials: Testimonial[] = [
    {
      quote:
        'Không còn lo lắng tìm chỗ đậu mỗi sáng. ParkGo giúp tôi đặt chỗ từ nhà, đến nơi chỉ cần quét QR là vào. Tiết kiệm cả 15 phút mỗi ngày.',
      name: 'Nguyễn Thanh Hà',
      role: 'Nhân viên văn phòng, Quận 1',
      initials: 'NH',
      color: '#3b82f6',
    },
    {
      quote:
        'Ứng dụng rất mượt, giao diện đẹp. Đặt chỗ cho ô tô ở sân bay cực kỳ dễ, giá cũng hiển thị rõ ràng. Hoàn tiền khi huỷ rất nhanh.',
      name: 'Trần Minh Đức',
      role: 'Doanh nhân, TP. HCM',
      initials: 'MD',
      color: '#10b981',
    },
    {
      quote:
        'Trước đây tôi hay bị chặt chém phí giữ xe. Từ khi dùng ParkGo, giá niêm yết rõ ràng, không phát sinh thêm. Rất yên tâm.',
      name: 'Lê Thị Kim Anh',
      role: 'Giáo viên, Hà Nội',
      initials: 'KA',
      color: '#8b5cf6',
    },
  ];
}
