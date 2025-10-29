using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueXe.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số CCCD/CMND là bắt buộc")]
        [StringLength(20, ErrorMessage = "Số CCCD/CMND không được vượt quá 20 ký tự")]
        public string IdentityNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string Address { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Nghề nghiệp không được vượt quá 50 ký tự")]
        public string? Occupation { get; set; }

        [StringLength(200, ErrorMessage = "Nơi làm việc không được vượt quá 200 ký tự")]
        public string? Workplace { get; set; }

        [Required]
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<RentalContract> RentalContracts { get; set; } = new List<RentalContract>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public enum Gender
    {
        Male = 0,   // Nam
        Female = 1, // Nữ
        Other = 2   // Khác
    }

    public enum CustomerStatus
    {
        Active = 0,     // Hoạt động
        Inactive = 1,   // Không hoạt động
        Blacklisted = 2 // Cấm thuê
    }
}
