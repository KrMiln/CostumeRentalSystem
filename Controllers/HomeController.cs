using CostumeRentalSystem.Models;
using CostumeRentalSystem.Services.Abstraction;
using CostumeRentalSystem.Services.Interfaces;
using CostumeRentalSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CostumeRentalSystem.Controllers;

public class HomeController : Controller
{
    private readonly ICostumeService _costumeService;
    private readonly IClientService _clientService;

    public HomeController(ICostumeService costumeService, IClientService clientService)
    {
        _costumeService = costumeService;
        _clientService = clientService;
    }

    public async Task<IActionResult> Index()
    {
        // 1. Взимаме само 3-те най-нови костюма за секция "Представени"
        var featuredPaged = await _costumeService.GetFilteredCostumesAsync(
            null, null, false, null, null, null, 1, 3);

        // 2. Взимаме статистика за наличните костюми (специфична заявка)
        // Ако още нямаш GetCountAsync, това е временно решение:
        var availablePaged = await _costumeService.GetFilteredCostumesAsync(
            null, null, true, null, null, null, 1, 1);

        // 3. Взимаме общия брой клиенти
        var clientsResult = await _clientService.GetFilteredClientsAsync(null, null, null, 1, 1);

        var viewModel = new HomeViewModel
        {
            FeaturedCostumes = featuredPaged.Items,
            TotalCostumes = featuredPaged.TotalItems,
            AvailableCount = availablePaged.TotalItems,
            TotalClients = clientsResult.TotalItems
        };

        return View(viewModel);
    }

    public IActionResult Terms()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}