using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CostumeRentalSystem.Data;

namespace CostumeRentalSystem.Controllers;

[Authorize(Roles = "Administrator")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm, string? roleFilter, int page = 1)
    {
        const int pageSize = 6;

        // 1. Стартираме заявката
        var query = _userManager.Users.AsQueryable();

        // 2. Филтриране по текст (Username или Email)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            query = query.Where(u => u.UserName.ToLower().Contains(searchTerm) ||
                                     u.Email.ToLower().Contains(searchTerm));
        }

        // 3. Филтриране по роля (Това е по-сложно при Identity, затова използваме Join)
        if (!string.IsNullOrWhiteSpace(roleFilter))
        {
            // Намираме ID-то на ролята
            var role = await _roleManager.FindByNameAsync(roleFilter);
            if (role != null)
            {
                // Взимаме само потребителите, които са в тази роля
                query = query.Where(u => _userManager.GetUsersInRoleAsync(roleFilter).Result.Select(r => r.Id).Contains(u.Id));
            }
        }

        // 4. Изчисляване на общия брой СЛЕД филтрацията
        int totalItems = await query.CountAsync();

        var users = await query
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userViewModels = new List<UserViewModel>();
        foreach (var user in users)
        {
            userViewModels.Add(new UserViewModel
            {
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            });
        }

        var model = new PagedResult<UserViewModel>
        {
            Items = userViewModels,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            TotalItems = totalItems
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string userId, string roleName)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
            return RedirectToAction(nameof(Index));

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["Error"] = "Потребителят не беше намерен.";
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(user, "Administrator"))
        {
            TempData["Error"] = "Ролята на администратор не може да бъде променяна от тук.";
            return RedirectToAction(nameof(Index));
        }

        // СЛУЧАЙ А: ПРАВИМ ГО КЛИЕНТ
        if (roleName == "Client")
        {
            if (user.ClientId == null)
            {
                // ПРАЩАМЕ ГО ДА СЪЗДАДЕ ПРОФИЛ. 
                return RedirectToAction("Create", "Clients", new { userId = user.Id });
            }
        }

        // СЛУЧАЙ Б: ВСИЧКИ ОСТАНАЛИ РОЛИ (или ако вече е бил клиент)
        var existingRoles = await _userManager.GetRolesAsync(user);
        bool wasClient = existingRoles.Contains("Client");

        var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);
        if (!removeResult.Succeeded)
        {
            TempData["Error"] = "Грешка при премахване на старите роли.";
            return RedirectToAction(nameof(Index));
        }

        var addResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!addResult.Succeeded)
        {
            TempData["Error"] = $"Грешка при добавяне на роля {roleName}.";
            return RedirectToAction(nameof(Index));
        }

        // Почистване на Clients таблицата
        if (roleName != "Client" && wasClient && user.ClientId != null)
        {
            var client = await _context.Clients.FindAsync(user.ClientId);
            if (client != null)
            {
                var hasRentals = await _context.Rentals.AnyAsync(r => r.ClientId == client.Id);
                if (!hasRentals)
                {
                    _context.Clients.Remove(client);
                    user.ClientId = null;
                    await _context.SaveChangesAsync();
                    await _userManager.UpdateAsync(user);
                }
            }
        }

        TempData["Success"] = $"Ролята на потребителя беше променена!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["Error"] = "Потребителят не беше намерен.";
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(user, "Administrator"))
        {
            TempData["Error"] = "Администраторски акаунт не може да бъде изтрит.";
            return RedirectToAction(nameof(Index));
        }

        string userName = user.UserName;

        // 1. Изтриваме първо записа в Clients, ако има такъв
        if (user.ClientId != null)
        {
            var client = await _context.Clients.FindAsync(user.ClientId);
            if (client != null)
            {
                // Проверка за наеми, преди да трием клиента (ако имаш такава бизнес логика)
                var hasRentals = await _context.Rentals.AnyAsync(r => r.ClientId == client.Id);
                if (hasRentals)
                {
                    TempData["Error"] = "Потребителят има активни наеми и не може да бъде изтрит!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
        }

        // 2. Сега трием самия потребител
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = $"Потребителят беше изтрит успешно.";
        }
        else
        {
            TempData["Error"] = "Възникна грешка при изтриването на потребителя.";
        }

        return RedirectToAction(nameof(Index));
    }
}


