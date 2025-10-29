using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueXe.Models
{
    public class RentalContract
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required(ErrorMessage = "Số hợp đồng là bắt buộc")]
        [StringLength(20, ErrorMessage = "Số hợp đồng không được vượt quá 20 ký tự")]
        public string ContractNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày bắt đầu thuê là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc thuê là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Số ngày thuê là bắt buộc")]
        [Range(1, 365, ErrorMessage = "Số ngày thuê phải từ 1 đến 365")]
        public int RentalDays { get; set; }

        [Required(ErrorMessage = "Giá thuê mỗi ngày là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê mỗi ngày phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }

        [Required(ErrorMessage = "Tổng giá thuê là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng giá thuê phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Phí đặt cọc phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Deposit { get; set; } = 0;

        [StringLength(500, ErrorMessage = "Điều khoản không được vượt quá 500 ký tự")]
        public string? Terms { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? SignedAt { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("CarId")]
        public virtual Car Car { get; set; } = null!;
    }

    public enum ContractStatus
    {
        Draft = 0,          // Nháp
        Active = 1,         // Đang hoạt động
        Completed = 2,      // Hoàn thành
        Cancelled = 3,      // Đã hủy
        Expired = 4         // Hết hạn
    }
}
