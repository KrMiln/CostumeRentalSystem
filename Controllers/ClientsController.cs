using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Services.Interfaces;
using CostumeRentalSystem.ViewModels;
using CostumeRentalSystem.ViewModels.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CostumeRentalSystem.Controllers;

[Authorize(Roles = "Administrator,Employee")]
public class ClientsController : Controller
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    // GET: Clients
    public async Task<IActionResult> Index(ClientIndexViewModel model, int page = 1)
    {
        const int pageSize = 6;

        var pagedResult = await _clientService.GetFilteredClientsAsync(
            model.SearchName, model.SearchPhone, model.SearchEmail, page, pageSize);

        model.Clients = pagedResult.Items;
        model.Pagination = pagedResult.ToPaginationConfig("Clients", nameof(Index), model.ToRouteValues());

        return View(model);
    }

    // GET: Clients/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var client = await _clientService.GetByIdAsync(id.Value, includeRentals: true);
        if (client == null) return NotFound();

        return View(client);
    }

    // GET: Clients/Create
    public IActionResult Create()
    {
        return View(new ClientFormViewModel());
    }

    // POST: Clients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            var client = MapToEntity(model);

            // Тук разопаковаме резултата (Success и ErrorMessage)
            var result = await _clientService.CreateAsync(client);

            if (result.Success)
            {
                TempData["Success"] = "Клиентът беше добавен успешно!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Ако не е успешно, добавяме грешката от сервиза в ModelState
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }
        }

        return View(model);
    }

    // GET: Clients/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var client = await _clientService.GetByIdAsync(id.Value);
        if (client == null) return NotFound();

        var viewModel = MapToViewModel(client);

        return View(viewModel);
    }

    // POST: Clients/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClientFormViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            // 1. Извикваме сервиза и взимаме резултата
            var result = await _clientService.UpdateAsync(MapToEntity(model));

            // 2. Проверяваме дали операцията е успешна
            if (result.Success)
            {
                TempData["Success"] = "Данните на клиента бяха обновени!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // 3. Ако има дублиран имейл или телефон, показваме грешката от сервиза
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }
        }

        // Ако стигнем до тук, значи има грешка (в ModelState или в уникалността)
        return View(model);
    }

    // GET: Clients/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var client = await _clientService.GetByIdAsync(id.Value);
        if (client == null) return NotFound();

        return View(client);
    }

    // POST: Clients/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _clientService.DeleteAsync(id);
        if (result.Success)
        {
            TempData["Success"] = "Клиентът беше изтрит.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    // --- HELPERS ---

    private Client MapToEntity(ClientFormViewModel model)
    {
        return new Client
        {
            Id = model.Id,
            Name = model.Name,
            PhoneNumber = model.Phone,
            Email = model.Email,
            Notes = model.Notes
        };
    }

    private ClientFormViewModel MapToViewModel(Client entity)
    {
        return new ClientFormViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Phone = entity.PhoneNumber,
            Email = entity.Email,
            Notes = entity.Notes
        };
    }
}