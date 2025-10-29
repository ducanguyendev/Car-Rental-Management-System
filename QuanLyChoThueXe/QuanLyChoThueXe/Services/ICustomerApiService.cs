using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Services
{
    public interface ICustomerApiService
    {
        Task<object> GetDashboardAsync();
        Task<object> GetProfileAsync();
        Task<object> UpdateProfileAsync(CustomerProfileViewModel model);
        Task<IEnumerable<Car>> SearchCarsAsync(CarSearchViewModel searchModel);
        Task<Car?> GetCarDetailsAsync(int id);
        Task<IEnumerable<Booking>> GetBookingsAsync();
        Task<object> CreateBookingAsync(CreateBookingViewModel model);
        Task<object> CancelBookingAsync(int id);
        Task<IEnumerable<RentalContract>> GetContractsAsync();
        Task<RentalContract?> GetContractDetailsAsync(int id);
        Task<object> ChangePasswordAsync(ChangePasswordViewModel model);
    }
}
