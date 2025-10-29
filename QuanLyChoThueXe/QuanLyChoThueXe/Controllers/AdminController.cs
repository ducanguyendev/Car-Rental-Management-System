using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;
using QuanLyChoThueXe.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace QuanLyChoThueXe.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public AdminController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalEmployees = await _context.Employees.CountAsync(),
                TotalCars = await _context.Cars.CountAsync(),
                TotalBookings = await _context.Bookings.CountAsync(),
                TotalContracts = await _context.RentalContracts.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                AvailableCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Available)
            };

            return View(stats);
        }

        #region User Management

        [HttpGet("Users")]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Customer)
                .Include(u => u.Employee)
                .ToListAsync();
            return View(users);
        }

        [HttpGet("Users/Create")]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost("Users/Create")]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Create Employee first
                var employee = new Employee
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    IdentityNumber = model.IdentityNumber,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Address = model.Address,
                    Position = model.Position,
                    Department = model.Department,
                    Salary = model.Salary,
                    HireDate = DateTime.Now,
                    Status = EmployeeStatus.Active,
                    CreatedAt = DateTime.Now
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                // Create User account
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = UserRole.Employee,
                    IsActive = true,
                    EmployeeId = employee.Id,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Log activity
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                await _authService.LogActivityAsync("CreateUser", $"Created user account for {model.FullName}", userId, ipAddress);

                TempData["SuccessMessage"] = "Tạo tài khoản nhân viên thành công!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo tài khoản: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet("Users/{id}/Edit")]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Customer)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost("Users/{id}/Edit")]
        public async Task<IActionResult> EditUser(int id, User model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Username = model.Username;
                user.Email = model.Email;
                user.IsActive = model.IsActive;
                user.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Log activity
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                await _authService.LogActivityAsync("UpdateUser", $"Updated user {user.Username}", userId, ipAddress);

                TempData["SuccessMessage"] = "Cập nhật thông tin người dùng thành công!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost("Users/{id}/ResetPassword")]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(id, newPassword);
                if (!result)
                {
                    return Json(new { success = false, message = "Không thể đặt lại mật khẩu" });
                }

                // Log activity
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                await _authService.LogActivityAsync("ResetPassword", $"Reset password for user ID {id}", userId, ipAddress);

                return Json(new { success = true, message = "Đặt lại mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost("Users/{id}/ToggleStatus")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // Log activity
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var action = user.IsActive ? "ActivateUser" : "DeactivateUser";
                await _authService.LogActivityAsync(action, $"{(user.IsActive ? "Activated" : "Deactivated")} user {user.Username}", userId, ipAddress);

                return Json(new { success = true, message = user.IsActive ? "Kích hoạt tài khoản thành công!" : "Vô hiệu hóa tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        #endregion

        #region System Configuration

        [HttpGet("Configuration")]
        public async Task<IActionResult> Configuration()
        {
            var configurations = await _context.SystemConfigurations.ToListAsync();
            return View(configurations);
        }

        [HttpPost("Configuration")]
        public async Task<IActionResult> UpdateConfiguration(List<SystemConfiguration> configurations)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                foreach (var config in configurations)
                {
                    var existingConfig = await _context.SystemConfigurations.FindAsync(config.Id);
                    if (existingConfig != null)
                    {
                        existingConfig.Value = config.Value;
                        existingConfig.UpdatedAt = DateTime.Now;
                        existingConfig.UpdatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
                    }
                }

                await _context.SaveChangesAsync();

                await _authService.LogActivityAsync("UpdateConfiguration", "Updated system configurations", userId, ipAddress);

                TempData["SuccessMessage"] = "Cập nhật cấu hình hệ thống thành công!";
                return RedirectToAction("Configuration");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View("Configuration", configurations);
            }
        }

        #endregion

        #region System Logs

        [HttpGet("Logs")]
        public async Task<IActionResult> Logs(int page = 1, int pageSize = 50)
        {
            var logs = await _context.SystemLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalLogs = await _context.SystemLogs.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalLogs / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(logs);
        }

        #endregion
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Username là bắt buộc")]
        [StringLength(50, ErrorMessage = "Username không được vượt quá 50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Lương là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Lương phải lớn hơn 0")]
        public decimal Salary { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalCars { get; set; }
        public int TotalBookings { get; set; }
        public int TotalContracts { get; set; }
        public int ActiveUsers { get; set; }
        public int AvailableCars { get; set; }
    }
}
