using CostumeRentalSystem.Models;
using CostumeRentalSystem.Services.Abstraction;
using CostumeRentalSystem.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CostumeRentalSystem.Controllers;

[Authorize(Roles = "Administrator")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: Categories
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();

        var model = categories.Select(c => new CategoryIndexViewModel
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();

        return View(model);
    }

    // GET: Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            var category = new Category { Name = model.Name };

            // Взимаме резултата от услугата
            var (success, errorMessage) = await _categoryService.AddAsync(category);

            if (success)
            {
                TempData["Success"] = "Категорията беше създадена успешно!";
                return RedirectToAction(nameof(Index));
            }

            // Ако не е успешно, добавяме грешката към ModelState, за да се покаже във View-то
            ModelState.AddModelError(string.Empty, errorMessage);
        }

        return View(model);
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var category = await _categoryService.GetByIdAsync(id.Value);
        if (category == null) return NotFound();

        // МАПВАНЕ: Entity -> ViewModel
        var model = new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name
        };

        return View(model);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
    {
        // Проверка за сигурност - ID-то от URL трябва да съвпада с това във формата
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            // МАПВАНЕ: ViewModel -> Entity
            var category = new Category
            {
                Id = model.Id,
                Name = model.Name
            };

            try
            {
                await _categoryService.UpdateAsync(category);
                TempData["Success"] = "Категорията беше обновена успешно!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _categoryService.ExistsAsync(model.Id)) return NotFound();
                else throw;
            }
        }

        // Ако има грешки в модела, връщаме потребителя към формата
        return View(model);
    }

    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var category = await _categoryService.GetByIdAsync(id.Value);
        if (category == null) return NotFound();

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var (success, errorMessage) = await _categoryService.DeleteAsync(id);

        if (!success)
        {
            TempData["Error"] = errorMessage;
            // Връщаме се към Index, където ще се покаже съобщението
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Категорията беше изтрита успешно!";
        return RedirectToAction(nameof(Index));
    }
}