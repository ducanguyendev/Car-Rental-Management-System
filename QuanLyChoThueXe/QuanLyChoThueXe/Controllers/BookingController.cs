using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Controllers
{
    [Authorize(Policy = "EmployeeOrAdmin")]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Booking
        public async Task<IActionResult> Index(string? searchString, BookingStatus? statusFilter)
        {
            var bookingsQuery = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bookingsQuery = bookingsQuery.Where(b => b.Customer.FullName.Contains(searchString) || 
                                             b.Car.Name.Contains(searchString) ||
                                             b.Car.LicensePlate.Contains(searchString));
            }

            if (statusFilter.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.Status == statusFilter.Value);
            }

            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.BookingStatuses = Enum.GetValues<BookingStatus>();

            return View(await bookingsQuery.ToListAsync());
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Booking/Create
        public async Task<IActionResult> Create()
        {
            var customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            var cars = await _context.Cars.Where(c => c.Status == CarStatus.Available).ToListAsync();
            
            ViewBag.Customers = new SelectList(customers, "Id", "FullName");
            ViewBag.Cars = new SelectList(cars, "Id", "Name");
            
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,CarId,StartDate,EndDate,RentalDays,TotalPrice,Notes")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xe có sẵn không
                var car = await _context.Cars.FindAsync(booking.CarId);
                if (car == null || car.Status != CarStatus.Available)
                {
                    ModelState.AddModelError("CarId", "Xe không có sẵn để thuê");
                    var activeCustomers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
                    var availableCars = await _context.Cars.Where(c => c.Status == CarStatus.Available).ToListAsync();
                    ViewBag.Customers = new SelectList(activeCustomers, "Id", "FullName");
                    ViewBag.Cars = new SelectList(availableCars, "Id", "Name");
                    return View(booking);
                }

                // Tính toán số ngày thuê và tổng giá
                booking.RentalDays = (int)(booking.EndDate - booking.StartDate).TotalDays + 1;
                booking.TotalPrice = booking.RentalDays * car.PricePerDay;

                booking.CreatedAt = DateTime.Now;
                _context.Add(booking);

                // Cập nhật trạng thái xe thành Reserved
                car.Status = CarStatus.Reserved;
                car.UpdatedAt = DateTime.Now;
                _context.Update(car);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đặt xe thành công!";
                return RedirectToAction(nameof(Index));
            }

            var activeCustomers2 = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            var availableCars2 = await _context.Cars.Where(c => c.Status == CarStatus.Available).ToListAsync();
            ViewBag.Customers = new SelectList(activeCustomers2, "Id", "FullName");
            ViewBag.Cars = new SelectList(availableCars2, "Id", "Name");
            return View(booking);
        }

        // GET: Booking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.Customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            ViewBag.Cars = await _context.Cars.ToListAsync();
            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,CarId,StartDate,EndDate,RentalDays,TotalPrice,Notes,Status,CreatedAt")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    booking.UpdatedAt = DateTime.Now;
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật đặt xe thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            ViewBag.Cars = await _context.Cars.ToListAsync();
            return View(booking);
        }

        // GET: Booking/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                // Cập nhật trạng thái xe về Available nếu đang Reserved
                var car = await _context.Cars.FindAsync(booking.CarId);
                if (car != null && car.Status == CarStatus.Reserved)
                {
                    car.Status = CarStatus.Available;
                    car.UpdatedAt = DateTime.Now;
                    _context.Update(car);
                }

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hủy đặt xe thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Booking/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == id);
                
            if (booking != null)
            {
                // Cập nhật trạng thái booking
                booking.Status = BookingStatus.Confirmed;
                booking.UpdatedAt = DateTime.Now;
                _context.Update(booking);

                // Tạo hợp đồng tự động
                var contract = new RentalContract
                {
                    CustomerId = booking.CustomerId,
                    CarId = booking.CarId,
                    ContractNumber = await GenerateContractNumber(),
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                    RentalDays = booking.RentalDays,
                    PricePerDay = booking.Car.PricePerDay,
                    TotalPrice = booking.TotalPrice,
                    Deposit = booking.TotalPrice * 0.5m, // 50% tiền cọc
                    Terms = "1. Khách hàng cam kết sử dụng xe đúng mục đích và tuân thủ luật giao thông.\n" +
                           "2. Khách hàng chịu trách nhiệm bảo quản xe trong thời gian thuê.\n" +
                           "3. Mọi hư hỏng do khách hàng gây ra sẽ được khấu trừ vào tiền cọc.\n" +
                           "4. Khách hàng phải trả xe đúng giờ và địa điểm đã thỏa thuận.",
                    Notes = booking.Notes,
                    Status = ContractStatus.Active,
                    CreatedAt = DateTime.Now
                };

                _context.RentalContracts.Add(contract);

                // Cập nhật trạng thái xe thành Rented
                var car = await _context.Cars.FindAsync(booking.CarId);
                if (car != null)
                {
                    car.Status = CarStatus.Rented;
                    car.UpdatedAt = DateTime.Now;
                    _context.Update(car);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Xác nhận đặt xe thành công! Đã tạo hợp đồng {contract.ContractNumber}";
                TempData["ContractId"] = contract.Id; // Lưu ID hợp đồng để có thể redirect
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GenerateContractNumber()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month.ToString("D2");
            var count = await _context.RentalContracts.CountAsync() + 1;
            return $"HD{year}{month}{count:D4}";
        }

        // POST: Booking/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = DateTime.Now;

                // Cập nhật trạng thái xe về Available
                var car = await _context.Cars.FindAsync(booking.CarId);
                if (car != null)
                {
                    car.Status = CarStatus.Available;
                    car.UpdatedAt = DateTime.Now;
                    _context.Update(car);
                }

                _context.Update(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hủy đặt xe thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}
