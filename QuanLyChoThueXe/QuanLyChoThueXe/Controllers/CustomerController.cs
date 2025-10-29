using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;
using QuanLyChoThueXe.Services;
using System.Security.Claims;

namespace QuanLyChoThueXe.Controllers
{
    [Authorize(Policy = "CustomerOrAdmin")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public CustomerController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // Dashboard khách hàng
        public async Task<IActionResult> Index()
        {
            // Kiểm tra nếu là Admin hoặc Employee thì redirect về trang tương ứng
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            
            if (User.IsInRole("Employee"))
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var dashboard = new CustomerDashboardViewModel
            {
                Customer = user.Customer,
                TotalBookings = await _context.Bookings.CountAsync(b => b.CustomerId == user.Customer.Id),
                PendingBookings = await _context.Bookings.CountAsync(b => b.CustomerId == user.Customer.Id && b.Status == BookingStatus.Pending),
                ActiveContracts = await _context.RentalContracts.CountAsync(r => r.CustomerId == user.Customer.Id && r.Status == ContractStatus.Active),
                RecentBookings = await _context.Bookings
                    .Include(b => b.Car)
                    .Where(b => b.CustomerId == user.Customer.Id)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentContracts = await _context.RentalContracts
                    .Include(r => r.Car)
                    .Where(r => r.CustomerId == user.Customer.Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboard);
        }

        // Quản lý hồ sơ
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var model = new CustomerProfileViewModel
            {
                Customer = user.Customer,
                Username = user.Username,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(CustomerProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("AccessDenied", "Home");
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

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        // Tìm kiếm xe
        public async Task<IActionResult> SearchCars(CarSearchViewModel searchModel)
        {
            var query = _context.Cars.Where(c => c.Status == CarStatus.Available);

            if (!string.IsNullOrEmpty(searchModel.CarType))
            {
                query = query.Where(c => c.Type == searchModel.CarType);
            }

            if (!string.IsNullOrEmpty(searchModel.Brand))
            {
                query = query.Where(c => c.Brand.Contains(searchModel.Brand));
            }

            if (searchModel.MinYear.HasValue)
            {
                query = query.Where(c => c.Year >= searchModel.MinYear.Value);
            }

            if (searchModel.MaxYear.HasValue)
            {
                query = query.Where(c => c.Year <= searchModel.MaxYear.Value);
            }

            if (searchModel.StartDate.HasValue && searchModel.EndDate.HasValue)
            {
                // Kiểm tra xe có sẵn trong khoảng thời gian
                query = query.Where(c => !_context.RentalContracts.Any(r => 
                    r.CarId == c.Id && 
                    r.Status == ContractStatus.Active &&
                    ((r.StartDate <= searchModel.StartDate.Value && r.EndDate >= searchModel.StartDate.Value) ||
                     (r.StartDate <= searchModel.EndDate.Value && r.EndDate >= searchModel.EndDate.Value) ||
                     (r.StartDate >= searchModel.StartDate.Value && r.EndDate <= searchModel.EndDate.Value))));
            }

            var cars = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

            searchModel.Cars = cars;
            return View(searchModel);
        }

        // Chi tiết xe
        public async Task<IActionResult> CarDetails(int id)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // Quản lý đặt xe
        public async Task<IActionResult> Bookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerId == user.Customer.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        // Tạo đặt xe
        public async Task<IActionResult> CreateBooking(int carId)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var model = new CreateBookingViewModel
            {
                CarId = carId,
                Car = car,
                Customer = user.Customer,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBooking(CreateBookingViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            // Xóa lỗi validation cho Car và Customer vì chúng không được bind từ form
            ModelState.Remove(nameof(model.Car));
            ModelState.Remove(nameof(model.Customer));

            // Kiểm tra thông tin khách hàng đã đầy đủ chưa
            var customer = user.Customer;
            if (string.IsNullOrEmpty(customer.FullName) || 
                string.IsNullOrEmpty(customer.PhoneNumber) || 
                string.IsNullOrEmpty(customer.Email) || 
                string.IsNullOrEmpty(customer.IdentityNumber) || 
                string.IsNullOrEmpty(customer.Address) ||
                customer.DateOfBirth == DateTime.MinValue)
            {
                ModelState.AddModelError("", "Vui lòng cập nhật đầy đủ thông tin cá nhân trước khi đặt xe!");
                TempData["ErrorMessage"] = "Vui lòng cập nhật đầy đủ thông tin cá nhân trước khi đặt xe!";
                return RedirectToAction("Profile");
            }

            // Kiểm tra validation cho ngày
            if (model.StartDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("StartDate", "Ngày nhận xe không được nhỏ hơn ngày hiện tại!");
            }

            if (model.EndDate.Date <= model.StartDate.Date)
            {
                ModelState.AddModelError("EndDate", "Ngày trả xe phải sau ngày nhận xe!");
            }

            // Kiểm tra xe có sẵn không
            var carInfo = await _context.Cars.FirstOrDefaultAsync(c => c.Id == model.CarId);
            if (carInfo == null)
            {
                return NotFound();
            }

            if (carInfo.Status != CarStatus.Available)
            {
                ModelState.AddModelError("", "Xe này không còn sẵn sàng để đặt!");
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
                ModelState.AddModelError("", "Xe đã được đặt trong khoảng thời gian này!");
            }

            if (!ModelState.IsValid)
            {
                var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == model.CarId);
                if (car == null)
                {
                    return NotFound();
                }
                model.Car = car;
                model.Customer = user.Customer;
                return View(model);
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

                TempData["SuccessMessage"] = "Tạo phiếu đặt xe thành công!";
                return RedirectToAction("Bookings");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi tạo phiếu đặt xe: {ex.Message}");
                model.Car = carInfo;
                model.Customer = user.Customer;
                return View(model);
            }
        }

        // Hủy đặt xe
        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == user.Customer.Id);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
            {
                TempData["ErrorMessage"] = "Không thể hủy phiếu đặt xe này!";
                return RedirectToAction("Bookings");
            }

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Log hoạt động
            await _authService.LogActivityAsync("CancelBooking", $"Hủy phiếu đặt xe ID: {id}", userId, Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

            TempData["SuccessMessage"] = "Hủy phiếu đặt xe thành công!";
            return RedirectToAction("Bookings");
        }

        // Quản lý hợp đồng
        public async Task<IActionResult> Contracts()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var contracts = await _context.RentalContracts
                .Include(r => r.Car)
                .Where(r => r.CustomerId == user.Customer.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(contracts);
        }

        // Chi tiết hợp đồng
        public async Task<IActionResult> ContractDetails(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var contract = await _context.RentalContracts
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == user.Customer.Id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // Đổi mật khẩu
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            if (result)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Profile");
            }
            else
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng!");
                return View(model);
            }
        }
    }
}