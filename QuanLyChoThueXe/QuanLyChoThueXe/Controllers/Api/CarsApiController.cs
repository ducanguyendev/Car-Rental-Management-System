using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "EmployeeOrAdmin")]
    public class CarsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CarsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/cars
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> GetCars(
            [FromQuery] string? searchString, 
            [FromQuery] CarStatus? statusFilter)
        {
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

            return await cars.ToListAsync();
        }

        // GET: api/cars/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Car>> GetCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return NotFound();
            }

            return car;
        }

        // POST: api/cars
        [HttpPost]
        public async Task<ActionResult<Car>> PostCar(Car car)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            car.CreatedAt = DateTime.Now;
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCar", new { id = car.Id }, car);
        }

        // PUT: api/cars/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCar(int id, Car car)
        {
            if (id != car.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                car.UpdatedAt = DateTime.Now;
                _context.Update(car);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarExists(id))
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

        // DELETE: api/cars/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/cars/5/status
        [HttpGet("{id}/status")]
        public async Task<ActionResult<object>> GetCarStatus(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy xe" });
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

            return Ok(new { 
                success = true, 
                status = car.Status.ToString(),
                statusText = statusText,
                carName = car.Name,
                licensePlate = car.LicensePlate
            });
        }

        // GET: api/cars/5/stats
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<object>> GetCarStats(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy xe" });
            }

            var rentalCount = await _context.RentalContracts.CountAsync(rc => rc.CarId == id);
            var bookingCount = await _context.Bookings.CountAsync(b => b.CarId == id);
            var totalRevenue = await _context.RentalContracts
                .Where(rc => rc.CarId == id)
                .SumAsync(rc => rc.TotalPrice);

            return Ok(new { 
                success = true, 
                rentalCount = rentalCount,
                bookingCount = bookingCount,
                totalRevenue = totalRevenue
            });
        }

        // GET: api/cars/available
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Car>>> GetAvailableCars()
        {
            return await _context.Cars
                .Where(c => c.Status == CarStatus.Available)
                .ToListAsync();
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}
