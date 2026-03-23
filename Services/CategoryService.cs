using CostumeRentalSystem.Data;
using CostumeRentalSystem.Models;
using CostumeRentalSystem.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace CostumeRentalSystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<(bool Success, string ErrorMessage)> AddAsync(Category category)
        {
            // 1. Проверка за дублиращо се име
            bool exists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());

            if (exists)
            {
                return (false, $"Категория с името '{category.Name}' вече съществува.");
            }

            // 2. Проверяваме текущия брой категории
            var count = await _context.Categories.CountAsync();

            if (count >= 10)
            {
                return (false, "Максималният брой категории (10) е достигнат. Изтрийте съществуваща категория, за да добавите нова.");
            }

            // 3. Ако всичко е наред, добавяме
            _context.Add(category);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Category category)
        {
            // 1. Проверка дали ДРУГА категория вече има това име
            // Проверяваме по име, но изключваме текущото ID
            bool exists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != category.Id);

            if (exists)
            {
                return (false, $"Вече съществува друга категория с името '{category.Name}'.");
            }

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                // В случай на неочаквана грешка (напр. проблем с базата)
                return (false, "Възникна техническа грешка при обновяването.");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Costumes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return (false, "Категорията не беше намерена.");
            }

            // ПРОВЕРКА: Има ли костюми в тази категория?
            if (category.Costumes != null && category.Costumes.Any())
            {
                return (false, $"Категорията '{category.Name}' не може да бъде изтрита, защото съдържа {category.Costumes.Count} костюма. Първо преместете или изтрийте костюмите.");
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return (true, string.Empty);
            }
            catch (Exception)
            {
                return (false, "Възникна грешка при опит за изтриване от базата данни.");
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(e => e.Id == id);
        }
    }
}
