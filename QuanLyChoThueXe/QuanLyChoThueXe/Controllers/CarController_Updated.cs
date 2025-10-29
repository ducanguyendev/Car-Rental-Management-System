using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;
using QuanLyChoThueXe.Services;

namespace QuanLyChoThueXe.Controllers
{
    [Authorize(Policy = "EmployeeOrAdmin")]
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICarApiService _carApiService;

        public CarController(ApplicationDbContext context, ICarApiService carApiService)
        {
            _context = context;
            _carApiService = carApiService;
        }

        // GET: Car
        public async Task<IActionResult> Index(string? searchString, CarStatus? statusFilter)
        {
            try
            {
                var cars = await _carApiService.GetCarsAsync(searchString, statusFilter);

                ViewBag.SearchString = searchString;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.CarStatuses = Enum.GetValues<CarStatus>();

                return View(cars);
            }
            catch (Exception ex)
            {
                // Fallback to direct database access if API fails
                var cars = from c in _context.Cars select c;

                if (!string.IsNullOrEmpty(searchString))
                {
                    cars = cars.Where(c => c.Name.Contains(searchString) || 
                                         c.Brand.Contains(searchString) || 
                                         c.LicensePlate.Contains(searchString));
                }

                if (statusFilter.HasValue)
                {
                    cars = cars.Where(c => c.Status == statusFilter.Value);
                }

                ViewBag.SearchString = searchString;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.CarStatuses = Enum.GetValues<CarStatus>();

                return View(await cars.ToListAsync());
            }
        }

        // GET: Car/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var car = await _carApiService.GetCarAsync(id.Value);
                if (car == null)
                {
                    return NotFound();
                }

                return View(car);
            }
            catch (Exception ex)
            {
                // Fallback to direct database access if API fails
                var car = await _context.Cars
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (car == null)
                {
                    return NotFound();
                }

                return View(car);
            }
        }

        // GET: Car/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Car/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,LicensePlate,Brand,Model,Year,Type,Seats,FuelType,PricePerDay,Description,ImageUrl,Status")] Car car)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _carApiService.CreateCarAsync(car);
                    TempData["SuccessMessage"] = "Thêm xe thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Fallback to direct database access if API fails
                    car.CreatedAt = DateTime.Now;
                    _context.Add(car);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm xe thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(car);
        }

        // GET: Car/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var car = await _carApiService.GetCarAsync(id.Value);
                if (car == null)
                {
                    return NotFound();
                }
                return View(car);
            }
            catch (Exception ex)
            {
                // Fallback to direct database access if API fails
                var car = await _context.Cars.FindAsync(id);
                if (car == null)
                {
                    return NotFound();
                }
                return View(car);
            }
        }

        // POST: Car/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,LicensePlate,Brand,Model,Year,Type,Seats,FuelType,PricePerDay,Description,ImageUrl,Status,CreatedAt")] Car car)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _carApiService.UpdateCarAsync(id, car);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Cập nhật thông tin xe thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Cập nhật thông tin xe thất bại!");
                    }
                }
                catch (Exception ex)
                {
                    // Fallback to direct database access if API fails
                    try
                    {
                        car.UpdatedAt = DateTime.Now;
                        _context.Update(car);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Cập nhật thông tin xe thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CarExists(car.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            return View(car);
        }

        // GET: Car/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var car = await _carApiService.GetCarAsync(id.Value);
                if (car == null)
                {
                    return NotFound();
                }

                return View(car);
            }
            catch (Exception ex)
            {
                // Fallback to direct database access if API fails
                var car = await _context.Cars
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (car == null)
                {
                    return NotFound();
                }

                return View(car);
            }
        }

        // POST: Car/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _carApiService.DeleteCarAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Xóa xe thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Xóa xe thất bại!";
                }
            }
            catch (Exception ex)
            {
                // Fallback to direct database access if API fails
                var car = await _context.Cars.FindAsync(id);
                if (car != null)
                {
                    _context.Cars.Remove(car);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Xóa xe thành công!";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Car/Status/5
        public async Task<IActionResult> Status(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var car = await _carApiService.GetCarAsync(id.Value);
                if (car == null)
                {
                    return NotFound();
                }

                // Get additional data for status view
                var carWithRelations = await _context.Cars
                    .Include(c => c.RentalContracts.Where(rc => rc.Status == ContractStatus.Active))
                    .Include(c => c.Bookings.Where(b => b.Status == BookingStatus.Confirmed))
                    .FirstOrDefaultAsync(m => m.Id == id);

                return View(carWithRelations ?? car);
            }
            catch (Exception ex)
            {
                // Fallback to direct database access if API fails
                var car = await _context.Cars
                    .Include(c => c.RentalContracts.Where(rc => rc.Status == ContractStatus.Active))
                    .Include(c => c.Bookings.Where(b => b.Status == BookingStatus.Confirmed))
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (car == null)
                {
                    return NotFound();
                }

                return View(car);
            }
        }

        // API: Get car status
        [HttpGet]
        public async Task<IActionResult> GetCarStatus(int id)
        {
            try
            {
                var result = await _carApiService.GetCarStatusAsync(id);
                return Json(result);
            }
            catch (Exception ex)
            {
                // Fallback to direct database access if API fails
                var car = await _context.Cars.FindAsync(id);
                if (car == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy xe" });
                }

                var statusText = car.Status switch
                {
                    CarStatus.Available => "Có sẵn",
                    CarStatus.Rented => "Đang thuê",
                    CarStatus.Reserved => "Đã đặt trước",
                    CarStatus.Maintenance => "Bảo trì",
                    CarStatus.OutOfService => "Ngừng hoạt động",
                    _ => "Không xác định"
                };

                return Json(new { 
                    success = true, 
                    status = car.Status.ToString(),
                    statusText = statusText,
                    carName = car.Name,
                    licensePlate = car.LicensePlate
                });
            }
        }

        // API: Get car statistics
        [HttpGet]
        public async Task<IActionResult> GetCarStats(int id)
        {
            try
            {
                var result = await _carApiService.GetCarStatsAsync(id);
                return Json(result);
            }
            catch (Exception ex)
            {
                // Fallback to direct database access if API fails
                var car = await _context.Cars.FindAsync(id);
                if (car == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy xe" });
                }

                var rentalCount = await _context.RentalContracts.CountAsync(rc => rc.CarId == id);
                var bookingCount = await _context.Bookings.CountAsync(b => b.CarId == id);
                var totalRevenue = await _context.RentalContracts
                    .Where(rc => rc.CarId == id)
                    .SumAsync(rc => rc.TotalPrice);

                return Json(new { 
                    success = true, 
                    rentalCount = rentalCount,
                    bookingCount = bookingCount,
                    totalRevenue = totalRevenue
                });
            }
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}
