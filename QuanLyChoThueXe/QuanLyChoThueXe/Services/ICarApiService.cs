using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Services
{
    public interface ICarApiService
    {
        Task<IEnumerable<Car>> GetCarsAsync(string? searchString = null, CarStatus? statusFilter = null);
        Task<Car?> GetCarAsync(int id);
        Task<Car> CreateCarAsync(Car car);
        Task<bool> UpdateCarAsync(int id, Car car);
        Task<bool> DeleteCarAsync(int id);
        Task<object> GetCarStatusAsync(int id);
        Task<object> GetCarStatsAsync(int id);
        Task<IEnumerable<Car>> GetAvailableCarsAsync();
    }
}
