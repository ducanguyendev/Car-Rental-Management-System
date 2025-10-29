using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace QuanLyChoThueXe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add Entity Framework
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add Authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Home/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                });

            // Add Authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("EmployeeOrAdmin", policy => policy.RequireRole("Employee", "Admin"));
                options.AddPolicy("CustomerOrAdmin", policy => policy.RequireRole("Customer", "Admin"));
            });

            // Add Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            
            // Add API Services
            builder.Services.AddHttpClient<ICarApiService, CarApiService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? "https://localhost:7000");
            });
            
            builder.Services.AddHttpClient<IBookingApiService, BookingApiService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? "https://localhost:7000");
            });
            
            builder.Services.AddHttpClient<ICustomerApiService, CustomerApiService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? "https://localhost:7000");
            });
            
            builder.Services.AddHttpClient<IRentalContractApiService, RentalContractApiService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? "https://localhost:7000");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
                
            // Map API routes
            app.MapControllers();

            app.Run();
        }
    }
}
