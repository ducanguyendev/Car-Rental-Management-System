using System.Text;
using System.Text.Json;
using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Services
{
    public class BookingApiService : IBookingApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public BookingApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<Booking>> GetBookingsAsync(string? searchString = null, BookingStatus? statusFilter = null)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(searchString))
                queryParams.Add($"searchString={Uri.EscapeDataString(searchString)}");
            
            if (statusFilter.HasValue)
                queryParams.Add($"statusFilter={statusFilter.Value}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"/api/bookings{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Booking>>(json, _jsonOptions) ?? new List<Booking>();
            }
            
            return new List<Booking>();
        }

        public async Task<Booking?> GetBookingAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/bookings/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Booking>(json, _jsonOptions);
            }
            
            return null;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            var json = JsonSerializer.Serialize(booking, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/bookings", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Booking>(responseJson, _jsonOptions) ?? booking;
            }
            
            throw new HttpRequestException($"Failed to create booking: {response.StatusCode}");
        }

        public async Task<bool> UpdateBookingAsync(int id, Booking booking)
        {
            var json = JsonSerializer.Serialize(booking, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/api/bookings/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/bookings/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<object> ConfirmBookingAsync(int id)
        {
            var response = await _httpClient.PostAsync($"/api/bookings/{id}/confirm", null);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { success = false, message = "Failed to confirm booking" };
        }

        public async Task<object> CancelBookingAsync(int id)
        {
            var response = await _httpClient.PostAsync($"/api/bookings/{id}/cancel", null);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { success = false, message = "Failed to cancel booking" };
        }

        public async Task<IEnumerable<Booking>> GetCustomerBookingsAsync(int customerId)
        {
            var response = await _httpClient.GetAsync($"/api/bookings/customers/{customerId}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Booking>>(json, _jsonOptions) ?? new List<Booking>();
            }
            
            return new List<Booking>();
        }

        public async Task<object> GetCreateDataAsync()
        {
            var response = await _httpClient.GetAsync("/api/bookings/create-data");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { customers = new List<Customer>(), cars = new List<Car>() };
        }
    }
}
