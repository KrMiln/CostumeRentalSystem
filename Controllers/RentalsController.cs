using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CostumeRentalSystem.Data;
using CostumeRentalSystem.Models;

namespace CostumeRentalSystem.Controllers;

[Authorize]
public class RentalsController : Controller
{
    private readonly ApplicationDbContext _context;

    public RentalsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Rentals
    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> Index()
    {
        var rentals = await _context.Rentals
            .Include(r => r.Client)
            .Include(r => r.Costume)
            .OrderByDescending(r => r.RentDate)
            .ToListAsync();
        return View(rentals);
    }

    // GET: Rentals/Active
    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> Active()
    {
        var rentals = await _context.Rentals
            .Include(r => r.Client)
            .Include(r => r.Costume)
            .Where(r => r.Status == RentalStatus.Активен)
            .OrderBy(r => r.ReturnDate)
            .ToListAsync();
        return View("Index", rentals);
    }

    // GET: Rentals/DueToday
    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> DueToday()
    {
        var today = DateTime.Today;
        var rentals = await _context.Rentals
            .Include(r => r.Client)
            .Include(r => r.Costume)
            .Where(r => r.Status == RentalStatus.Активен && r.ReturnDate.Date == today)
            .OrderBy(r => r.Client.Name)
            .ToListAsync();
        return View("Index", rentals);
    }

    // GET: Rentals/Create
    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> Create()
    {
        await PopulateDropDowns();
        var model = new Rental
        {
            RentDate = DateTime.Today,
            ReturnDate = DateTime.Today.AddDays(1),
            Status = RentalStatus.Активен
        };
        return View(model);
    }

    // POST: Rentals/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> Create(Rental rental)
        {
        if (ModelState.IsValid)
        {
            var costume = await _context.Costumes.FindAsync(rental.CostumeId);
            if (costume == null || !costume.IsAvailable)
            {
                ModelState.AddModelError(nameof(Rental.CostumeId), "Избраният костюм не е наличен за наемане.");
                await PopulateDropDowns(rental.ClientId, rental.CostumeId);
                return View(rental);
            }

            costume.IsAvailable = false;
            rental.Status = RentalStatus.Активен;

            _context.Add(rental);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropDowns(rental.ClientId, rental.CostumeId);
        return View(rental);
    }

    // GET: Rentals/Return/5
    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> Return(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var rental = await _context.Rentals
            .Include(r => r.Client)
            .Include(r => r.Costume)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (rental == null)
        {
            return NotFound();
        }

        if (rental.Status != RentalStatus.Активен)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(rental);
    }

    // POST: Rentals/Return/5
    [HttpPost, ActionName("Return")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,Employee")]
    public async Task<IActionResult> ReturnConfirmed(int id)
    {
        var rental = await _context.Rentals
            .Include(r => r.Costume)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (rental == null)
        {
            return NotFound();
        }

        if (rental.Status == RentalStatus.Активен)
        {
            rental.Status = RentalStatus.Върнат;
            if (rental.Costume != null)
            {
                rental.Costume.IsAvailable = true;
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Rentals/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var rental = await _context.Rentals
            .Include(r => r.Client)
            .Include(r => r.Costume)
            .ThenInclude(c => c.Category)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (rental == null)
        {
            return NotFound();
        }

        return View(rental);
    }

    private async Task PopulateDropDowns(object selectedClient = null, object selectedCostume = null)
    {
        ViewBag.ClientId = new SelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", selectedClient);
        ViewBag.CostumeId = new SelectList(await _context.Costumes.Where(c => c.IsAvailable).OrderBy(c => c.Name).ToListAsync(), "Id", "Name", selectedCostume);
    }
}

