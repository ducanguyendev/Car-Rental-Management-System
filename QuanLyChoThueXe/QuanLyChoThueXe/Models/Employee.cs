using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueXe.Models
{
    public class Employee
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

        [Required(ErrorMessage = "Chức vụ là bắt buộc")]
        [StringLength(50, ErrorMessage = "Chức vụ không được vượt quá 50 ký tự")]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phòng ban là bắt buộc")]
        [StringLength(50, ErrorMessage = "Phòng ban không được vượt quá 50 ký tự")]
        public string Department { get; set; } = string.Empty;

        [Required]
        public decimal Salary { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        [Required]
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
    }

    public enum EmployeeStatus
    {
        Active = 0,     // Đang làm việc
        Inactive = 1,   // Nghỉ việc
        Suspended = 2   // Tạm nghỉ
    }
}
