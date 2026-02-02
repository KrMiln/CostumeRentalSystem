using CostumeRentalSystem.Data; 
using CostumeRentalSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; 
using System.Diagnostics;

namespace CostumeRentalSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Взимаме малко статистика за началната страница
        ViewBag.TotalCostumes = await _context.Costumes.CountAsync();
        ViewBag.AvailableCount = await _context.Costumes.CountAsync(c => c.IsAvailable);

        // Взимаме 3 случайни налични костюма за секция "Препоръчани"
        // (Guid.NewGuid() е трик за разбъркване в SQL)
        var featuredCostumes = await _context.Costumes
            .Where(c => c.IsAvailable)
            .OrderBy(c => Guid.NewGuid())
            .Take(3)
            .Include(c => c.Category)
            .ToListAsync();

        return View(featuredCostumes);
    }

    public IActionResult Contact()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> MyAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        // Изпращаме данните към изгледа чрез ViewBag
        ViewBag.Email = user.Email;
        ViewBag.Username = user.UserName;
        ViewBag.Id = user.Id;
        ViewBag.Roles = roles; // Списък с ролите (Admin, Client и т.н.)

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}