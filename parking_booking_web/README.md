# ParkGo - Hệ thống Đặt chỗ Bãi Đỗ Xe (Web CMS)

Đây là mã nguồn Frontend (Web CMS) cho dự án ParkGo - nền tảng kết nối tài xế với các bãi đỗ xe thông minh. Dự án được xây dựng dựa trên [Angular](https://angular.dev/) và cung cấp giao diện cho 4 nhóm người dùng: **Admin**, **Owner (Chủ bãi)**, **Driver (Tài xế)** và **Guard (Bảo vệ)**.

## Công nghệ sử dụng
- **Core:** Angular (Standalone Components), TypeScript.
- **Style:** SCSS, CSS Variables (Custom Design System, không dùng Bootstrap/Tailwind để tối ưu dung lượng).
- **Bản đồ:** Leaflet.js (Hiển thị và tương tác với sơ đồ bãi đỗ xe).
- **State Management:** Angular Signals & RxJS.

## Cấu trúc thư mục (`/src/app`)
- `/core`: Chứa các interceptors, guards, utils dùng chung toàn hệ thống.
- `/features`: Chứa logic nghiệp vụ chia theo tính năng (Admin, Owner, Users, Booking, Wallet,...). Mỗi feature tuân theo kiến trúc Domain-Driven Design (Presentation, Application, Domain, Data).
- `/shell`: Chứa cấu trúc Layout chính (Navbar, Sidebar, Footer, Layout component).

## Hướng dẫn Cài đặt & Khởi chạy

### 1. Cài đặt thư viện
Yêu cầu bạn đã cài đặt Node.js (phiên bản khuyến nghị >= 18). Mở terminal tại thư mục `parking_booking_web` và chạy:
```bash
npm install
```

### 2. Cấu hình kết nối Backend (Proxy)
Dự án sử dụng file `proxy.conf.json` để điều hướng các request API từ frontend sang backend .NET.
Mặc định frontend chạy ở `http://localhost:4200` và chuyển tiếp `/api/*` sang `http://localhost:5242`.
*Đảm bảo Backend (.NET) của bạn đang chạy ở port 5242 trước khi test gọi API.*

### 3. Chạy môi trường Development
```bash
npm start
```
(Lệnh này tương đương với `ng serve --proxy-config proxy.conf.json`). Sau khi khởi chạy thành công, truy cập `http://localhost:4200` trên trình duyệt. App sẽ tự động tải lại (hot-reload) khi có file thay đổi.

## Xây dựng bản Production (Build)
Để biên dịch dự án ra các file tĩnh chuẩn bị cho việc deploy (ví dụ lên Vercel, Netlify hoặc IIS):
```bash
npm run build
```
Mã nguồn đã biên dịch sẽ nằm trong thư mục `dist/parking_booking_web/`.

## Lệnh tạo Code tự động (Scaffolding)
Sử dụng Angular CLI để tạo nhanh các component:
```bash
ng generate component path/to/component-name
```
