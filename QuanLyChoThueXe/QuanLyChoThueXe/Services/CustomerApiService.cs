using System.Text;
using System.Text.Json;
using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Services
{
    public class CustomerApiService : ICustomerApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public CustomerApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<object> GetDashboardAsync()
        {
            var response = await _httpClient.GetAsync("/api/customers/dashboard");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { };
        }

        public async Task<object> GetProfileAsync()
        {
            var response = await _httpClient.GetAsync("/api/customers/profile");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { };
        }

        public async Task<object> UpdateProfileAsync(CustomerProfileViewModel model)
        {
            var json = JsonSerializer.Serialize(model, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync("/api/customers/profile", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(responseJson, _jsonOptions) ?? new { };
            }
            
            return new { success = false, message = "Failed to update profile" };
        }

        public async Task<IEnumerable<Car>> SearchCarsAsync(CarSearchViewModel searchModel)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(searchModel.CarType))
                queryParams.Add($"carType={Uri.EscapeDataString(searchModel.CarType)}");
            
            if (!string.IsNullOrEmpty(searchModel.Brand))
                queryParams.Add($"brand={Uri.EscapeDataString(searchModel.Brand)}");
            
            if (searchModel.MinYear.HasValue)
                queryParams.Add($"minYear={searchModel.MinYear.Value}");
            
            if (searchModel.MaxYear.HasValue)
                queryParams.Add($"maxYear={searchModel.MaxYear.Value}");
            
            if (searchModel.StartDate.HasValue)
                queryParams.Add($"startDate={searchModel.StartDate.Value:yyyy-MM-dd}");
            
            if (searchModel.EndDate.HasValue)
                queryParams.Add($"endDate={searchModel.EndDate.Value:yyyy-MM-dd}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"/api/customers/search-cars{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Car>>(json, _jsonOptions) ?? new List<Car>();
            }
            
            return new List<Car>();
        }

        public async Task<Car?> GetCarDetailsAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/customers/car-details/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Car>(json, _jsonOptions);
            }
            
            return null;
        }

        public async Task<IEnumerable<Booking>> GetBookingsAsync()
        {
            var response = await _httpClient.GetAsync("/api/customers/bookings");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Booking>>(json, _jsonOptions) ?? new List<Booking>();
            }
            
            return new List<Booking>();
        }

        public async Task<object> CreateBookingAsync(CreateBookingViewModel model)
        {
            var json = JsonSerializer.Serialize(model, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/customers/create-booking", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(responseJson, _jsonOptions) ?? new { };
            }
            
            return new { success = false, message = "Failed to create booking" };
        }

        public async Task<object> CancelBookingAsync(int id)
        {
            var response = await _httpClient.PostAsync($"/api/customers/cancel-booking/{id}", null);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { success = false, message = "Failed to cancel booking" };
        }

        public async Task<IEnumerable<RentalContract>> GetContractsAsync()
        {
            var response = await _httpClient.GetAsync("/api/customers/contracts");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<RentalContract>>(json, _jsonOptions) ?? new List<RentalContract>();
            }
            
            return new List<RentalContract>();
        }

        public async Task<RentalContract?> GetContractDetailsAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/customers/contract-details/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RentalContract>(json, _jsonOptions);
            }
            
            return null;
        }

        public async Task<object> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            var json = JsonSerializer.Serialize(model, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/customers/change-password", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(responseJson, _jsonOptions) ?? new { };
            }
            
            return new { success = false, message = "Failed to change password" };
        }
    }
}
