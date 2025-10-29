using System.Text;
using System.Text.Json;
using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Services
{
    public class RentalContractApiService : IRentalContractApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public RentalContractApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<RentalContract>> GetRentalContractsAsync(string? searchString = null, ContractStatus? statusFilter = null)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(searchString))
                queryParams.Add($"searchString={Uri.EscapeDataString(searchString)}");
            
            if (statusFilter.HasValue)
                queryParams.Add($"statusFilter={statusFilter.Value}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"/api/rentalcontracts{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<RentalContract>>(json, _jsonOptions) ?? new List<RentalContract>();
            }
            
            return new List<RentalContract>();
        }

        public async Task<RentalContract?> GetRentalContractAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/rentalcontracts/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RentalContract>(json, _jsonOptions);
            }
            
            return null;
        }

        public async Task<RentalContract> CreateRentalContractAsync(RentalContract contract)
        {
            var json = JsonSerializer.Serialize(contract, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/rentalcontracts", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RentalContract>(responseJson, _jsonOptions) ?? contract;
            }
            
            throw new HttpRequestException($"Failed to create rental contract: {response.StatusCode}");
        }

        public async Task<bool> UpdateRentalContractAsync(int id, RentalContract contract)
        {
            var json = JsonSerializer.Serialize(contract, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/api/rentalcontracts/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRentalContractAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/rentalcontracts/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<object> CompleteContractAsync(int id)
        {
            var response = await _httpClient.PostAsync($"/api/rentalcontracts/{id}/complete", null);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { success = false, message = "Failed to complete contract" };
        }

        public async Task<object> CancelContractAsync(int id)
        {
            var response = await _httpClient.PostAsync($"/api/rentalcontracts/{id}/cancel", null);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { success = false, message = "Failed to cancel contract" };
        }

        public async Task<object> GetPrintDataAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/rentalcontracts/{id}/print");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { };
        }

        public async Task<object> GetCreateDataAsync()
        {
            var response = await _httpClient.GetAsync("/api/rentalcontracts/create-data");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(json, _jsonOptions) ?? new { };
            }
            
            return new { customers = new List<Customer>(), cars = new List<Car>() };
        }
    }
}
