using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueXe.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên xe là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên xe không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Biển số xe là bắt buộc")]
        [StringLength(20, ErrorMessage = "Biển số xe không được vượt quá 20 ký tự")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hãng xe là bắt buộc")]
        [StringLength(50, ErrorMessage = "Hãng xe không được vượt quá 50 ký tự")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model xe là bắt buộc")]
        [StringLength(50, ErrorMessage = "Model xe không được vượt quá 50 ký tự")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm sản xuất là bắt buộc")]
        [Range(1990, 2025, ErrorMessage = "Năm sản xuất phải từ 1990 đến 2025")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Loại xe là bắt buộc")]
        [StringLength(30, ErrorMessage = "Loại xe không được vượt quá 30 ký tự")]
        public string Type { get; set; } = string.Empty; // Sedan, SUV, Hatchback, etc.

        [Required(ErrorMessage = "Số chỗ ngồi là bắt buộc")]
        [Range(2, 50, ErrorMessage = "Số chỗ ngồi phải từ 2 đến 50")]
        public int Seats { get; set; }

        [Required(ErrorMessage = "Loại nhiên liệu là bắt buộc")]
        [StringLength(20, ErrorMessage = "Loại nhiên liệu không được vượt quá 20 ký tự")]
        public string FuelType { get; set; } = string.Empty; // Xăng, Dầu, Điện, Hybrid

        [Required(ErrorMessage = "Giá thuê là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Đường dẫn hình ảnh không được vượt quá 200 ký tự")]
        public string? ImageUrl { get; set; }

        [Required]
        public CarStatus Status { get; set; } = CarStatus.Available;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<RentalContract> RentalContracts { get; set; } = new List<RentalContract>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public enum CarStatus
    {
        Available = 0,      // Có sẵn
        Rented = 1,         // Đang thuê
        Reserved = 2,       // Đã đặt trước
        Maintenance = 3,    // Bảo trì
        OutOfService = 4    // Ngừng hoạt động
    }
}
