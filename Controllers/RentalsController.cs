using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CostumeRentalSystem.Data;
using CostumeRentalSystem.Models;

namespace CostumeRentalSystem.Controllers;

[Authorize(Roles = "Administrator,Employee")]
public class RentalsController : Controller
{
    private readonly ApplicationDbContext _context;

    public RentalsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Списък с всички наеми (История)
    public async Task<IActionResult> Index(string searchString, DateTime? searchDate)
    {
        // Започваме с базовата заявка
        var rentalsQuery = _context.Rentals
            .Include(r => r.Client)
            .Include(r => r.Costume)
            .AsQueryable();

        // Филтър по текст (Име на клиент или Костюм)
        if (!string.IsNullOrEmpty(searchString))
        {
            rentalsQuery = rentalsQuery.Where(r =>
                r.Client.Name.Contains(searchString) ||
                r.Costume.Name.Contains(searchString));
        }

        // Филтър по дата (проверява и двете колони - наемане и връщане)
        if (searchDate.HasValue)
        {
            rentalsQuery = rentalsQuery.Where(r =>
                r.RentDate.Date == searchDate.Value.Date ||
                r.ReturnDate.Date == searchDate.Value.Date);
        }

        var rentals = await rentalsQuery.OrderByDescending(r => r.RentDate).ToListAsync();

        // Запазваме въведеното от потребителя, за да остане в полетата след търсене
        ViewData["CurrentFilter"] = searchString;
        ViewData["CurrentDate"] = searchDate?.ToString("yyyy-MM-dd");

        return View(rentals);
    }

    // Само текущо активни наеми
    public async Task<IActionResult> Active()
    {
        var rentals = await _context.Rentals
            .Include(r => r.Client)
            .Include(r => r.Costume)
            .Where(r => r.Status == RentalStatus.Active)
            .OrderBy(r => r.ReturnDate)
            .ToListAsync();
        ViewData["Subtitle"] = "Активни наеми";
        return View("Index", rentals);
    }

    // Наеми, които трябва да се върнат днес или са закъснели
    public async Task<IActionResult> DueToday()
    {
        var today = DateTime.Today;
        var rentals = await _context.Rentals
            .Include(r => r.Client)
            .Include(r => r.Costume)
            .Where(r => r.Status == RentalStatus.Active && r.ReturnDate.Date <= today)
            .OrderBy(r => r.ReturnDate)
            .ToListAsync();
        ViewData["Subtitle"] = "Дължими днес или закъснели";
        return View("Index", rentals);
    }

    // GET: Зареждане на страницата за нов наем
    public async Task<IActionResult> Create()
    {
        await PopulateDropDowns();

        var model = new Rental
        {
            RentDate = DateTime.Today,
            ReturnDate = DateTime.Today.AddDays(1),
            Status = RentalStatus.Active
        };

        return View(model);
    }

    // POST: Запис на новия наем в базата
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Rental rental)
    {
        if (ModelState.IsValid)
        {
            var costume = await _context.Costumes.FindAsync(rental.CostumeId);

            if (costume == null || !costume.IsAvailable)
            {
                ModelState.AddModelError("", "Избраният костюм в момента не е наличен.");
            }
            else
            {
                // Основна логика: Маркираме костюма като зает
                costume.IsAvailable = false;
                rental.Status = RentalStatus.Active;

                _context.Add(rental);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        await PopulateDropDowns(rental.ClientId, rental.CostumeId);
        return View(rental);
    }

    // GET: Rentals/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var rental = await _context.Rentals.FindAsync(id);
        if (rental == null) return NotFound();

        // Тук позволяваме да се виждат ВСИЧКИ костюми, 
        // защото текущият костюм на този наем е маркиран като "зает"
        ViewBag.ClientId = new SelectList(_context.Clients.OrderBy(c => c.Name), "Id", "Name", rental.ClientId);
        ViewBag.CostumeId = new SelectList(_context.Costumes.OrderBy(c => c.Name), "Id", "Name", rental.CostumeId);

        // Добавяме статусите в ViewBag за падащото меню
        ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(RentalStatus)));

        return View(rental);
    }

    // POST: Rentals/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,CostumeId,RentDate,ReturnDate,Status")] Rental rental)
    {
        if (id != rental.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Вземаме оригиналния наем от базата (без проследяване), за да видим дали костюмът е сменен
                var oldRental = await _context.Rentals.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

                // Ако костюмът е сменен или статусът е променен на Returned
                if (oldRental.CostumeId != rental.CostumeId || rental.Status == RentalStatus.Returned)
                {
                    var oldCostume = await _context.Costumes.FindAsync(oldRental.CostumeId);
                    if (oldCostume != null) oldCostume.IsAvailable = true;

                    if (rental.Status == RentalStatus.Active)
                    {
                        var newCostume = await _context.Costumes.FindAsync(rental.CostumeId);
                        if (newCostume != null) newCostume.IsAvailable = false;
                    }
                }

                _context.Update(rental);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Rentals.Any(e => e.Id == rental.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.ClientId = new SelectList(_context.Clients, "Id", "Name", rental.ClientId);
        ViewBag.CostumeId = new SelectList(_context.Costumes, "Id", "Name", rental.CostumeId);
        return View(rental);
    }

    // GET: Rentals/Delete/5
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var rental = await _context.Rentals
            .Include(r => r.Client)
            .Include(r => r.Costume)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (rental == null) return NotFound();

        return View(rental);
    }

    [HttpPost, ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var rental = await _context.Rentals
            .Include(r => r.Costume)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rental != null)
        {
            // Освобождаваме костюма, ако наемът е бил активен
            if (rental.Status == RentalStatus.Active && rental.Costume != null)
            {
                rental.Costume.IsAvailable = true;
            }

            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // Детайли за конкретен наем
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var rental = await _context.Rentals
            .Include(r => r.Client)
            .Include(r => r.Costume)
            .ThenInclude(c => c.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (rental == null) return NotFound();

        return View(rental);
    }

    // Помощен метод за пълнене на Dropdown менютата
    private async Task PopulateDropDowns(object selectedClient = null, object selectedCostume = null)
    {
        var clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
        // Показваме само костюми, които са маркирани като "Налични"
        var availableCostumes = await _context.Costumes
            .Where(c => c.IsAvailable)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.ClientId = new SelectList(clients, "Id", "Name", selectedClient);
        ViewBag.CostumeId = new SelectList(availableCostumes, "Id", "Name", selectedCostume);
    }
}