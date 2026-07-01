# Luồng Tự Động Hủy Booking (Booking Auto Cancel Flow)

Dưới đây là sơ đồ chi tiết giải thích cách mà `BookingAutoCancelService` (chạy ngầm mỗi 1 phút) hoạt động để tự động chuyển trạng thái Booking, giải phóng chỗ trống và khóa tài khoản người dùng vi phạm.

```mermaid
flowchart TD
    Start((Bắt đầu vòng lặp\n(Mỗi 1 phút))) --> GetTime[Lấy thời gian hiện tại\n(UtcNow)]
    
    GetTime --> CheckReminders{Tìm Booking còn\n< 2 phút hết hạn?}
    CheckReminders -- Có --> SendReminder[Gửi Thông báo:\n'Sắp hết hạn Check-in']
    SendReminder --> CheckExpired
    CheckReminders -- Không --> CheckExpired

    CheckExpired{Tìm Booking đã\nquá hạn Check-in?}
    CheckExpired -- Không --> Wait[Nghỉ 1 phút\n(Task.Delay)]
    Wait --> Start
    
    CheckExpired -- Có (Danh sách Expired) --> ProcessExpired[Bắt đầu xử lý từng Booking]
    
    ProcessExpired --> ChangeStatus[Đổi Status = NoShow]
    ChangeStatus --> NotifyExpired[Gửi Thông báo:\n'Đặt chỗ đã bị hủy']
    NotifyExpired --> FreeSlot[Giải phóng Chỗ đỗ\n(Slot Status = Available)]
    FreeSlot --> UpdateLot[Tăng số chỗ trống của Bãi xe\n(AvailableSlots + 1)]
    
    UpdateLot --> CheckUserViolations[Kiểm tra lịch sử vi phạm\ncủa User (30 ngày qua)]
    
    CheckUserViolations --> IsBanned{Số lần Hủy/NoShow\n> 3 lần?}
    
    IsBanned -- Có --> LockAccount[Khóa tài khoản (IsLocked = true)]
    LockAccount --> NotifyBan[Gửi Thông báo:\n'Tài khoản đã bị khóa']
    NotifyBan --> SaveDB
    
    IsBanned -- Không --> SaveDB
    
    SaveDB[(Lưu xuống Database\n(SaveChangesAsync))] --> Wait
    
    classDef highlight fill:#f9f,stroke:#333,stroke-width:2px;
    class CheckReminders,CheckExpired,IsBanned highlight;
```

## Các điểm nhấn kỹ thuật trong luồng này:
1. **Chạy nền (Background Service):** Luồng này hoàn toàn tách biệt với các API của người dùng. Dù không có ai tương tác với App, Server vẫn âm thầm tự quét dọn dữ liệu mỗi 60 giây.
2. **Ngăn chặn kẹt chỗ (Deadlock chỗ đỗ):** Nếu người dùng đặt chỗ nhưng cố tình không đến, bãi xe sẽ mất doanh thu vì chỗ đó bị khóa cứng. Luồng này giúp "đòi lại" chỗ đỗ (`AvailableSlots + 1`) để bán cho khách khác.
3. **Phạt nguội tự động:** Hệ thống tự đếm số lần vi phạm (Hủy quá trễ hoặc Không đến) trong 30 ngày. Vượt quá 3 lần sẽ tự động khóa tài khoản để chống hành vi phá hoại bãi xe.
