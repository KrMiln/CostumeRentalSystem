using CostumeRentalSystem.Models;

namespace CostumeRentalSystem.Services.Abstraction
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<(bool Success, string ErrorMessage)> AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task<(bool Success, string ErrorMessage)> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
