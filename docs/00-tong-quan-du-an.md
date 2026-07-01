# Tổng Quan Dự Án: Hệ Thống Đặt Chỗ Đỗ Xe (ParkGo / Parking Booking)

## 1. Giới thiệu chung
Dự án là một nền tảng công nghệ toàn diện giúp kết nối tài xế ô tô có nhu cầu đỗ xe với các bãi đỗ xe trên toàn quốc. Thay vì tìm kiếm thủ công và cầu may, tài xế có thể xem bản đồ thời gian thực, biết chính xác số lượng chỗ trống, biểu phí, và thực hiện đặt/giữ chỗ trước (Booking) ngay trên điện thoại.

## 2. Các đối tượng người dùng (Roles)
- **Tài xế (Driver):** Tìm kiếm bãi đỗ, giữ chỗ, thanh toán online, xem lịch sử phương tiện và đánh giá (Review).
- **Nhân viên bãi xe (Guard):** Quét mã QR của tài xế để Check-in/Check-out siêu tốc, xác nhận thu tiền mặt, báo cáo vắng mặt (No-show).
- **Chủ bãi đỗ (Parking Owner):** Thêm mới bãi đỗ lên bản đồ, cập nhật thông tin (giá cả, sức chứa, giờ hoạt động), quản lý nhân viên (Staff) và xem thống kê doanh thu.
- **Quản trị viên (Admin):** Kiểm duyệt và phê duyệt các bãi đỗ xe mới, quản lý tài khoản, chặn người dùng vi phạm, cấu hình hệ thống (Voucher, Notification).

## 3. Các tính năng cốt lõi (Core Features)
1. **Bản đồ không gian (Spatial Map Search):** Tìm kiếm bãi đỗ xe xung quanh vị trí người dùng thông qua thuật toán GIS, tự động lọc danh sách bãi xe mỗi khi vuốt bản đồ.
2. **Đặt giữ chỗ an toàn (Booking & Overbooking Prevention):** Cho phép đặt chỗ trước với thời hạn (Ví dụ: 10 phút). Đặc biệt, hệ thống xử lý tranh chấp rất tốt để chặn đứng các trường hợp nhận quá giới hạn sức chứa thực tế.
3. **Check-in/Check-out siêu tốc bằng mã QR:** Rút ngắn thời gian ra/vào bãi so với thẻ giấy/thẻ từ truyền thống.
4. **Tính phí động & Thanh toán (Dynamic Pricing & Payment):** Tính giá phức tạp theo block thời gian (block 30p, 1h, giá qua đêm), tự động áp dụng Voucher và hỗ trợ quét mã VietQR.
5. **Thông báo & Đồng bộ thời gian thực (Real-time Sync):** Cập nhật số chỗ trống của bãi xe trên bản đồ và thông báo đẩy tới người dùng tức thời.

## 4. Kiến trúc & Công nghệ (Tech Stack)
- **Backend:** .NET 10 Web API, kiến trúc đa tầng (N-Tier), Entity Framework Core (Code-First).
- **Cơ sở dữ liệu:** Microsoft SQL Server (sử dụng gói thư viện NetTopologySuite để xử lý dữ liệu địa lý GPS) và Redis Cache.
- **Frontend (Web CMS):** Angular, Typescript, thiết kế theo hướng Component-based, áp dụng Lazy Loading tối ưu hiệu năng.

## 5. Mục tiêu và Tầm nhìn
Dự án hướng đến việc số hóa toàn bộ quy trình quản lý bãi xe truyền thống. Giúp giảm thiểu ách tắc giao thông do phương tiện chạy lòng vòng tìm chỗ, minh bạch hóa chi phí, tránh "chặt chém" và cung cấp bộ công cụ quản lý chuyên nghiệp để tối đa hóa công suất, doanh thu cho các chủ bãi đỗ.
