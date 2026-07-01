# Danh sách thư viện và chức năng trong hệ thống Parking Booking

Tài liệu này liệt kê toàn bộ các thư viện (dependencies) quan trọng được sử dụng ở cả Backend và Frontend, giải thích chức năng chính của chúng và các vấn đề mà chúng giải quyết trong hệ thống.

## 1. Backend (.NET 10 Web API)

Các thư viện được quản lý trong file `parking_booking.csproj`.

| Thư viện (NuGet Package) | Chức năng chính | Vấn đề giải quyết |
| :--- | :--- | :--- |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | Xử lý xác thực (Authentication) dựa trên JSON Web Token (JWT). | Đảm bảo tính bảo mật cho hệ thống. Mã hóa, giải mã và xác thực token JWT được gửi từ client để xác định danh tính và quyền hạn của người dùng (Customer, Driver, Guard, Admin). |
| **Microsoft.AspNetCore.OpenApi** | Tự động sinh ra tài liệu mô tả API (kết hợp với Swagger). | Giúp frontend developer và mobile developer dễ dàng đọc tài liệu, hiểu cấu trúc request/response, và trực tiếp test các API endpoints mà không cần thông qua ứng dụng bên ngoài như Postman. |
| **Microsoft.EntityFrameworkCore.SqlServer** | Thư viện ORM (Object-Relational Mapping) cốt lõi của Entity Framework dành cho SQL Server. | Giúp Backend giao tiếp với cơ sở dữ liệu SQL Server thông qua các Model class (Code-First) thay vì phải viết các câu lệnh SQL thuần túy phức tạp. Quản lý việc ánh xạ dữ liệu và thực thi truy vấn. |
| **Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite** | Hỗ trợ xử lý kiểu dữ liệu không gian/địa lý (Geography/Geometry) cho EF Core và SQL Server. | Giải quyết bài toán tính toán khoảng cách. Ví dụ: Tính toán khoảng cách từ vị trí hiện tại của xe đến bãi đỗ xe hoặc vị trí đỗ trống gần nhất. |
| **Microsoft.EntityFrameworkCore.Tools** | Cung cấp bộ công cụ gõ lệnh (CLI) cho Entity Framework Core. | Hỗ trợ lập trình viên thao tác với database qua Command Line (ví dụ: `dotnet ef migrations add`, `dotnet ef database update`) để theo dõi lịch sử thay đổi schema cơ sở dữ liệu. |
| **RedLock.net** | Cung cấp thuật toán khóa phân tán (Distributed Lock) dựa trên Redis. | Giải quyết tình trạng Race Condition (cạnh tranh dữ liệu) khi có nhiều request đồng thời. Đảm bảo tại một thời điểm, chỉ có 1 request được quyền đặt chỗ (Booking) thành công tại 1 Slot (ParkingSlot), chống tình trạng đặt trùng chỗ. |
| **StackExchange.Redis** | Driver kết nối từ ứng dụng .NET đến máy chủ Redis Cache. | Hỗ trợ đọc/ghi dữ liệu siêu tốc độ vào RAM. Thường dùng để lưu trữ session, cache các dữ liệu ít thay đổi hoặc để tích hợp chung với RedLock cho cơ chế khóa đồng thời. |

## 2. Frontend Web CMS (Angular 21)

Các thư viện được quản lý trong file `package.json` của dự án Angular.

| Thư viện (npm Package) | Chức năng chính | Vấn đề giải quyết |
| :--- | :--- | :--- |
| **@angular/core, common, router, forms...** | Các package lõi của framework Angular. | Cung cấp kiến trúc Component-based mạnh mẽ để xây dựng SPA (Single Page Application). Quản lý state, routing chuyển trang, xử lý biểu mẫu (Forms), dependency injection, v.v. |
| **leaflet** | Thư viện JavaScript mã nguồn mở để xây dựng bản đồ tương tác (Interactive Map). | Hiển thị bản đồ toàn khu vực (Global Map) trên trang tìm kiếm. Nhận dữ liệu tọa độ (kinh độ, vĩ độ) để ghim vị trí các bãi đỗ xe, giúp người dùng (Tài xế) dễ dàng tìm kiếm bãi đỗ trực quan. |
| **qrcode** | Thư viện hỗ trợ tạo mã QR (QR Code generator). | Tạo mã QR trực tiếp trên trình duyệt dựa vào thông tin Booking. Mã QR này được dùng để tài xế đưa cho bảo vệ quét khi check-in / check-out tại cổng bãi đỗ xe. |
| **rxjs** | Thư viện cho lập trình phản ứng (Reactive Programming) bằng cách sử dụng Observables. | Quản lý luồng dữ liệu bất đồng bộ (Asynchronous streams), đặc biệt là xử lý các request HTTP gọi API backend, quản lý state và event debounce. |
| **zone.js** | Execution context (ngữ cảnh thực thi) dành riêng cho Angular. | Chịu trách nhiệm theo dõi các sự kiện bất đồng bộ (như setTimeout, HTTP Request, Event Listeners) và thông báo cho Angular để tự động cập nhật lại giao diện (Change Detection). |
| **jsdom, vitest, typescript, prettier** | Các công cụ phát triển (DevDependencies). | Hỗ trợ môi trường test (Vitest/JSDOM), trình biên dịch (TypeScript), và format code (Prettier) giúp duy trì chất lượng mã nguồn. |


http: hỏi -> server trả lời
websocket: server chủ động bắn dữ liệu xuống trình duyệt
signalR: trình duyệt không hỗ trợ websocket thì tự động lùi xuống công nghệ cũ hơn

NetTopologySuite (NTS) là một thư viện hỗ trợ xử lý dữ liệu Không gian và Địa lý (GIS - Geographic Information System) cực kỳ mạnh mẽ trong môi trường .NET.
Lưu trữ tọa độ: Thay vì lưu Kinh độ (Longitude) và Vĩ độ (Latitude) bằng hai cột kiểu double thông thường. Hệ thống dùng đối tượng Point của NetTopologySuite để lưu kiểu geography vào SQL Server. (Trong ParkingLot.cs: public Point Location { get; set; })