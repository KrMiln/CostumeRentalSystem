using CostumeRentalSystem.Common;
using CostumeRentalSystem.Common.Enums;
using CostumeRentalSystem.Data;
using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Services.IServices;
using CostumeRentalSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CostumeRentalSystem.Services
{
    public class RentalService : IRentalService
    {
        private readonly ApplicationDbContext _context;

        public RentalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Rental>> GetFilteredRentalsAsync(
            string? searchString, DateTime? startDate, DateTime? endDate, RentalStatus? status, int page, int pageSize)
        {
            var query = _context.Rentals
                .Include(r => r.Client)
                .Include(r => r.Costume)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(r => r.Client.Name.Contains(searchString) || r.Costume.Name.Contains(searchString));
            }

            if (startDate.HasValue) query = query.Where(r => r.RentDate == startDate.Value);
            if (endDate.HasValue) query = query.Where(r => r.ReturnDate == endDate.Value);
            if (status.HasValue) query = query.Where(r => r.Status == status.Value);

            return await query
                .OrderByDescending(r => r.RentDate)
                .ToPagedResultAsync(page, pageSize);
        }

        public async Task<PagedResult<Rental>> GetFilteredRentalsByUserIdAsync(string userId, int page, int pageSize)
        {
            return await _context.Rentals
                .Include(r => r.Costume)
                .Where(r => r.Client.UserId == userId)
                .AsNoTracking()
                .OrderBy(r => r.RentDate)
                .ToPagedResultAsync(page, pageSize);
        }

        public async Task<Rental?> GetRentalByIdAsync(int id)
        {
            return await _context.Rentals
                .Include(r => r.Client)
                .Include(r => r.Costume)
                .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string ErrorMessage)> CreateRentalAsync(Rental rental)
        {
            var costume = await _context.Costumes.FindAsync(rental.CostumeId);

            if (costume == null || !costume.IsAvailable)
                return (false, "Избраният костюм в момента не е наличен.");

            costume.IsAvailable = false;
            rental.Status = RentalStatus.Active;

            _context.Add(rental);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateRentalAsync(Rental rental)
        {
            var oldRental = await _context.Rentals.AsNoTracking().FirstOrDefaultAsync(r => r.Id == rental.Id);
            if (oldRental == null) return (false, "Записът не е намерен.");

            var currentCostume = await _context.Costumes.FindAsync(rental.CostumeId);
            var busyStatuses = new[] { RentalStatus.Active, RentalStatus.Lost, RentalStatus.Damaged };

            if (busyStatuses.Contains(rental.Status) && oldRental.Status == RentalStatus.Returned)
            {
                if (currentCostume != null && !currentCostume.IsAvailable)
                    return (false, "Костюмът вече е зает от друг клиент!");
            }

            if (currentCostume != null)
            {
                currentCostume.IsAvailable = (rental.Status == RentalStatus.Returned);
            }

            _context.Update(rental);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteRentalAsync(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return (false, "Наемът не съществува.");

            if (rental.Status == RentalStatus.Active || rental.Status == RentalStatus.Damaged)
                return (false, "Не може да изтриете активен или повреден наем.");

            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<IEnumerable<Client>> GetClientsAsync()
        {
            return await _context.Clients
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Costume>> GetAvailableCostumesAsync(int? currentCostumeId = null)
        {
            return await _context.Costumes
                .Where(c => c.IsAvailable || (currentCostumeId != null && c.Id == currentCostumeId))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}