using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChoThueXe.Services
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(RegisterViewModel model);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);
        Task LogActivityAsync(string action, string description, int userId, string ipAddress);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.Customer)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            // Update last login
            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                    return false;

                // Create customer
                var customer = new Customer
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    IdentityNumber = model.IdentityNumber,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Address = model.Address,
                    Occupation = model.Occupation,
                    Workplace = model.Workplace,
                    Status = CustomerStatus.Active,
                    CreatedAt = DateTime.Now
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Create user account
                var user = new User
                {
                    Username = model.Email.Split('@')[0], // Use email prefix as username
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = UserRole.Customer,
                    IsActive = true,
                    CustomerId = customer.Id,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task LogActivityAsync(string action, string description, int userId, string ipAddress)
        {
            var log = new SystemLog
            {
                Action = action,
                Description = description,
                UserId = userId.ToString(),
                UserName = await _context.Users.Where(u => u.Id == userId).Select(u => u.Username).FirstOrDefaultAsync() ?? "Unknown",
                IpAddress = ipAddress,
                Timestamp = DateTime.Now,
                Level = Models.LogLevel.Info
            };

            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
