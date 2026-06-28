import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Stat {
  value: string;
  label: string;
  suffix?: string;
}

/**
 * StatsStripComponent – SRP: Displays social proof stats.
 * <!-- mock: numbers below are sample data, replace with API -->
 */
@Component({
  selector: 'app-stats-strip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './stats-strip.html',
  styleUrl: './stats-strip.scss',
})
export class StatsStripComponent {
  /* mock data – replace with real API values */
  readonly stats: Stat[] = [
    { value: '200+', label: 'Bãi đỗ xe đối tác' },
    { value: '50K+', label: 'Lượt đặt thành công' },
    { value: '12', label: 'Quận / Tỉnh thành' },
    { value: '4.8', label: 'Đánh giá trung bình', suffix: '★' },
  ];
}
