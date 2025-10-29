using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Services
{
    public interface IBookingApiService
    {
        Task<IEnumerable<Booking>> GetBookingsAsync(string? searchString = null, BookingStatus? statusFilter = null);
        Task<Booking?> GetBookingAsync(int id);
        Task<Booking> CreateBookingAsync(Booking booking);
        Task<bool> UpdateBookingAsync(int id, Booking booking);
        Task<bool> DeleteBookingAsync(int id);
        Task<object> ConfirmBookingAsync(int id);
        Task<object> CancelBookingAsync(int id);
        Task<IEnumerable<Booking>> GetCustomerBookingsAsync(int customerId);
        Task<object> GetCreateDataAsync();
    }
}
