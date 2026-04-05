using CostumeRentalSystem.Common;
using CostumeRentalSystem.Data;
using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.ViewModels.Users;
using CostumeRentalSystem.ViewModels.Users;
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
    private readonly ApplicationDbContext _context;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IActionResult> Index(UserIndexViewModel model, int page = 1)
    {
        const int pageSize = 6;
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(model.SearchTerm))
        {
            var term = model.SearchTerm.Trim().ToLower();
            query = query.Where(u => u.UserName.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(model.RoleFilter))
        {
            var role = await _roleManager.FindByNameAsync(model.RoleFilter);
            if (role != null)
            {
                query = query.Where(u => _context.UserRoles.Any(ur => ur.RoleId == role.Id && ur.UserId == u.Id));
            }
        }

        var pagedUsers = await query
            .OrderBy(u => u.UserName)
            .ToPagedResultAsync(page, pageSize);

        var userList = new List<UserDetailsViewModel>();
        foreach (var user in pagedUsers.Items)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userList.Add(new UserDetailsViewModel
            {
                UserId = user.Id,
                Username = user.UserName ?? "Няма име",
                Email = user.Email ?? "Няма имейл",
                Role = roles.FirstOrDefault() ?? "Няма роля"
            });
        }

        model.Users = userList;
        model.TotalUsersCount = pagedUsers.TotalItems;

        model.Pagination = pagedUsers.ToPaginationConfig(
            "Users",
            nameof(Index),
            model.ToRouteValues());

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
            TempData["Error"] = "Ролята на администратор не може да бъде променяна.";
            return RedirectToAction(nameof(Index));
        }

        if (roleName == "Client" && user.ClientId == null)
        {
            return RedirectToAction("Create", "Clients", new { userId = user.Id });
        }

        var existingRoles = await _userManager.GetRolesAsync(user);

        await _userManager.RemoveFromRolesAsync(user, existingRoles);
        var addResult = await _userManager.AddToRoleAsync(user, roleName);

        if (!addResult.Succeeded)
        {
            TempData["Error"] = $"Грешка при добавяне на роля.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = $"Ролята беше успешно променена!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return NotFound();

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

        var roles = await _userManager.GetRolesAsync(user);

        var model = new UserDetailsViewModel
        {
            UserId = user.Id,
            Username = user.UserName ?? "Няма име",
            Email = user.Email ?? "Няма имейл",
            Role = roles.FirstOrDefault() ?? "Няма роля"
        };

        return View(model);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string userId)
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

        var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);

        if (client != null)
        {
            client.UserId = null;
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
            TempData["Success"] = "Потребителският акаунт беше изтрит.";
        else
            TempData["Error"] = "Грешка при изтриване на акаунта.";

        return RedirectToAction(nameof(Index));
    }
}