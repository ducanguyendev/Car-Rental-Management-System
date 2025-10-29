using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Services
{
    public interface IRentalContractApiService
    {
        Task<IEnumerable<RentalContract>> GetRentalContractsAsync(string? searchString = null, ContractStatus? statusFilter = null);
        Task<RentalContract?> GetRentalContractAsync(int id);
        Task<RentalContract> CreateRentalContractAsync(RentalContract contract);
        Task<bool> UpdateRentalContractAsync(int id, RentalContract contract);
        Task<bool> DeleteRentalContractAsync(int id);
        Task<object> CompleteContractAsync(int id);
        Task<object> CancelContractAsync(int id);
        Task<object> GetPrintDataAsync(int id);
        Task<object> GetCreateDataAsync();
    }
}
