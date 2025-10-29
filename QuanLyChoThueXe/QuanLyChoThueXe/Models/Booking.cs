using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueXe.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu thuê là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc thuê là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Số ngày thuê là bắt buộc")]
        [Range(1, 365, ErrorMessage = "Số ngày thuê phải từ 1 đến 365")]
        public int RentalDays { get; set; }

        [Required(ErrorMessage = "Giá thuê là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("CarId")]
        public virtual Car Car { get; set; } = null!;
    }

    public enum BookingStatus
    {
        Pending = 0,        // Chờ xử lý
        Confirmed = 1,      // Đã xác nhận
        Cancelled = 2,      // Đã hủy
        Completed = 3,      // Hoàn thành
        Expired = 4         // Hết hạn
    }
}
