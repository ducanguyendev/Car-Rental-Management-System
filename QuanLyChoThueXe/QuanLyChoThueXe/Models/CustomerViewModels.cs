using System.ComponentModel.DataAnnotations;

namespace QuanLyChoThueXe.Models
{
    public class CustomerDashboardViewModel
    {
        public Customer Customer { get; set; } = null!;
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ActiveContracts { get; set; }
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<RentalContract> RecentContracts { get; set; } = new List<RentalContract>();
    }

    public class CustomerProfileViewModel
    {
        public Customer Customer { get; set; } = null!;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CarSearchViewModel
    {
        public string CarType { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Car> Cars { get; set; } = new List<Car>();
    }

    public class CreateBookingViewModel
    {
        public int CarId { get; set; }
        public Car Car { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        
        [Required(ErrorMessage = "Ngày nhận xe là bắt buộc")]
        [Display(Name = "Ngày nhận xe")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "Ngày trả xe là bắt buộc")]
        [Display(Name = "Ngày trả xe")]
        public DateTime EndDate { get; set; }
        
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
        
        public decimal TotalPrice { get; set; }
    }
}
