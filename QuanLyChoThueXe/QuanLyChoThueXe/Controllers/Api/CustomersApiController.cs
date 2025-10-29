using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;
using QuanLyChoThueXe.Services;
using System.Security.Claims;

namespace QuanLyChoThueXe.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "CustomerOrAdmin")]
    public class CustomersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public CustomersApiController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // GET: api/customers/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetDashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
            }

            var dashboard = new
            {
                customer = user.Customer,
                totalBookings = await _context.Bookings.CountAsync(b => b.CustomerId == user.Customer.Id),
                pendingBookings = await _context.Bookings.CountAsync(b => b.CustomerId == user.Customer.Id && b.Status == BookingStatus.Pending),
                activeContracts = await _context.RentalContracts.CountAsync(r => r.CustomerId == user.Customer.Id && r.Status == ContractStatus.Active),
                recentBookings = await _context.Bookings
                    .Include(b => b.Car)
                    .Where(b => b.CustomerId == user.Customer.Id)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .Select(b => new { b.Id, b.StartDate, b.EndDate, b.Status, b.TotalPrice, carName = b.Car.Name })
                    .ToListAsync(),
                recentContracts = await _context.RentalContracts
                    .Include(r => r.Car)
                    .Where(r => r.CustomerId == user.Customer.Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new { r.Id, r.ContractNumber, r.StartDate, r.EndDate, r.Status, r.TotalPrice, carName = r.Car.Name })
                    .ToListAsync()
            };

            return Ok(dashboard);
        }

        // GET: api/customers/profile
        [HttpGet("profile")]
        public async Task<ActionResult<object>> GetProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
            }

            var profile = new
            {
                customer = user.Customer,
                username = user.Username,
                email = user.Email
            };

            return Ok(profile);
        }

        // PUT: api/customers/profile
        [HttpPut("profile")]
        public async Task<ActionResult<object>> UpdateProfile(CustomerProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
            }

            // Cập nhật thông tin khách hàng
            user.Customer.FullName = model.Customer.FullName;
            user.Customer.PhoneNumber = model.Customer.PhoneNumber;
            user.Customer.Address = model.Customer.Address;
            user.Customer.Email = model.Customer.Email;
            user.Customer.IdentityNumber = model.Customer.IdentityNumber;
            user.Customer.DateOfBirth = model.Customer.DateOfBirth;
            user.Customer.Gender = model.Customer.Gender;
            user.Customer.Occupation = model.Customer.Occupation;
            user.Customer.Workplace = model.Customer.Workplace;
            user.Customer.UpdatedAt = DateTime.Now;

            // Cập nhật thông tin user
            user.Email = model.Email;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Log hoạt động
            await _authService.LogActivityAsync("UpdateProfile", "Cập nhật thông tin cá nhân", userId, Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

            return Ok(new { success = true, message = "Cập nhật thông tin thành công!" });
        }

        // GET: api/customers/search-cars
        [HttpGet("search-cars")]
        public async Task<ActionResult<IEnumerable<Car>>> SearchCars(
            [FromQuery] string? carType,
            [FromQuery] string? brand,
            [FromQuery] int? minYear,
            [FromQuery] int? maxYear,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var query = _context.Cars.Where(c => c.Status == CarStatus.Available);

            if (!string.IsNullOrEmpty(carType))
            {
                query = query.Where(c => c.Type == carType);
            }

            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(c => c.Brand.Contains(brand));
            }

            if (minYear.HasValue)
            {
                query = query.Where(c => c.Year >= minYear.Value);
            }

            if (maxYear.HasValue)
            {
                query = query.Where(c => c.Year <= maxYear.Value);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                // Kiểm tra xe có sẵn trong khoảng thời gian
                query = query.Where(c => !_context.RentalContracts.Any(r => 
                    r.CarId == c.Id && 
                    r.Status == ContractStatus.Active &&
                    ((r.StartDate <= startDate.Value && r.EndDate >= startDate.Value) ||
                     (r.StartDate <= endDate.Value && r.EndDate >= endDate.Value) ||
                     (r.StartDate >= startDate.Value && r.EndDate <= endDate.Value))));
            }

            var cars = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return Ok(cars);
        }

        // GET: api/customers/car-details/5
        [HttpGet("car-details/{id}")]
        public async Task<ActionResult<Car>> GetCarDetails(int id)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return Ok(car);
        }

        // GET: api/customers/bookings
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
            }

            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerId == user.Customer.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(bookings);
        }

        // POST: api/customers/create-booking
        [HttpPost("create-booking")]
        public async Task<ActionResult<object>> CreateBooking(CreateBookingViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
            }

            // Kiểm tra thông tin khách hàng đã đầy đủ chưa
            var customer = user.Customer;
            if (string.IsNullOrEmpty(customer.FullName) || 
                string.IsNullOrEmpty(customer.PhoneNumber) || 
                string.IsNullOrEmpty(customer.Email) || 
                string.IsNullOrEmpty(customer.IdentityNumber) || 
                string.IsNullOrEmpty(customer.Address) ||
                customer.DateOfBirth == DateTime.MinValue)
            {
                return BadRequest(new { message = "Vui lòng cập nhật đầy đủ thông tin cá nhân trước khi đặt xe!" });
            }

            // Kiểm tra validation cho ngày
            if (model.StartDate.Date < DateTime.Today)
            {
                return BadRequest(new { message = "Ngày nhận xe không được nhỏ hơn ngày hiện tại!" });
            }

            if (model.EndDate.Date <= model.StartDate.Date)
            {
                return BadRequest(new { message = "Ngày trả xe phải sau ngày nhận xe!" });
            }

            // Kiểm tra xe có sẵn không
            var carInfo = await _context.Cars.FirstOrDefaultAsync(c => c.Id == model.CarId);
            if (carInfo == null)
            {
                return NotFound(new { message = "Không tìm thấy xe" });
            }

            if (carInfo.Status != CarStatus.Available)
            {
                return BadRequest(new { message = "Xe này không còn sẵn sàng để đặt!" });
            }

            // Kiểm tra xe có bị trùng lịch không
            var hasConflict = await _context.RentalContracts.AnyAsync(r => 
                r.CarId == model.CarId && 
                r.Status == ContractStatus.Active &&
                ((r.StartDate <= model.StartDate && r.EndDate >= model.StartDate) ||
                 (r.StartDate <= model.EndDate && r.EndDate >= model.EndDate) ||
                 (r.StartDate >= model.StartDate && r.EndDate <= model.EndDate)));

            if (hasConflict)
            {
                return BadRequest(new { message = "Xe đã được đặt trong khoảng thời gian này!" });
            }

            // Tính số ngày và tổng giá
            var rentalDays = (int)(model.EndDate - model.StartDate).TotalDays;
            var totalPrice = carInfo.PricePerDay * rentalDays;

            var booking = new Booking
            {
                CustomerId = user.Customer.Id,
                CarId = model.CarId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                RentalDays = rentalDays,
                TotalPrice = totalPrice,
                Notes = model.Notes,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now
            };

            try
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                
                // Log hoạt động
                await _authService.LogActivityAsync("CreateBooking", $"Tạo phiếu đặt xe #{booking.Id} cho xe ID: {model.CarId}", userId, Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

                return Ok(new { success = true, message = "Tạo phiếu đặt xe thành công!", bookingId = booking.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi khi tạo phiếu đặt xe: {ex.Message}" });
            }
        }

        // POST: api/customers/cancel-booking/5
        [HttpPost("cancel-booking/{id}")]
        public async Task<ActionResult<object>> CancelBooking(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == user.Customer.Id);

            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy phiếu đặt xe" });
            }

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
            {
                return BadRequest(new { message = "Không thể hủy phiếu đặt xe này!" });
            }

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Log hoạt động
            await _authService.LogActivityAsync("CancelBooking", $"Hủy phiếu đặt xe ID: {id}", userId, Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

            return Ok(new { success = true, message = "Hủy phiếu đặt xe thành công!" });
        }

        // GET: api/customers/contracts
        [HttpGet("contracts")]
        public async Task<ActionResult<IEnumerable<RentalContract>>> GetContracts()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
            }

            var contracts = await _context.RentalContracts
                .Include(r => r.Car)
                .Where(r => r.CustomerId == user.Customer.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(contracts);
        }

        // GET: api/customers/contract-details/5
        [HttpGet("contract-details/{id}")]
        public async Task<ActionResult<RentalContract>> GetContractDetails(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
            }

            var contract = await _context.RentalContracts
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == user.Customer.Id);

            if (contract == null)
            {
                return NotFound();
            }

            return Ok(contract);
        }

        // POST: api/customers/change-password
        [HttpPost("change-password")]
        public async Task<ActionResult<object>> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            if (result)
            {
                return Ok(new { success = true, message = "Đổi mật khẩu thành công!" });
            }
            else
            {
                return BadRequest(new { message = "Mật khẩu hiện tại không đúng!" });
            }
        }
    }
}
