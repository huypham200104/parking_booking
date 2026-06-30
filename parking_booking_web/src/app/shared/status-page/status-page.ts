import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({ selector: 'app-status-page', standalone: true, imports: [RouterLink], templateUrl: './status-page.html', styleUrl: './status-page.scss' })
export class StatusPageComponent {
  private readonly route = inject(ActivatedRoute);
  readonly code = this.route.snapshot.data['code'] ?? '404';
  readonly title = this.code === '403' ? 'Bạn không có quyền truy cập' : 'Không tìm thấy trang';
  readonly message = this.code === '403'
    ? 'Tài khoản hiện tại không được cấp quyền cho khu vực này.'
    : 'Đường dẫn bạn truy cập không tồn tại hoặc đã được thay đổi.';
}
