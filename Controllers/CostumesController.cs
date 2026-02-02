using CostumeRentalSystem.Data;
using CostumeRentalSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static CostumeRentalSystem.Models.Costume;

namespace CostumeRentalSystem.Controllers;

[Authorize]
public class CostumesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CostumesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Costumes
    [AllowAnonymous]
    public async Task<IActionResult> Index(string searchName, int? categoryId, bool onlyAvailable, string size, decimal? minPrice, decimal? maxPrice)
    {
        var costumesQuery = _context.Costumes.Include(c => c.Category).AsQueryable();

        // Филтър по име
        if (!string.IsNullOrEmpty(searchName))
            costumesQuery = costumesQuery.Where(c => c.Name.Contains(searchName));

        // Филтър по категория
        if (categoryId.HasValue)
            costumesQuery = costumesQuery.Where(c => c.CategoryId == categoryId);

        // Филтър за наличност
        if (onlyAvailable)
            costumesQuery = costumesQuery.Where(c => c.IsAvailable);

        if (!string.IsNullOrEmpty(size))
        {
            // Опитваме се да превърнем стринга в стойност от Enum-а
            if (Enum.TryParse<CostumeSize>(size, out var selectedSize))
            {
                costumesQuery = costumesQuery.Where(c => c.Size == selectedSize);
            }
        }

        // НОВО: Филтър по цена (диапазон)
        if (minPrice.HasValue)
            costumesQuery = costumesQuery.Where(c => c.PricePerDay >= minPrice.Value);

        if (maxPrice.HasValue)
            costumesQuery = costumesQuery.Where(c => c.PricePerDay <= maxPrice.Value);

        // Запазваме стойностите за изгледа
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", categoryId);
        ViewBag.SearchName = searchName;
        ViewBag.OnlyAvailable = onlyAvailable;
        ViewBag.Size = size;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;

        // За да работи правилно Dropdown менюто в изгледа:
        ViewBag.SizeList = new SelectList(Enum.GetValues(typeof(CostumeSize)));

        // Запазваме избрания размер, за да остане селектиран
        ViewBag.SelectedSize = size;

        return View(await costumesQuery.ToListAsync());
    }

    // GET: Costumes/Details/5
    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var costume = await _context.Costumes
            .Include(c => c.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (costume == null)
        {
            return NotFound();
        }

        return View(costume);
    }

    // GET: Costumes/Create
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesDropDownList();
        return View();
    }

    // POST: Costumes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(Costume costume, IFormFile imageFile)
    {
        // 1. Премахваме валидацията за ImageUrl, защото го попълваме програмно
        ModelState.Remove("ImageUrl");

        if (imageFile != null && imageFile.Length > 0)
        {
            var uploadResult = await UploadImage(imageFile);
            if (uploadResult.Success)
            {
                costume.ImageUrl = uploadResult.FilePath;
            }
            else
            {
                ModelState.AddModelError("imageFile", uploadResult.ErrorMessage);
            }
        }

        if (ModelState.IsValid)
        {
            _context.Add(costume);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Костюмът е създаден успешно!";
            return RedirectToAction(nameof(Index));
        }

        await PopulateCategoriesDropDownList(costume.CategoryId);
        return View(costume);
    }

    // GET: Costumes/Edit/5
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var costume = await _context.Costumes.FindAsync(id);
        if (costume == null)
        {
            return NotFound();
        }
        await PopulateCategoriesDropDownList(costume.CategoryId);
        return View(costume);
    }

    // POST: Costumes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int id, Costume costume, IFormFile imageFile)
    {
        if (id != costume.Id) return NotFound();

        var existingCostume = await _context.Costumes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

        if (imageFile != null && imageFile.Length > 0)
        {
            // Изтриване на старата снимка
            if (!string.IsNullOrEmpty(existingCostume?.ImageUrl) && existingCostume.ImageUrl.StartsWith("/images/"))
            {
                DeleteImage(existingCostume.ImageUrl);
            }

            var uploadResult = await UploadImage(imageFile);
            if (uploadResult.Success)
            {
                costume.ImageUrl = uploadResult.FilePath;
            }
            else
            {
                ModelState.AddModelError("imageFile", uploadResult.ErrorMessage);
            }
        }
        else
        {
            // Запазваме стария път, ако не е качен нов файл
            costume.ImageUrl = existingCostume?.ImageUrl;
        }

        ModelState.Remove("imageFile");
        ModelState.Remove("ImageUrl");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(costume);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Костюмът е обновен успешно!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Costumes.Any(e => e.Id == costume.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        await PopulateCategoriesDropDownList(costume.CategoryId);
        return View(costume);
    }

    // GET: Costumes/Delete/5
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var costume = await _context.Costumes
            .Include(c => c.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (costume == null)
        {
            return NotFound();
        }

        return View(costume);
    }

    // POST: Costumes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var costume = await _context.Costumes.FindAsync(id);
        if (costume != null)
        {
            // Delete the image file if it exists
            if (!string.IsNullOrEmpty(costume.ImageUrl) && costume.ImageUrl.StartsWith("/images/"))
            {
                DeleteImage(costume.ImageUrl);
            }

            _context.Costumes.Remove(costume);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Костюмът е изтрит успешно!";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesDropDownList(object selectedCategory = null)
    {
        var categoriesQuery = _context.Categories.OrderBy(c => c.Name);
        ViewBag.CategoryId = new SelectList(await categoriesQuery.ToListAsync(), "Id", "Name", selectedCategory);
    }

    private async Task<(bool Success, string FilePath, string ErrorMessage)> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return (false, null, "Моля изберете файл.");
        }

        // Check file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            return (false, null, "Файлът е твърде голям. Максималният размер е 5MB.");
        }

        // Check file extension
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return (false, null, "Невалиден формат на файла. Разрешени формати: JPG, PNG, GIF, WEBP.");
        }

        try
        {
            // Create unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";

            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "costumes");
            Directory.CreateDirectory(uploadsFolder);

            // Full file path
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database
            return (true, $"/images/costumes/{fileName}", null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Грешка при качване на файла: {ex.Message}");
        }
    }

    private void DeleteImage(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return;

        try
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Грешка при изтриване на снимка: {ex.Message}");
        }
    }
}