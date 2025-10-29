using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Controllers
{
    [Authorize(Policy = "EmployeeOrAdmin")]
    public class RentalContractController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RentalContractController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RentalContract
        public async Task<IActionResult> Index(string? searchString, ContractStatus? statusFilter)
        {
            var contractsQuery = _context.RentalContracts
                .Include(rc => rc.Customer)
                .Include(rc => rc.Car)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                contractsQuery = contractsQuery.Where(rc => rc.ContractNumber.Contains(searchString) || 
                                                rc.Customer.FullName.Contains(searchString) || 
                                                rc.Car.Name.Contains(searchString) ||
                                                rc.Car.LicensePlate.Contains(searchString));
            }

            if (statusFilter.HasValue)
            {
                contractsQuery = contractsQuery.Where(rc => rc.Status == statusFilter.Value);
            }

            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.ContractStatuses = Enum.GetValues<ContractStatus>();

            return View(await contractsQuery.ToListAsync());
        }

        // GET: RentalContract/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.RentalContracts
                .Include(rc => rc.Customer)
                .Include(rc => rc.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: RentalContract/Create
        public async Task<IActionResult> Create(int? bookingId)
        {
            ViewBag.Customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            ViewBag.Cars = await _context.Cars.Where(c => c.Status == CarStatus.Available).ToListAsync();
            
            if (bookingId.HasValue)
            {
                var booking = await _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Car)
                    .FirstOrDefaultAsync(b => b.Id == bookingId.Value);
                
                if (booking != null)
                {
                    var contract = new RentalContract
                    {
                        CustomerId = booking.CustomerId,
                        CarId = booking.CarId,
                        StartDate = booking.StartDate,
                        EndDate = booking.EndDate,
                        RentalDays = booking.RentalDays,
                        PricePerDay = booking.Car.PricePerDay,
                        TotalPrice = booking.TotalPrice,
                        Deposit = booking.TotalPrice * 0.5m, // 50% tiền cọc
                        Notes = booking.Notes
                    };
                    return View(contract);
                }
            }
            
            return View();
        }

        // POST: RentalContract/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,CarId,StartDate,EndDate,RentalDays,PricePerDay,TotalPrice,Deposit,Terms,Notes")] RentalContract contract)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xe có sẵn không
                var car = await _context.Cars.FindAsync(contract.CarId);
                if (car == null || car.Status != CarStatus.Available)
                {
                    ModelState.AddModelError("CarId", "Xe không có sẵn để thuê");
                    ViewBag.Customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
                    ViewBag.Cars = await _context.Cars.Where(c => c.Status == CarStatus.Available).ToListAsync();
                    return View(contract);
                }

                // Tạo số hợp đồng tự động
                contract.ContractNumber = await GenerateContractNumber();

                // Tính toán số ngày thuê và tổng giá
                contract.RentalDays = (int)(contract.EndDate - contract.StartDate).TotalDays + 1;
                contract.TotalPrice = contract.RentalDays * contract.PricePerDay;

                contract.CreatedAt = DateTime.Now;
                _context.Add(contract);

                // Cập nhật trạng thái xe thành Rented
                car.Status = CarStatus.Rented;
                car.UpdatedAt = DateTime.Now;
                _context.Update(car);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo hợp đồng thuê xe thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            ViewBag.Cars = await _context.Cars.Where(c => c.Status == CarStatus.Available).ToListAsync();
            return View(contract);
        }

        // GET: RentalContract/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.RentalContracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            ViewBag.Customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            ViewBag.Cars = await _context.Cars.ToListAsync();
            return View(contract);
        }

        // POST: RentalContract/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,CarId,ContractNumber,StartDate,EndDate,RentalDays,PricePerDay,TotalPrice,Deposit,Terms,Notes,Status,CreatedAt")] RentalContract contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contract.UpdatedAt = DateTime.Now;
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật hợp đồng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
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
            return View(contract);
        }

        // GET: RentalContract/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.RentalContracts
                .Include(rc => rc.Customer)
                .Include(rc => rc.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: RentalContract/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.RentalContracts.FindAsync(id);
            if (contract != null)
            {
                // Cập nhật trạng thái xe về Available nếu đang Rented
                var car = await _context.Cars.FindAsync(contract.CarId);
                if (car != null && car.Status == CarStatus.Rented)
                {
                    car.Status = CarStatus.Available;
                    car.UpdatedAt = DateTime.Now;
                    _context.Update(car);
                }

                _context.RentalContracts.Remove(contract);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa hợp đồng thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: RentalContract/Print/5
        public async Task<IActionResult> Print(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.RentalContracts
                .Include(rc => rc.Customer)
                .Include(rc => rc.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: RentalContract/Sign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sign(int id)
        {
            var contract = await _context.RentalContracts.FindAsync(id);
            if (contract != null)
            {
                contract.Status = ContractStatus.Active;
                contract.SignedAt = DateTime.Now;
                contract.UpdatedAt = DateTime.Now;
                _context.Update(contract);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ký hợp đồng thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: RentalContract/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var contract = await _context.RentalContracts.FindAsync(id);
            if (contract != null)
            {
                contract.Status = ContractStatus.Completed;
                contract.UpdatedAt = DateTime.Now;

                // Cập nhật trạng thái xe về Available
                var car = await _context.Cars.FindAsync(contract.CarId);
                if (car != null)
                {
                    car.Status = CarStatus.Available;
                    car.UpdatedAt = DateTime.Now;
                    _context.Update(car);
                }

                _context.Update(contract);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hoàn thành hợp đồng thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: RentalContract/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var contract = await _context.RentalContracts.FindAsync(id);
            if (contract != null)
            {
                contract.Status = ContractStatus.Cancelled;
                contract.UpdatedAt = DateTime.Now;

                // Cập nhật trạng thái xe về Available
                var car = await _context.Cars.FindAsync(contract.CarId);
                if (car != null)
                {
                    car.Status = CarStatus.Available;
                    car.UpdatedAt = DateTime.Now;
                    _context.Update(car);
                }

                _context.Update(contract);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hủy hợp đồng thành công!";
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

        private bool ContractExists(int id)
        {
            return _context.RentalContracts.Any(e => e.Id == id);
        }
    }
}
