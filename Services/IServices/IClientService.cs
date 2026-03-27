using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Models;
using CostumeRentalSystem.ViewModels;

namespace CostumeRentalSystem.Services.Interfaces
{
    public interface IClientService
    {
        Task<PagedResult<Client>> GetFilteredClientsAsync(
            string? searchName, string? searchPhone, string? searchEmail, int page, int pageSize);
        Task<Client?> GetByIdAsync(int id, bool includeRentals = false);
        Task<bool> CreateAsync(Client client);
        Task<bool> UpdateAsync(Client client);
        Task<(bool Success, string ErrorMessage)> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}