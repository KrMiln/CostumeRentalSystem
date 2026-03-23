using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CostumeRentalSystem.Controllers;

[Authorize(Roles = "Administrator")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction(nameof(Index));
        }

        // Не променяме администратора
        if (await _userManager.IsInRoleAsync(user, "Administrator"))
        {
            return RedirectToAction(nameof(Index));
        }

        var existingRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, existingRoles);

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        await _userManager.AddToRoleAsync(user, roleName);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(user, "Administrator"))
        {
            return RedirectToAction(nameof(Index));
        }

        await _userManager.DeleteAsync(user);
        return RedirectToAction(nameof(Index));
    }
}


