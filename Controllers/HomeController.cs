using CostumeRentalSystem.Models;
using CostumeRentalSystem.Services.IServices;
using CostumeRentalSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
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
        var featuredPaged = await _costumeService.GetFilteredCostumesAsync(
            null, null, false, null, null, null, 1, 3);

        var availablePaged = await _costumeService.GetFilteredCostumesAsync(
            null, null, true, null, null, null, 1, 1);

        var clientsResult = await _clientService.GetFilteredClientsAsync(
            null, null, null, 1, 1);

        var viewModel = new HomeViewModel
        {
            FeaturedCostumes = featuredPaged.Items,
            TotalCostumes = featuredPaged.TotalItems,
            AvailableCount = availablePaged.TotalItems,
            TotalClients = clientsResult.TotalItems
        };

        return View(viewModel);
    }

    public IActionResult Contact()
    {
        return View(new ContactFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress("Тестов Потребител", model.Email));
            email.To.Add(new MailboxAddress("Админ", "admin@costume-shop.com"));
            email.Subject = model.Subject ?? "Ново съобщение";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
                <div style='font-family: Arial; padding: 20px; border: 1px solid #8e44ad;'>
                    <h2 style='color: #8e44ad;'>Ново запитване</h2>
                    <p><strong>От:</strong> {model.Name}</p>
                    <p><strong>Имейл:</strong> {model.Email}</p>
                    <hr>
                    <p>{model.Message}</p>
                </div>"
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("sandbox.smtp.mailtrap.io", 2525, MailKit.Security.SecureSocketOptions.StartTls);

                await client.AuthenticateAsync("d5a237b1d366ad", "78c9ee15a8da99");

                await client.SendAsync(email);
                await client.DisconnectAsync(true);
            }

            TempData["Success"] = "Съобщението е изпратено успешно!";
            ViewBag.IsSent = true;
            ModelState.Clear();
            return View(new ContactFormViewModel());
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Грешка при изпращане: " + ex.Message);
            return View(model);
        }
    }

    public IActionResult Terms()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}