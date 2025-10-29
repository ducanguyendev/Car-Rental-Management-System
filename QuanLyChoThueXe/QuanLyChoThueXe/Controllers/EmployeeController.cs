using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;
using QuanLyChoThueXe.Services;
using System.Security.Claims;

namespace QuanLyChoThueXe.Controllers
{
    [Authorize(Policy = "EmployeeOrAdmin")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public EmployeeController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // Dashboard nhân viên
        public async Task<IActionResult> Index()
        {
            // Nếu là Admin thì redirect về Admin dashboard
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Employee == null)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            // Thống kê nhanh
            var totalCars = await _context.Cars.CountAsync();
            var availableCars = await _context.Cars.CountAsync(c => c.Status == Models.CarStatus.Available);
            var totalCustomers = await _context.Customers.CountAsync();
            var activeBookings = await _context.Bookings.CountAsync(b => b.Status == Models.BookingStatus.Pending);
            var activeContracts = await _context.RentalContracts.CountAsync(rc => rc.Status == Models.ContractStatus.Active);

            ViewBag.Employee = user.Employee;
            ViewBag.TotalCars = totalCars;
            ViewBag.AvailableCars = availableCars;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.ActiveBookings = activeBookings;
            ViewBag.ActiveContracts = activeContracts;

            return View();
        }

        // Xem hồ sơ cá nhân
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Employee == null)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            ViewBag.Username = user.Username;
            ViewBag.Email = user.Email;
            ViewBag.Employee = user.Employee;

            return View();
        }

        // Đổi mật khẩu - GET
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // Đổi mật khẩu - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            if (!result)
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng!");
                return View(model);
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }
    }
}

