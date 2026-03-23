using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Services.Abstraction;
using CostumeRentalSystem.ViewModels;
using CostumeRentalSystem.ViewModels.Rentals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static CostumeRentalSystem.Data.Entities.Costume;

namespace CostumeRentalSystem.Controllers;

[Authorize]
public class CostumesController : Controller
{
    private readonly ICostumeService _costumeService;

    public CostumesController(ICostumeService costumeService)
    {
        _costumeService = costumeService;
    }

    // --- READ OPERATIONS ---

    [AllowAnonymous]
    public async Task<IActionResult> Index(CostumeIndexViewModel model, int page = 1)
    {
        const int pageSize = 8;

        // 1. Валидация на филтрите
        if (model.MinPrice > model.MaxPrice)
        {
            TempData["Error"] = "Минималната цена не може да бъде по-висока от максималната!";
            model.MinPrice = null;
            model.MaxPrice = null;

            ModelState.Remove(nameof(model.MinPrice));
            ModelState.Remove(nameof(model.MaxPrice));
        }

        CostumeSize? selectedSize = null;
        if (!string.IsNullOrEmpty(model.SelectedSize) && Enum.TryParse<CostumeSize>(model.SelectedSize, out var parsedSize))
            selectedSize = parsedSize;

        // 2. Използваме оптимизираната услуга (връща PagedResult)
        var pagedResult = await _costumeService.GetFilteredCostumesAsync(
            model.SearchName, model.CategoryId, model.OnlyAvailable, selectedSize, model.MinPrice, model.MaxPrice, page, pageSize);

        // 3. Мапване на резултатите към модела
        model.Costumes = pagedResult.Items;
        model.Pagination = pagedResult.ToPaginationConfig("Costumes", nameof(Index), model.ToRouteValues());

        // 4. Подготовка на Dropdowns
        model.Categories = new SelectList(await _costumeService.GetCategoriesAsync(), "Id", "Name", model.CategoryId);
        model.SizeList = new SelectList(Enum.GetValues(typeof(CostumeSize)));

        return View(model);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var costume = await _costumeService.GetByIdAsync(id.Value);
        return costume == null ? NotFound() : View(costume);
    }

    // --- CREATE OPERATIONS ---

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create()
    {
        var model = new CostumeFormViewModel
        {
            Categories = await GetCategoriesSelectList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(CostumeFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            var costume = MapToEntity(model);
            var result = await _costumeService.CreateAsync(costume, model.ImageFile);

            if (result.Success)
            {
                TempData["Success"] = "Костюмът е създаден успешно!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.ErrorMessage);
        }

        model.Categories = await GetCategoriesSelectList(model.CategoryId);
        return View(model);
    }

    // --- UPDATE OPERATIONS ---

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var costume = await _costumeService.GetByIdAsync(id.Value);
        if (costume == null) return NotFound();

        var model = MapToViewModel(costume);
        model.Categories = await GetCategoriesSelectList(costume.CategoryId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int id, CostumeFormViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var costume = MapToEntity(model);
            var result = await _costumeService.UpdateAsync(costume, model.ImageFile);

            if (result.Success)
            {
                TempData["Success"] = "Костюмът е обновен успешно!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.ErrorMessage);
        }

        model.Categories = await GetCategoriesSelectList(model.CategoryId);
        return View(model);
    }

    // --- DELETE OPERATIONS ---

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var costume = await _costumeService.GetByIdAsync(id.Value);
        return costume == null ? NotFound() : View(costume);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _costumeService.DeleteAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Костюмът е изтрит успешно!";
        return RedirectToAction(nameof(Index));
    }

    // --- HELPERS ---

    private async Task<SelectList> GetCategoriesSelectList(object? selected = null)
    {
        var categories = await _costumeService.GetCategoriesAsync();
        return new SelectList(categories, "Id", "Name", selected);
    }

    private Costume MapToEntity(CostumeFormViewModel model)
    {
        return new Costume
        {
            Id = model.Id,
            Name = model.Name,
            CategoryId = model.CategoryId,
            Size = model.Size ?? CostumeSize.M,
            PricePerDay = model.PricePerDay,
            IsAvailable = model.IsAvailable,
            Notes = model.Notes,
            ImagePath = model.ExistingImagePath
        };
    }

    private CostumeFormViewModel MapToViewModel(Costume entity)
    {
        return new CostumeFormViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            CategoryId = entity.CategoryId,
            Size = entity.Size,
            PricePerDay = entity.PricePerDay,
            IsAvailable = entity.IsAvailable,
            Notes = entity.Notes,
            ExistingImagePath = entity.ImagePath
        };
    }
}