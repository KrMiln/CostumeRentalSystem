using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.ViewModels;
using CostumeRentalSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CostumeRentalSystem.Controllers;

[Authorize(Roles = "Administrator,Employee,Client")]
public class RentalsController : Controller
{
    private readonly IRentalService _rentalService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RentalsController(IRentalService rentalService, UserManager<ApplicationUser> userManager)
    {
        _rentalService = rentalService;
        _userManager = userManager;
    }

    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> Index(RentalIndexViewModel model, int page = 1)
    {
        const int pageSize = 6;

        if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate > model.EndDate)
        {
            TempData["Error"] = "Началната дата не може да бъде след крайната дата!";

            model.StartDate = null;
            model.EndDate = null;
        }

        var pagedResult = await _rentalService.GetFilteredRentalsAsync(
            model.SearchString, model.StartDate, model.EndDate, model.Status, page, pageSize);

        model.Rentals = pagedResult.Items;
        model.Pagination = pagedResult.ToPaginationConfig("Rentals", nameof(Index), model.ToRouteValues());

        return View(model);
    }

    [Authorize(Roles = "Client")]
    public async Task<IActionResult> MyRentals(int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        const int pageSize = 6;
        var pagedResult = await _rentalService.GetFilteredRentalsByUserIdAsync(user.Id, page, pageSize);

        ViewData["Pagination"] = pagedResult.ToPaginationConfig("Rentals", nameof(MyRentals), new());

        return View(pagedResult.Items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var rental = await _rentalService.GetRentalByIdAsync(id.Value);
        if (rental == null) return NotFound();

        if (User.IsInRole("Client"))
        {
            var user = await _userManager.GetUserAsync(User);
            if (rental.Client?.UserId != user?.Id) return Forbid();
        }

        return View(rental);
    }

    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> Create(int? costumeId)
    {
        var model = new RentalFormViewModel { CostumeId = costumeId ?? 0 };
        await PopulateDropDowns(null, model.CostumeId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> Create(RentalFormViewModel model)
    {
        if (model.EndDate <= model.StartDate)
            ModelState.AddModelError("EndDate", "Датата на връщане трябва да е след датата на наемане.");

        if (ModelState.IsValid)
        {
            var result = await _rentalService.CreateRentalAsync(MapToEntity(model));

            if (result.Success)
            {
                TempData["Success"] = "Наемът беше създаден успешно!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.ErrorMessage);
        }

        await PopulateDropDowns(model.ClientId, model.CostumeId);
        return View(model);
    }

    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var rental = await _rentalService.GetRentalByIdAsync(id.Value);
        if (rental == null) return NotFound();

        var model = MapToViewModel(rental);
        await PopulateDropDowns(model.ClientId, model.CostumeId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> Edit(int id, RentalFormViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var result = await _rentalService.UpdateRentalAsync(MapToEntity(model));

            if (result.Success)
            {
                TempData["Success"] = "Промените по наема бяха запазени успешно!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.ErrorMessage);
        }

        await PopulateDropDowns(model.ClientId, model.CostumeId);
        return View(model);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var rental = await _rentalService.GetRentalByIdAsync(id.Value);
        return rental == null ? NotFound() : View(rental);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _rentalService.DeleteRentalAsync(id);
        if (result.Success)
            TempData["Success"] = "Наемът беше изтрит успешно!";
        else
            TempData["Error"] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropDowns(int? selectedClient = null, int? selectedCostume = null)
    {
        ViewBag.ClientId = new SelectList(await _rentalService.GetClientsAsync(), "Id", "Name", selectedClient);
        ViewBag.CostumeId = new SelectList(await _rentalService.GetAvailableCostumesAsync(selectedCostume), "Id", "Name", selectedCostume);
    }

    private Rental MapToEntity(RentalFormViewModel model) => new Rental
    {
        Id = model.Id,
        ClientId = model.ClientId,
        CostumeId = model.CostumeId,
        RentDate = model.StartDate,
        ReturnDate = model.EndDate,
        Status = model.Status
    };

    private RentalFormViewModel MapToViewModel(Rental entity) => new RentalFormViewModel
    {
        Id = entity.Id,
        ClientId = entity.ClientId,
        ClientName = entity.Client?.Name,
        CostumeId = entity.CostumeId,
        CostumeName = entity.Costume?.Name,
        CostumeImagePath = entity.Costume?.ImagePath,
        PricePerDay = entity.Costume?.PricePerDay ?? 0,
        StartDate = entity.RentDate,
        EndDate = entity.ReturnDate,
        Status = entity.Status
    };
}