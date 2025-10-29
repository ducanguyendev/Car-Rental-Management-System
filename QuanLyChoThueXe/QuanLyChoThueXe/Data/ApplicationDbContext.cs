using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<RentalContract> RentalContracts { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Car
            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LicensePlate).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Brand).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Model).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(30);
                entity.Property(e => e.FuelType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PricePerDay).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ImageUrl).HasMaxLength(200);
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Cấu hình Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(15);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IdentityNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Occupation).HasMaxLength(50);
                entity.Property(e => e.Workplace).HasMaxLength(200);
                entity.Property(e => e.Gender).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Cấu hình Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Bookings)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Car)
                    .WithMany(c => c.Bookings)
                    .HasForeignKey(e => e.CarId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình RentalContract
            modelBuilder.Entity<RentalContract>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ContractNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PricePerDay).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Deposit).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Terms).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.RentalContracts)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Car)
                    .WithMany(c => c.RentalContracts)
                    .HasForeignKey(e => e.CarId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Type).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Car)
                    .WithMany()
                    .HasForeignKey(e => e.CarId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).HasConversion<int>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Cấu hình Employee
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(15);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IdentityNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Position).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Gender).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.IdentityNumber).IsUnique();
            });

            // Cấu hình SystemLog
            modelBuilder.Entity<SystemLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.Level).HasConversion<int>();
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETDATE()");
            });

            // Cấu hình SystemConfiguration
            modelBuilder.Entity<SystemConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Value).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Key).IsUnique();
            });

            // Cấu hình Invoice
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Type).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.PaymentDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.RentalContract)
                    .WithMany()
                    .HasForeignKey(e => e.RentalContractId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Cars
            modelBuilder.Entity<Car>().HasData(
                new Car
                {
                    Id = 1,
                    Name = "Toyota Vios",
                    LicensePlate = "30A-12345",
                    Brand = "Toyota",
                    Model = "Vios",
                    Year = 2023,
                    Type = "Sedan",
                    Seats = 5,
                    FuelType = "Xăng",
                    PricePerDay = 500000,
                    Description = "Xe sedan tiết kiệm nhiên liệu, phù hợp cho gia đình",
                    Status = CarStatus.Available,
                    CreatedAt = DateTime.Now
                },
                new Car
                {
                    Id = 2,
                    Name = "Honda CR-V",
                    LicensePlate = "30B-67890",
                    Brand = "Honda",
                    Model = "CR-V",
                    Year = 2022,
                    Type = "SUV",
                    Seats = 7,
                    FuelType = "Xăng",
                    PricePerDay = 800000,
                    Description = "SUV cao cấp, không gian rộng rãi",
                    Status = CarStatus.Available,
                    CreatedAt = DateTime.Now
                },
                new Car
                {
                    Id = 3,
                    Name = "Ford Ranger",
                    LicensePlate = "30C-11111",
                    Brand = "Ford",
                    Model = "Ranger",
                    Year = 2023,
                    Type = "Pickup",
                    Seats = 5,
                    FuelType = "Dầu",
                    PricePerDay = 700000,
                    Description = "Xe bán tải mạnh mẽ, phù hợp cho công việc",
                    Status = CarStatus.Rented,
                    CreatedAt = DateTime.Now
                }
            );

            // Seed Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    Id = 1,
                    FullName = "Nguyễn Văn A",
                    PhoneNumber = "0123456789",
                    Email = "nguyenvana@email.com",
                    IdentityNumber = "123456789",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = Gender.Male,
                    Address = "123 Đường ABC, Quận 1, TP.HCM",
                    Occupation = "Kỹ sư",
                    Status = CustomerStatus.Active,
                    CreatedAt = DateTime.Now
                },
                new Customer
                {
                    Id = 2,
                    FullName = "Trần Thị B",
                    PhoneNumber = "0987654321",
                    Email = "tranthib@email.com",
                    IdentityNumber = "987654321",
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Gender = Gender.Female,
                    Address = "456 Đường XYZ, Quận 2, TP.HCM",
                    Occupation = "Giáo viên",
                    Status = CustomerStatus.Active,
                    CreatedAt = DateTime.Now
                }
            );

            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

            // Seed System Configurations
            modelBuilder.Entity<SystemConfiguration>().HasData(
                new SystemConfiguration
                {
                    Id = 1,
                    Key = "MaxCancelTime",
                    Value = "24",
                    Description = "Thời gian tối đa cho phép hủy đặt xe (giờ)",
                    UpdatedBy = "System",
                    UpdatedAt = DateTime.Now
                },
                new SystemConfiguration
                {
                    Id = 2,
                    Key = "DepositRate",
                    Value = "50",
                    Description = "Tỷ lệ tiền đặt cọc (%)",
                    UpdatedBy = "System",
                    UpdatedAt = DateTime.Now
                }
            );
        }
    }
}
