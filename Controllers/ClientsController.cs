using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Services.IServices;
using CostumeRentalSystem.ViewModels;
using CostumeRentalSystem.ViewModels.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CostumeRentalSystem.Controllers;

[Authorize(Roles = "Administrator,Employee")]
public class ClientsController : Controller
{
    private readonly IClientService _clientService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ClientsController(IClientService clientService, UserManager<ApplicationUser> userManager)
    {
        _clientService = clientService;
        _userManager = userManager;
    }

    // GET: Clients
    public async Task<IActionResult> Index(ClientIndexViewModel model, int page = 1)
    {
        const int pageSize = 6;

        var pagedResult = await _clientService.GetFilteredClientsAsync(
            model.SearchName, model.SearchPhone, model.SearchEmail, page, pageSize);

        model.Clients = pagedResult.Items;
        model.Pagination = pagedResult.ToPaginationConfig(
            "Clients", nameof(Index), model.ToRouteValues());

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
    [HttpGet]
    public async Task<IActionResult> Create(string? userId)
    {
        var viewModel = new ClientFormViewModel();

        if (!string.IsNullOrEmpty(userId))
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                viewModel.UserId = user.Id;
                viewModel.Email = user.Email;
            }
        }

        return View(viewModel);
    }

    // POST: Clients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var client = MapToEntity(model);
        var result = await _clientService.CreateAsync(client);

        if (result.Success)
        {
            if (!string.IsNullOrEmpty(model.UserId))
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user != null)
                {
                    user.ClientId = client.Id;
                    await _userManager.UpdateAsync(user);

                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, "Client");

                    await _userManager.UpdateSecurityStampAsync(user);

                    TempData["Success"] = "Клиентът беше създаден и ролята беше променена!";
                    return RedirectToAction("Index", "Users");
                }
            }

            TempData["Success"] = "Клиентът беше създаден успешно!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage);
        return View(model);
    }

    // GET: Clients/Edit/5
    [HttpGet]
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
            try
            {
                await _clientService.UpdateAsync(MapToEntity(model));
                TempData["Success"] = "Данните на клиента бяха обновени успешно!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _clientService.ExistsAsync(model.Id)) return NotFound();
                else throw;
            }
        }

        return View(model);
    }

    // GET: Clients/Delete/5
    [HttpGet]
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
        var client = await _clientService.GetByIdAsync(id);
        string? associatedUserId = client?.UserId;

        var result = await _clientService.DeleteAsync(id);

        if (result.Success)
        {
            if (!string.IsNullOrEmpty(associatedUserId))
            {
                var user = await _userManager.FindByIdAsync(associatedUserId);
                if (user != null)
                {
                    user.ClientId = null; 
                    await _userManager.UpdateAsync(user);
                }
            }

            TempData["Success"] = "Клиентът беше изтрит успешно и връзката с потребителя беше прекъсната!";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    private Client MapToEntity(ClientFormViewModel model)
    {
        return new Client
        {
            Id = model.Id,
            Name = model.Name,
            PhoneNumber = model.Phone,
            Email = model.Email,
            Notes = model.Notes,
            UserId = model.UserId
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
            Notes = entity.Notes,
            UserId = entity.UserId
        };
    }
}
