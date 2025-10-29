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
    public class RentalContractsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RentalContractsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/rentalcontracts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RentalContract>>> GetRentalContracts(
            [FromQuery] string? searchString, 
            [FromQuery] ContractStatus? statusFilter)
        {
            var contractsQuery = _context.RentalContracts
                .Include(r => r.Customer)
                .Include(r => r.Car)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                contractsQuery = contractsQuery.Where(r => r.ContractNumber.Contains(searchString) ||
                                                          r.Customer.FullName.Contains(searchString) ||
                                                          r.Car.Name.Contains(searchString) ||
                                                          r.Car.LicensePlate.Contains(searchString));
            }

            if (statusFilter.HasValue)
            {
                contractsQuery = contractsQuery.Where(r => r.Status == statusFilter.Value);
            }

            return await contractsQuery.ToListAsync();
        }

        // GET: api/rentalcontracts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RentalContract>> GetRentalContract(int id)
        {
            var contract = await _context.RentalContracts
                .Include(r => r.Customer)
                .Include(r => r.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return contract;
        }

        // POST: api/rentalcontracts
        [HttpPost]
        public async Task<ActionResult<RentalContract>> PostRentalContract(RentalContract contract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra xe có sẵn không
            var car = await _context.Cars.FindAsync(contract.CarId);
            if (car == null || car.Status != CarStatus.Available)
            {
                return BadRequest(new { message = "Xe không có sẵn để thuê" });
            }

            // Tạo số hợp đồng nếu chưa có
            if (string.IsNullOrEmpty(contract.ContractNumber))
            {
                contract.ContractNumber = await GenerateContractNumber();
            }

            contract.CreatedAt = DateTime.Now;
            _context.RentalContracts.Add(contract);

            // Cập nhật trạng thái xe thành Rented
            car.Status = CarStatus.Rented;
            car.UpdatedAt = DateTime.Now;
            _context.Update(car);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRentalContract", new { id = contract.Id }, contract);
        }

        // PUT: api/rentalcontracts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRentalContract(int id, RentalContract contract)
        {
            if (id != contract.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                contract.UpdatedAt = DateTime.Now;
                _context.Update(contract);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RentalContractExists(id))
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

        // DELETE: api/rentalcontracts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRentalContract(int id)
        {
            var contract = await _context.RentalContracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái xe về Available
            var car = await _context.Cars.FindAsync(contract.CarId);
            if (car != null)
            {
                car.Status = CarStatus.Available;
                car.UpdatedAt = DateTime.Now;
                _context.Update(car);
            }

            _context.RentalContracts.Remove(contract);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/rentalcontracts/5/complete
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<object>> CompleteContract(int id)
        {
            var contract = await _context.RentalContracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy hợp đồng" });
            }

            contract.Status = ContractStatus.Completed;
            contract.UpdatedAt = DateTime.Now;
            _context.Update(contract);

            // Cập nhật trạng thái xe về Available
            var car = await _context.Cars.FindAsync(contract.CarId);
            if (car != null)
            {
                car.Status = CarStatus.Available;
                car.UpdatedAt = DateTime.Now;
                _context.Update(car);
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Hoàn thành hợp đồng thành công!" });
        }

        // POST: api/rentalcontracts/5/cancel
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<object>> CancelContract(int id)
        {
            var contract = await _context.RentalContracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy hợp đồng" });
            }

            contract.Status = ContractStatus.Cancelled;
            contract.UpdatedAt = DateTime.Now;
            _context.Update(contract);

            // Cập nhật trạng thái xe về Available
            var car = await _context.Cars.FindAsync(contract.CarId);
            if (car != null)
            {
                car.Status = CarStatus.Available;
                car.UpdatedAt = DateTime.Now;
                _context.Update(car);
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Hủy hợp đồng thành công!" });
        }

        // GET: api/rentalcontracts/5/print
        [HttpGet("{id}/print")]
        public async Task<ActionResult<object>> GetPrintData(int id)
        {
            var contract = await _context.RentalContracts
                .Include(r => r.Customer)
                .Include(r => r.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            var printData = new
            {
                contract = contract,
                customer = contract.Customer,
                car = contract.Car,
                printDate = DateTime.Now
            };

            return Ok(printData);
        }

        // GET: api/rentalcontracts/create-data
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

        private bool RentalContractExists(int id)
        {
            return _context.RentalContracts.Any(e => e.Id == id);
        }
    }
}
