using System.ComponentModel.DataAnnotations;

namespace parking_booking_backend.DTOs;

public class CreateUserNotificationRequest
{
    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung là bắt buộc.")]
    public string Message { get; set; } = string.Empty;
}
