using CostumeRentalSystem.Data;
using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Enums;
using CostumeRentalSystem.Models;
using CostumeRentalSystem.Services.Abstraction;
using CostumeRentalSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using static CostumeRentalSystem.Data.Entities.Costume;

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

            // 1. Филтър по име
            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(c => c.Name.Contains(searchName));

            // 2. Филтър по категория
            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId);

            // 3. Филтър за наличност
            if (onlyAvailable)
                query = query.Where(c => c.IsAvailable);

            // 4. Филтър по размер
            if (size.HasValue)
                query = query.Where(c => c.Size == size);

            // 5. Филтър по цена
            if (minPrice.HasValue)
                query = query.Where(c => c.PricePerDay >= minPrice);
            if (maxPrice.HasValue)
                query = query.Where(c => c.PricePerDay <= maxPrice);

            // Прилагаме магията на пагинацията
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
                var uploadResult = await UploadImageInternal(imageFile);
                if (!uploadResult.Success) return (false, uploadResult.ErrorMessage);
                costume.ImagePath = uploadResult.FilePath;
            }

            _context.Add(costume);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Costume costume, IFormFile? imageFile)
        {
            var existingCostume = await _context.Costumes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == costume.Id);
            if (existingCostume == null) return (false, "Костюмът не е намерен.");

            if (imageFile != null)
            {
                DeleteImageInternal(existingCostume.ImagePath);
                var uploadResult = await UploadImageInternal(imageFile);
                if (!uploadResult.Success) return (false, uploadResult.ErrorMessage);
                costume.ImagePath = uploadResult.FilePath;
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
            // Взимаме костюма заедно с наемите
            var costume = await _context.Costumes
                .Include(c => c.Rentals)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (costume == null) return (false, "Костюмът не съществува.");

            // Бизнес логика: Проверка за активни наеми
            if (costume.Rentals.Any(r => r.Status != RentalStatus.Returned))
                return (false, "Костюмът не може да бъде изтрит, защото още не е върнат!");

            DeleteImageInternal(costume.ImagePath);
            _context.Costumes.Remove(costume);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        // Private helpers moved from Controller to Service
        private async Task<(bool Success, string? FilePath, string? ErrorMessage)> UploadImageInternal(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension)) return (false, null, "Невалиден формат.");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "costumes");
            Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return (true, $"/images/costumes/{fileName}", null);
        }

        private void DeleteImageInternal(string? path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("/images/")) return;
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, path.TrimStart('/'));
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
}