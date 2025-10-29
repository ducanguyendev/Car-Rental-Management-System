using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;
using System.Globalization;

namespace QuanLyChoThueXe.Controllers
{
    [Authorize(Policy = "EmployeeOrAdmin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Report/Revenue
        public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate, string period = "month")
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddDays(-30);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var contracts = await _context.RentalContracts
                .Include(rc => rc.Car)
                .Include(rc => rc.Customer)
                .Where(rc => rc.CreatedAt >= startDate.Value && rc.CreatedAt <= endDate.Value)
                .ToListAsync();

            var totalRevenue = contracts.Sum(rc => rc.TotalPrice);
            var totalDeposits = contracts.Sum(rc => rc.Deposit);
            var completedContracts = contracts.Count(rc => rc.Status == ContractStatus.Completed);
            var activeContracts = contracts.Count(rc => rc.Status == ContractStatus.Active);

            var contractsByCarType = contracts
                .GroupBy(rc => rc.Car.Type)
                .Select(g => new { Type = g.Key, Count = g.Count(), Revenue = g.Sum(rc => rc.TotalPrice) })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalDeposits = totalDeposits;
            ViewBag.CompletedContracts = completedContracts;
            ViewBag.ActiveContracts = activeContracts;
            ViewBag.Contracts = contracts;
            ViewBag.ContractsByCarType = contractsByCarType;

            return View();
        }

        // GET: Report/CarStatus
        public async Task<IActionResult> CarStatus()
        {
            var totalCars = await _context.Cars.CountAsync();
            var availableCars = await _context.Cars.CountAsync(c => c.Status == Models.CarStatus.Available);
            var rentedCars = await _context.Cars.CountAsync(c => c.Status == Models.CarStatus.Rented);
            var reservedCars = await _context.Cars.CountAsync(c => c.Status == Models.CarStatus.Reserved);
            var maintenanceCars = await _context.Cars.CountAsync(c => c.Status == Models.CarStatus.Maintenance);

            var carsByType = await _context.Cars
                .GroupBy(c => c.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            var mostRentedCars = await _context.Cars
                .OrderByDescending(c => c.RentalContracts.Count)
                .Take(10)
                .Select(c => new { Car = c, RentCount = c.RentalContracts.Count })
                .ToListAsync();

            ViewBag.TotalCars = totalCars;
            ViewBag.AvailableCars = availableCars;
            ViewBag.RentedCars = rentedCars;
            ViewBag.ReservedCars = reservedCars;
            ViewBag.MaintenanceCars = maintenanceCars;
            ViewBag.CarsByType = carsByType;
            ViewBag.MostRentedCars = mostRentedCars;

            return View();
        }

        // GET: Report/Customer
        public async Task<IActionResult> Customer()
        {
            var totalCustomers = await _context.Customers.CountAsync();
            var activeCustomers = await _context.Customers.CountAsync(c => c.Status == Models.CustomerStatus.Active);

            var topCustomers = await _context.Customers
                .Select(c => new
                {
                    Customer = c,
                    BookingCount = c.Bookings.Count,
                    ContractCount = c.RentalContracts.Count,
                    TotalRevenue = c.RentalContracts.Sum(rc => rc.TotalPrice)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(10)
                .ToListAsync();

            var recentCustomers = await _context.Customers
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.ActiveCustomers = activeCustomers;
            ViewBag.TopCustomers = topCustomers;
            ViewBag.RecentCustomers = recentCustomers;

            return View();
        }

        // GET: Report/Contract
        public async Task<IActionResult> Contract(ContractStatus? statusFilter, DateTime? startDate, DateTime? endDate)
        {
            var contractsQuery = _context.RentalContracts
                .Include(rc => rc.Car)
                .Include(rc => rc.Customer)
                .AsQueryable();

            if (statusFilter.HasValue)
            {
                contractsQuery = contractsQuery.Where(rc => rc.Status == statusFilter.Value);
            }

            if (startDate.HasValue)
            {
                contractsQuery = contractsQuery.Where(rc => rc.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                contractsQuery = contractsQuery.Where(rc => rc.EndDate <= endDate.Value);
            }

            var contracts = await contractsQuery.ToListAsync();

            var contractsByStatus = contracts
                .GroupBy(rc => rc.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            var contractsByCarType = contracts
                .GroupBy(rc => rc.Car.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.StatusFilter = statusFilter;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Contracts = contracts;
            ViewBag.ContractsByStatus = contractsByStatus;
            ViewBag.ContractsByCarType = contractsByCarType;

            return View();
        }

        // GET: Report/Overview
        public async Task<IActionResult> Overview()
        {
            // Thống kê tổng quan
            var totalCars = await _context.Cars.CountAsync();
            var availableCars = await _context.Cars.CountAsync(c => c.Status == Models.CarStatus.Available);
            var totalCustomers = await _context.Customers.CountAsync();
            var activeCustomers = await _context.Customers.CountAsync(c => c.Status == Models.CustomerStatus.Active);
            var totalBookings = await _context.Bookings.CountAsync();
            var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == Models.BookingStatus.Pending);
            var totalContracts = await _context.RentalContracts.CountAsync();
            var activeContracts = await _context.RentalContracts.CountAsync(rc => rc.Status == Models.ContractStatus.Active);

            // Doanh thu
            var todayRevenue = await _context.RentalContracts
                .Where(rc => rc.CreatedAt.Date == DateTime.Today)
                .SumAsync(rc => (decimal?)rc.TotalPrice) ?? 0;

            var thisMonthRevenue = await _context.RentalContracts
                .Where(rc => rc.CreatedAt.Year == DateTime.Now.Year && rc.CreatedAt.Month == DateTime.Now.Month)
                .SumAsync(rc => (decimal?)rc.TotalPrice) ?? 0;

            var thisYearRevenue = await _context.RentalContracts
                .Where(rc => rc.CreatedAt.Year == DateTime.Now.Year)
                .SumAsync(rc => (decimal?)rc.TotalPrice) ?? 0;

            // Top xe thuê nhiều nhất
            var topRentedCars = await _context.Cars
                .OrderByDescending(c => c.RentalContracts.Count)
                .Take(5)
                .Select(c => new { Car = c, RentCount = c.RentalContracts.Count })
                .ToListAsync();

            // Top khách hàng
            var topCustomers = await _context.Customers
                .Select(c => new { Customer = c, Revenue = c.RentalContracts.Sum(rc => rc.TotalPrice) })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalCars = totalCars;
            ViewBag.AvailableCars = availableCars;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.ActiveCustomers = activeCustomers;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.PendingBookings = pendingBookings;
            ViewBag.TotalContracts = totalContracts;
            ViewBag.ActiveContracts = activeContracts;
            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.ThisMonthRevenue = thisMonthRevenue;
            ViewBag.ThisYearRevenue = thisYearRevenue;
            ViewBag.TopRentedCars = topRentedCars;
            ViewBag.TopCustomers = topCustomers;

            return View();
        }
    }
}

