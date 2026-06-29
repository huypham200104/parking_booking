# ParkGo - Hệ thống Đặt chỗ Bãi Đỗ Xe Thông Minh (Smart Parking Booking System)

ParkGo là một nền tảng toàn diện giúp kết nối tài xế có nhu cầu tìm kiếm bãi đỗ xe với các chủ bãi (Owner) một cách nhanh chóng và thuận tiện thông qua Web và App di động. Dự án cung cấp hệ thống quản lý trọn gói từ việc đặt chỗ, thanh toán, đến giám sát không gian bãi đỗ xe theo thời gian thực.

## Cấu trúc Dự án (Project Structure)

Dự án được chia thành 2 module chính (Monorepo):

1. **[Backend (.NET 10 API)](./parking_booking_backend/)**
   - API cung cấp dữ liệu và logic nghiệp vụ cho toàn bộ hệ thống.
   - Core Tech: ASP.NET Core Web API, Entity Framework Core (Code-First), Redis (RedLock), SignalR (Realtime).
   - Database: SQL Server LocalDB (Môi trường Dev) / PostgreSQL (Production).

2. **[Frontend (Angular Web CMS)](./parking_booking_web/)**
   - Ứng dụng Web quản lý dành cho 4 đối tượng: Admin (Quản trị viên), Owner (Chủ bãi), Driver (Tài xế) và Guard (Bảo vệ).
   - Core Tech: Angular (Standalone), SCSS, Leaflet.js (Bản đồ bãi đỗ xe).

3. **[Tài liệu Hệ thống (Docs)](./docs/)**
   - Chứa toàn bộ tài liệu thiết kế hệ thống, kiến trúc, sơ đồ cơ sở dữ liệu và API design.

## Yêu cầu Hệ thống (Prerequisites)
Để chạy dự án này trên môi trường local, máy tính của bạn cần cài đặt:
- [.NET 10.0 SDK](https://dotnet.microsoft.com/)
- [Node.js](https://nodejs.org/) (Phiên bản >= 18.0)
- SQL Server LocalDB (Đi kèm với Visual Studio hoặc cài rời)

## Hướng dẫn Khởi chạy (Getting Started)

Để khởi chạy toàn bộ hệ thống, bạn cần chạy song song cả Backend và Frontend theo 2 bước sau:

### Bước 1: Khởi chạy Backend (.NET)
Mở Terminal / PowerShell và trỏ vào thư mục backend:
```bash
cd parking_booking_backend
```
Khôi phục thư viện và cập nhật Database (Migration):
```bash
dotnet restore
dotnet ef database update
```
Chạy server API:
```bash
dotnet run
```
Backend sẽ mặc định lắng nghe tại `http://localhost:5242`.

### Bước 2: Khởi chạy Frontend (Angular)
Mở một Terminal / PowerShell mới và trỏ vào thư mục frontend:
```bash
cd parking_booking_web
```
Cài đặt các gói thư viện Node.js:
```bash
npm install
```
Khởi chạy Web server (Đã config sẵn proxy trỏ sang Backend):
```bash
npm start
```
Truy cập ứng dụng tại địa chỉ: `http://localhost:4200`

## Tài liệu đính kèm (Documentation Links)
Để hiểu rõ hơn về luồng hoạt động cũng như các tính năng của hệ thống, vui lòng xem chi tiết trong thư mục `docs/`:
- [Tổng quan dự án](docs/01-tong-quan.md)
- [Thiết kế Cơ sở dữ liệu (Database Schema)](docs/07-database-schema.md)
- [Thiết kế API (API Design)](docs/08-api-design.md)
- [Cơ chế Bản đồ Bãi đỗ xe](docs/09-map-mechanism.md)
- [Nhật ký Lỗi và Giải pháp](docs/11-project-errors-log.md)