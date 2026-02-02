using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CostumeRentalSystem.Data;
using CostumeRentalSystem.Models;

namespace CostumeRentalSystem.Controllers;

[Authorize(Roles = "Administrator,Employee")]
public class ClientsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Clients
    public async Task<IActionResult> Index(string searchName, string searchPhone, string searchEmail)
    {
        // Започваме с базовата заявка към всички клиенти
        var clientsQuery = _context.Clients.AsQueryable();

        // Филтър по име
        if (!string.IsNullOrEmpty(searchName))
        {
            clientsQuery = clientsQuery.Where(c => c.Name.Contains(searchName));
        }

        // Филтър по телефон
        if (!string.IsNullOrEmpty(searchPhone))
        {
            clientsQuery = clientsQuery.Where(c => c.PhoneNumber.Contains(searchPhone));
        }

        // Филтър по имейл
        if (!string.IsNullOrEmpty(searchEmail))
        {
            clientsQuery = clientsQuery.Where(c => c.Email.Contains(searchEmail));
        }

        // Запазваме стойностите във ViewBag, за да останат попълнени в полетата след търсене
        ViewBag.SearchName = searchName;
        ViewBag.SearchPhone = searchPhone;
        ViewBag.SearchEmail = searchEmail;

        var clients = await clientsQuery.OrderBy(c => c.Name).ToListAsync();
        return View(clients);
    }

    // GET: Clients/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var client = await _context.Clients
            .FirstOrDefaultAsync(m => m.Id == id);
        if (client == null)
        {
            return NotFound();
        }

        return View(client);
    }

    // GET: Clients/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Clients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client)
    {
        if (ModelState.IsValid)
        {
            _context.Add(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(client);
    }

    // GET: Clients/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            return NotFound();
        }
        return View(client);
    }

    // POST: Clients/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Client client)
    {
        if (id != client.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Clients.Any(e => e.Id == client.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(client);
    }

    // GET: Clients/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var client = await _context.Clients
            .FirstOrDefaultAsync(m => m.Id == id);
        if (client == null)
        {
            return NotFound();
        }

        return View(client);
    }

    // POST: Clients/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client != null)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}

