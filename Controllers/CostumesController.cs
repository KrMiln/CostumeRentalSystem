using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CostumeRentalSystem.Data;
using CostumeRentalSystem.Models;

namespace CostumeRentalSystem.Controllers;

[Authorize]
public class CostumesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CostumesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Costumes
    [AllowAnonymous]
    public async Task<IActionResult> Index(int? categoryId, bool? onlyAvailable, string searchName)
    {
        var query = _context.Costumes
            .Include(c => c.Category)
            .AsQueryable();

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }

        if (onlyAvailable.HasValue && onlyAvailable.Value)
        {
            query = query.Where(c => c.IsAvailable);
        }

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            query = query.Where(c => c.Name.Contains(searchName));
        }

        ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.OnlyAvailable = onlyAvailable ?? false;
        ViewBag.SearchName = searchName;

        var costumes = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(costumes);
    }

    // GET: Costumes/Details/5
    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var costume = await _context.Costumes
            .Include(c => c.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (costume == null)
        {
            return NotFound();
        }

        return View(costume);
    }

    // GET: Costumes/Create
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesDropDownList();
        return View();
    }

    // POST: Costumes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create([Bind("Name,CategoryId,Size,PricePerDay,IsAvailable,Notes")] Costume costume)
    {
        if (ModelState.IsValid)
        {
            _context.Add(costume);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await PopulateCategoriesDropDownList(costume.CategoryId);
        return View(costume);
    }

    // GET: Costumes/Edit/5
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var costume = await _context.Costumes.FindAsync(id);
        if (costume == null)
        {
            return NotFound();
        }
        await PopulateCategoriesDropDownList(costume.CategoryId);
        return View(costume);
    }

    // POST: Costumes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CategoryId,Size,PricePerDay,IsAvailable,Notes")] Costume costume)
    {
        if (id != costume.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(costume);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Costumes.Any(e => e.Id == costume.Id))
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
        await PopulateCategoriesDropDownList(costume.CategoryId);
        return View(costume);
    }

    // GET: Costumes/Delete/5
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var costume = await _context.Costumes
            .Include(c => c.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (costume == null)
        {
            return NotFound();
        }

        return View(costume);
    }

    // POST: Costumes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var costume = await _context.Costumes.FindAsync(id);
        if (costume != null)
        {
            _context.Costumes.Remove(costume);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesDropDownList(object selectedCategory = null)
    {
        var categoriesQuery = _context.Categories.OrderBy(c => c.Name);
        ViewBag.CategoryId = new SelectList(await categoriesQuery.ToListAsync(), "Id", "Name", selectedCategory);
    }
}

