using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Redirect customers to their dashboard
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Customer"))
            {
                return RedirectToAction("Index", "Customer");
            }

            // Thống kê tổng quan
            var totalCars = await _context.Cars.CountAsync();
            var availableCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Available);
            var rentedCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Rented);
            var reservedCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Reserved);

            var totalCustomers = await _context.Customers.CountAsync();
            var activeCustomers = await _context.Customers.CountAsync(c => c.Status == CustomerStatus.Active);

            var totalBookings = await _context.Bookings.CountAsync();
            var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
            var confirmedBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed);

            var totalContracts = await _context.RentalContracts.CountAsync();
            var activeContracts = await _context.RentalContracts.CountAsync(rc => rc.Status == ContractStatus.Active);

            var unreadNotifications = await _context.Notifications.CountAsync(n => n.Status == NotificationStatus.Unread);

            ViewBag.TotalCars = totalCars;
            ViewBag.AvailableCars = availableCars;
            ViewBag.RentedCars = rentedCars;
            ViewBag.ReservedCars = reservedCars;

            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.ActiveCustomers = activeCustomers;

            ViewBag.TotalBookings = totalBookings;
            ViewBag.PendingBookings = pendingBookings;
            ViewBag.ConfirmedBookings = confirmedBookings;

            ViewBag.TotalContracts = totalContracts;
            ViewBag.ActiveContracts = activeContracts;

            ViewBag.UnreadNotifications = unreadNotifications;

            // Lấy danh sách xe gần đây
            var recentCars = await _context.Cars
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Lấy danh sách booking gần đây
            var recentBookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Lấy danh sách hợp đồng gần đây
            var recentContracts = await _context.RentalContracts
                .Include(rc => rc.Customer)
                .Include(rc => rc.Car)
                .OrderByDescending(rc => rc.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentCars = recentCars;
            ViewBag.RecentBookings = recentBookings;
            ViewBag.RecentContracts = recentContracts;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // API: Get dashboard statistics
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = new
            {
                totalCars = await _context.Cars.CountAsync(),
                availableCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Available),
                rentedCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Rented),
                reservedCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Reserved),
                totalCustomers = await _context.Customers.CountAsync(),
                activeCustomers = await _context.Customers.CountAsync(c => c.Status == CustomerStatus.Active),
                totalBookings = await _context.Bookings.CountAsync(),
                pendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending),
                totalContracts = await _context.RentalContracts.CountAsync(),
                activeContracts = await _context.RentalContracts.CountAsync(rc => rc.Status == ContractStatus.Active),
                unreadNotifications = await _context.Notifications.CountAsync(n => n.Status == NotificationStatus.Unread)
            };

            return Json(stats);
        }
    }
}
