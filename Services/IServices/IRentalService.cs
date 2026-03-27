using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Enums;
using CostumeRentalSystem.ViewModels;

namespace CostumeRentalSystem.Abstraction
{
    namespace CostumeRentalSystem.Services.Interfaces
    {
        public interface IRentalService
        {
            // Страницирано търсене за общи наеми
            Task<PagedResult<Rental>> GetFilteredRentalsAsync(
                string? searchString, DateTime? startDate, DateTime? endDate, RentalStatus? status, int page, int pageSize);

            // Страницирано търсене за наеми на конкретен клиент
            Task<PagedResult<Rental>> GetFilteredRentalsByEmailAsync(string email, int page, int pageSize);

            // CRUD операции с вградена бизнес логика
            Task<Rental?> GetRentalByIdAsync(int id);
            Task<(bool Success, string ErrorMessage)> CreateRentalAsync(Rental rental);
            Task<(bool Success, string ErrorMessage)> UpdateRentalAsync(Rental rental);
            Task<(bool Success, string ErrorMessage)> DeleteRentalAsync(int id);
            Task<IEnumerable<Client>> GetClientsAsync();
            Task<IEnumerable<Costume>> GetAvailableCostumesAsync(int? currentCostumeId = null);
        }
    }
}
