using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueXe.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int RentalContractId { get; set; }

        [Required(ErrorMessage = "Số hóa đơn là bắt buộc")]
        [StringLength(20, ErrorMessage = "Số hóa đơn không được vượt quá 20 ký tự")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại hóa đơn là bắt buộc")]
        public InvoiceType Type { get; set; }

        [Required(ErrorMessage = "Số tiền là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedBy { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("RentalContractId")]
        public virtual RentalContract RentalContract { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual Employee? CreatedByEmployee { get; set; }
    }

    public enum InvoiceType
    {
        Deposit = 0,        // Đặt cọc
        FinalPayment = 1,   // Tất toán
        Refund = 2,         // Hoàn tiền
        AdditionalFee = 3   // Phí phát sinh
    }

    public enum InvoiceStatus
    {
        Paid = 0,           // Đã thanh toán
        Pending = 1,        // Chờ thanh toán
        Cancelled = 2       // Đã hủy
    }
}

