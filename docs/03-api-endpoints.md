# Parking Booking API

Tài liệu này phản ánh các route hiện có trong backend. Mọi response nghiệp vụ dùng dạng `ApiResponse<T>`. Các API có ghi role yêu cầu JWT Bearer với role tương ứng; `Authenticated` chấp nhận mọi tài khoản đã đăng nhập.

## Quy ước chung

- Base path: `/api`.
- Lỗi validation: `400`; thiếu/sai token: `401`; sai role hoặc không sở hữu tài nguyên: `403`; không tìm thấy: `404`; xung đột trạng thái: `409`.
- Rate limit: API đọc tối đa 120 request/phút; API ghi tối đa 30 request/phút. Vượt hạn mức trả `429` kèm `Retry-After`.
- Auth login tối đa 5 request/10 phút/IP; auth verify tối đa 10 request/10 phút/IP.
- Trừ các endpoint ghi `Anonymous`, tất cả endpoint đều yêu cầu JWT.

## Auth

| Method | Route | Quyền | Request | Response |
|---|---|---|---|---|
| POST | `/api/auth/login` | Anonymous | `{ phoneNumber }` | `{ otpSent: true }` |
| POST | `/api/auth/verify` | Anonymous | `{ phoneNumber, otp, fullName? }` | `{ token, role, userId }` |

Hiện OTP là cơ chế mô phỏng: login chỉ xác nhận request và verify chấp nhận chuỗi OTP hợp lệ theo validation.

## Bookings

| Method | Route | Quyền | Request | Response |
|---|---|---|---|---|
| GET | `/api/bookings/me` | Authenticated | — | `BookingHistoryResponse[]` |
| GET | `/api/bookings/recent-parking-lots` | Driver | — | `ParkingLotSummaryResponse[]` |
| GET | `/api/bookings/staff` | Guard | — | `StaffBookingResponse[]` |
| GET | `/api/bookings/owner` | ParkingOwner | — | `StaffBookingResponse[]` |
| GET | `/api/bookings/admin/all?page=&size=` | Admin | Phân trang | `PaginationResponse<StaffBookingResponse>` |
| POST | `/api/bookings` | Authenticated | `{ parkingSlotId, vehicleId?, guestLicensePlate? }` | `201 BookingResponse` |
| POST | `/api/bookings/{id}/check-in` | Authenticated, có quyền booking | — | `BookingResponse` |
| POST | `/api/bookings/{id}/apply-voucher` | Authenticated, có quyền booking | `{ voucherCode }` | `BookingResponse` |
| POST | `/api/bookings/{id}/check-out` | Authenticated, có quyền booking | `{ useWallet, collectCash }` | `{ totalPrice, vietQrUrl, status, checkOutTimestamp }` |
| POST | `/api/bookings/{id}/cancel` | Authenticated, có quyền booking | — | `{ cancelled: true }` |
| POST | `/api/bookings/{id}/no-show` | Admin, Guard | — | `{ noShow: true }` |
| GET | `/api/bookings/{id}/qr` | Authenticated, có quyền booking | — | `{ qrToken }` |
| POST | `/api/bookings/verify-qr` | Guard | `{ qrToken }` | `VerifyQrResponse` |

Booking mới ở trạng thái `Pending`; hạn check-in bằng `bookingTimestamp + 10 phút`. Booking hết hạn chuyển thành `NoShow`. `Cancelled` và `NoShow` trong 30 ngày đều là vi phạm; tài khoản bị khóa ở lần thứ 4. Verify QR chỉ xác thực QR và quyền Guard tại bãi, không tự check-in.

## Parking lots, staff và layout

| Method | Route | Quyền | Request | Response |
|---|---|---|---|---|
| GET | `/api/parking-lots/nearby?lat=&lng=&radiusKm=` | Authenticated | Tọa độ và bán kính | `ParkingLotSummaryResponse[]` |
| POST | `/api/parking-lots` | ParkingOwner | `CreateParkingLotRequest` | `201 ParkingLotDetailResponse` |
| PUT | `/api/parking-lots/{id}` | ParkingOwner sở hữu | `UpdateParkingLotRequest` | `ParkingLotDetailResponse` |
| PUT | `/api/parking-lots/{id}/approve` | Admin | — | `{ approved: true }` |
| GET | `/api/parking-lots/bounds?minLat=&maxLat=&minLng=&maxLng=&userLat?=&userLng?=` | Authenticated | Bounds | `ParkingLotSummaryResponse[]` |
| GET | `/api/parking-lots/search?keyword=` | Authenticated | Keyword | `ParkingLotSearchResponse` |
| GET | `/api/parking-lots/{id}` | Authenticated | — | `ParkingLotDetailResponse` |
| GET | `/api/parking-lots/staff/me` | Guard | — | `ParkingLotSummaryResponse[]` |
| POST | `/api/parking-lots/{id}/report` | Authenticated | `{ status, currentLat, currentLng }` | `{ reported: true }` |
| POST | `/api/parking-lots/{id}/staff` | Authenticated; service kiểm tra quyền sở hữu | `{ userId }` | `{ added: true }` |
| POST | `/api/parking-lots/{id}/staff/by-phone` | ParkingOwner | `{ phoneNumber }` | `{ added: true }` |
| POST | `/api/parking-lots/{id}/staff/create` | ParkingOwner | `{ fullName, phoneNumber }` | `{ created: true }` |
| DELETE | `/api/parking-lots/{id}/staff/{userId}` | ParkingOwner | — | `{ removed: true }` |
| GET | `/api/parking-lots/{id}/floors` | Authenticated | — | `ParkingFloorResponse[]` |
| POST | `/api/parking-lots/{id}/floors` | Authenticated; service kiểm tra quyền | `CreateParkingFloorRequest` | `201 ParkingFloorResponse` |
| GET | `/api/parking-lots/{id}/floors/{floorId}/slots` | Authenticated | — | `ParkingSlotResponse[]` |
| PUT | `/api/parking-lots/{id}/floors/{floorId}/slots` | Authenticated; service kiểm tra quyền | `UpsertParkingSlotRequest[]` | `ParkingSlotResponse[]` |
| GET | `/api/parking-lots/{id}/reviews` | Anonymous | — | `ReviewResponse[]` |
| GET | `/api/parking-lots/owner/me` | ParkingOwner | — | `ParkingLotSummaryResponse[]` |
| GET | `/api/parking-lots/owner/staff` | ParkingOwner | — | `OwnerStaffAssignmentResponse[]` |
| GET | `/api/parking-lots/admin/all?page=&size=&keyword?=` | Admin | Phân trang | `PaginationResponse<ParkingLotSummaryResponse>` |

## Users

| Method | Route | Quyền | Request | Response |
|---|---|---|---|---|
| GET | `/api/users/me` | Authenticated | — | `{ id, phoneNumber, fullName, role, trustScore }` |
| PUT | `/api/users/me` | Authenticated | `{ fullName }` | `UserResponse` |
| GET | `/api/users?page=&size=&hasPenalty?=` | Admin | Phân trang/bộ lọc | `PaginationResponse<AdminUserResponse>` |
| PUT | `/api/users/{id}/lock` | Admin | — | Trạng thái thành công |
| POST | `/api/users` | Admin | `{ phoneNumber, fullName, role, password }` | `AdminUserResponse` |

## Vehicles

| Method | Route | Quyền | Request | Response |
|---|---|---|---|---|
| GET | `/api/vehicles` | Authenticated | — | `VehicleResponse[]` |
| POST | `/api/vehicles` | Authenticated | `{ licensePlate, vehicleType, isDefault }` | `201 VehicleResponse` |
| PUT | `/api/vehicles/{id}` | Chủ phương tiện | `{ vehicleType, isDefault }` | `VehicleResponse` |
| DELETE | `/api/vehicles/{id}` | Chủ phương tiện | — | `{ deleted: true }` |

## Monthly passes

| Method | Route | Quyền | Request | Response |
|---|---|---|---|---|
| GET | `/api/monthly-passes/me` | Authenticated | — | `MonthlyPassResponse[]` |
| POST | `/api/monthly-passes` | Authenticated | `{ vehicleId, parkingLotId, durationDays, useWallet }` | `201 MonthlyPassResponse` |

## Wallets và payments

| Method | Route | Quyền | Request | Response |
|---|---|---|---|---|
| GET | `/api/wallets/me` | Authenticated | — | `{ id, userId, balance, transactions }` |
| POST | `/api/wallets/deposit` | Authenticated | `{ amount }` | `{ amount, vietQrUrl }` |
| POST | `/api/payments/webhook` | Authenticated | `{ code, data: { amount, description, transactionDateTime } }` | `{ matched }` |

Payment và VietQR hiện là mô phỏng; webhook chỉ đối chiếu booking code trong description và chưa xác thực chữ ký nhà cung cấp.

## Reviews, templates và vouchers

| Method | Route | Quyền | Request | Response |
|---|---|---|---|---|
| POST | `/api/reviews` | Authenticated | `{ bookingId, rating, comment? }` | `201 ReviewResponse` |
| GET | `/api/layout-templates` | Authenticated | — | `LayoutTemplateResponse[]` |
| GET | `/api/vouchers/valid` | Authenticated | — | `VoucherResponse[]` |
| POST | `/api/vouchers` | Admin | `CreateVoucherRequest` | `201 VoucherResponse` |
| PUT | `/api/vouchers/{id}` | Admin | `UpdateVoucherRequest` | `VoucherResponse` |
| DELETE | `/api/vouchers/{id}` | Admin | — | `{ deleted: true }` |

## Notifications và bãi đỗ yêu thích

| Method | Route | Quyền | Request | Response |
|---|---|---|---|---|
| GET | `/api/notifications?unreadOnly=false` | Authenticated | Bộ lọc chưa đọc | `NotificationResponse[]` |
| PUT | `/api/notifications/{id}/read` | Chủ thông báo | — | `{ read: true }` |
| PUT | `/api/notifications/read-all` | Authenticated | — | `{ read: true }` |
| POST | `/api/notifications/send` | Admin | `{ phoneNumber, title, message }` | `{ sent: true }` |
| GET | `/api/favourites` | Driver | — | `ParkingLotSummaryResponse[]` |
| POST | `/api/favourites/{parkingLotId}` | Driver | — | `{ favourite: true }` |
| DELETE | `/api/favourites/{parkingLotId}` | Driver | — | `{ favourite: false }` |

Hệ thống tự tạo thông báo cho các sự kiện booking và gửi nhắc check-in khi còn dưới 2 phút.

## Development data

| Method | Route | Quyền | Request | Response |
|---|---|---|---|---|
| POST | `/api/dev/data/seed?recreate=false` | Anonymous, Development only | Query `recreate` | `SeedResult`; production trả `404` |

## Health checks

| Method | Route | Quyền | Ý nghĩa | Response |
|---|---|---|---|---|
| GET | `/health/live` | Anonymous | Tiến trình API đang chạy | `{ status }` |
| GET | `/health/ready` | Anonymous | SQL Server và Redis sẵn sàng | `{ status }` |

Health endpoint không chịu rate limit và không trả exception, secret hoặc connection string.
