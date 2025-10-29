using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "EmployeeOrAdmin")]
    public class BookingsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings(
            [FromQuery] string? searchString, 
            [FromQuery] BookingStatus? statusFilter)
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

            return await bookingsQuery.ToListAsync();
        }

        // GET: api/bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return booking;
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra xe có sẵn không
            var car = await _context.Cars.FindAsync(booking.CarId);
            if (car == null || car.Status != CarStatus.Available)
            {
                return BadRequest(new { message = "Xe không có sẵn để thuê" });
            }

            // Tính toán số ngày thuê và tổng giá
            booking.RentalDays = (int)(booking.EndDate - booking.StartDate).TotalDays + 1;
            booking.TotalPrice = booking.RentalDays * car.PricePerDay;

            booking.CreatedAt = DateTime.Now;
            _context.Bookings.Add(booking);

            // Cập nhật trạng thái xe thành Reserved
            car.Status = CarStatus.Reserved;
            car.UpdatedAt = DateTime.Now;
            _context.Update(car);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBooking", new { id = booking.Id }, booking);
        }

        // PUT: api/bookings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBooking(int id, Booking booking)
        {
            if (id != booking.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                booking.UpdatedAt = DateTime.Now;
                _context.Update(booking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/bookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

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

            return NoContent();
        }

        // POST: api/bookings/5/confirm
        [HttpPost("{id}/confirm")]
        public async Task<ActionResult<object>> ConfirmBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == id);
                
            if (booking == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy phiếu đặt xe" });
            }

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

            return Ok(new { 
                success = true, 
                message = $"Xác nhận đặt xe thành công! Đã tạo hợp đồng {contract.ContractNumber}",
                contractId = contract.Id,
                contractNumber = contract.ContractNumber
            });
        }

        // POST: api/bookings/5/cancel
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<object>> CancelBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy phiếu đặt xe" });
            }

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

            return Ok(new { success = true, message = "Hủy đặt xe thành công!" });
        }

        // GET: api/bookings/customers/5
        [HttpGet("customers/{customerId}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetCustomerBookings(int customerId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings;
        }

        // GET: api/bookings/create-data
        [HttpGet("create-data")]
        public async Task<ActionResult<object>> GetCreateData()
        {
            var customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            var cars = await _context.Cars.Where(c => c.Status == CarStatus.Available).ToListAsync();
            
            return Ok(new { customers, cars });
        }

        private async Task<string> GenerateContractNumber()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month.ToString("D2");
            var count = await _context.RentalContracts.CountAsync() + 1;
            return $"HD{year}{month}{count:D4}";
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}
