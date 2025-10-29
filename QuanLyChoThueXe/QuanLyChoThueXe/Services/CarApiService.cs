using System.Text;
using System.Text.Json;
using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Services
{
    public class CarApiService : ICarApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public CarApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<Car>> GetCarsAsync(string? searchString = null, CarStatus? statusFilter = null)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(searchString))
                queryParams.Add($"searchString={Uri.EscapeDataString(searchString)}");
            
            if (statusFilter.HasValue)
                queryParams.Add($"statusFilter={statusFilter.Value}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"/api/cars{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Car>>(json, _jsonOptions) ?? new List<Car>();
            }
            
            return new List<Car>();
        }

        public async Task<Car?> GetCarAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/cars/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Car>(json, _jsonOptions);
            }
            
            return null;
        }

        public async Task<Car> CreateCarAsync(Car car)
        {
            var json = JsonSerializer.Serialize(car, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/cars", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Car>(responseJson, _jsonOptions) ?? car;
            }
            
            throw new HttpRequestException($"Failed to create car: {response.StatusCode}");
        }

        public async Task<bool> UpdateCarAsync(int id, Car car)
        {
            var json = JsonSerializer.Serialize(car, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/api/cars/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCarAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/cars/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<object> GetCarStatusAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/cars/{id}/status");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { success = false, message = "Failed to get car status" };
        }

        public async Task<object> GetCarStatsAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/cars/{id}/stats");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { success = false, message = "Failed to get car stats" };
        }

        public async Task<IEnumerable<Car>> GetAvailableCarsAsync()
        {
            var response = await _httpClient.GetAsync("/api/cars/available");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Car>>(json, _jsonOptions) ?? new List<Car>();
            }
            
            return new List<Car>();
        }
    }
}
