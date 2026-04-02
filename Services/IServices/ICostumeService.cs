using CostumeRentalSystem.Common;
using CostumeRentalSystem.Common.Enums;
using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Models;
using CostumeRentalSystem.ViewModels;

namespace CostumeRentalSystem.Services.IServices
{
    public interface ICostumeService
    {
        Task<PagedResult<Costume>> GetFilteredCostumesAsync(
            string? searchName, int? categoryId, bool onlyAvailable, CostumeSize? size, decimal? minPrice, decimal? maxPrice, int page, int pageSize);
        Task<Costume?> GetByIdAsync(int id);

        Task<(bool Success, string ErrorMessage)> CreateAsync(Costume costume, IFormFile? imageFile);
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Costume costume, IFormFile? imageFile);
        Task<(bool Success, string ErrorMessage)> DeleteAsync(int id);
        Task<IEnumerable<Category>> GetCategoriesAsync();
    }
}
