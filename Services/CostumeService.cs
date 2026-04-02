using CostumeRentalSystem.Common; // Добави това за достъп до ImageHelper
using CostumeRentalSystem.Common.Enums;
using CostumeRentalSystem.Data;
using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Models;
using CostumeRentalSystem.Services.IServices;
using CostumeRentalSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CostumeRentalSystem.Services
{
    public class CostumeService : ICostumeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CostumeService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<PagedResult<Costume>> GetFilteredCostumesAsync(
           string? searchName, int? categoryId, bool onlyAvailable, CostumeSize? size, decimal? minPrice, decimal? maxPrice, int page, int pageSize)
        {
            var query = _context.Costumes
                .Include(c => c.Category)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(c => c.Name.Contains(searchName));

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId);

            if (onlyAvailable)
                query = query.Where(c => c.IsAvailable);

            if (size.HasValue)
                query = query.Where(c => c.Size == size);

            if (minPrice.HasValue)
                query = query.Where(c => c.PricePerDay >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(c => c.PricePerDay <= maxPrice);

            return await query.OrderBy(c => c.Id).ToPagedResultAsync(page, pageSize);
        }

        public async Task<Costume?> GetByIdAsync(int id)
        {
            return await _context.Costumes.Include(c => c.Category).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string ErrorMessage)> CreateAsync(Costume costume, IFormFile? imageFile)
        {
            if (imageFile != null)
            {
                // Използваме ImageHelper
                var imagePath = await ImageHelper.UploadImageAsync(imageFile, _webHostEnvironment.WebRootPath);

                if (imagePath == null)
                    return (false, "Грешка при качването на изображението.");

                costume.ImagePath = imagePath;
            }

            _context.Add(costume);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Costume costume, IFormFile? imageFile)
        {
            var existingCostume = await _context.Costumes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == costume.Id);

            if (existingCostume == null) return (false, "Костюмът не е намерен.");

            costume.IsAvailable = existingCostume.IsAvailable;

            if (imageFile != null)
            {
                // 1. Изтриваме старата снимка чрез ImageHelper
                ImageHelper.DeleteImage(existingCostume.ImagePath, _webHostEnvironment.WebRootPath);

                // 2. Качваме новата
                var newPath = await ImageHelper.UploadImageAsync(imageFile, _webHostEnvironment.WebRootPath);

                if (newPath == null) return (false, "Грешка при качването на новата снимка.");
                costume.ImagePath = newPath;
            }
            else
            {
                costume.ImagePath = existingCostume.ImagePath;
            }

            _context.Update(costume);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(int id)
        {
            var costume = await _context.Costumes
                .Include(c => c.Rentals)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (costume == null) return (false, "Костюмът не съществува.");

            if (costume.Rentals.Any(r => r.Status != RentalStatus.Returned))
                return (false, "Костюмът не може да бъде изтрит, защото още не е върнат!");

            // Използваме ImageHelper преди да изтрием записа от базата
            ImageHelper.DeleteImage(costume.ImagePath, _webHostEnvironment.WebRootPath);

            _context.Costumes.Remove(costume);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }
    }
}