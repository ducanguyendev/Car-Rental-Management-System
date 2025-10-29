using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueXe.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required(ErrorMessage = "Tiêu đề thông báo là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề thông báo không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung thông báo là bắt buộc")]
        [StringLength(1000, ErrorMessage = "Nội dung thông báo không được vượt quá 1000 ký tự")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("CarId")]
        public virtual Car Car { get; set; } = null!;
    }

    public enum NotificationType
    {
        CarAvailable = 0,       // Xe có sẵn
        BookingConfirmed = 1,   // Đặt xe được xác nhận
        BookingCancelled = 2,   // Đặt xe bị hủy
        ContractExpiring = 3,   // Hợp đồng sắp hết hạn
        PaymentReminder = 4,    // Nhắc nhở thanh toán
        General = 5             // Thông báo chung
    }

    public enum NotificationStatus
    {
        Unread = 0,     // Chưa đọc
        Read = 1,       // Đã đọc
        Deleted = 2     // Đã xóa
    }
}
