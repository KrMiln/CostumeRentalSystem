using CostumeRentalSystem.Common;
using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Models;
using CostumeRentalSystem.ViewModels;

namespace CostumeRentalSystem.Services.IServices
{
    public interface IClientService
    {
        Task<PagedResult<Client>> GetFilteredClientsAsync(
            string? searchName, string? searchPhone, string? searchEmail, int page, int pageSize);
        Task<Client?> GetByIdAsync(int id, bool includeRentals = false);
        Task<(bool Success, string ErrorMessage)> CreateAsync(Client client);
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Client client);
        Task<(bool Success, string ErrorMessage)> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}