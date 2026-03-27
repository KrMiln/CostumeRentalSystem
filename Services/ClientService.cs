using CostumeRentalSystem.Data;
using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Enums;
using CostumeRentalSystem.Services.Interfaces;
using CostumeRentalSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CostumeRentalSystem.Services
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;

        public ClientService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Client>> GetFilteredClientsAsync(
            string? searchName, string? searchPhone, string? searchEmail, int page, int pageSize)
        {
            var query = _context.Clients.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(c => c.Name.Contains(searchName));

            if (!string.IsNullOrWhiteSpace(searchPhone))
                query = query.Where(c => c.PhoneNumber.Contains(searchPhone));

            if (!string.IsNullOrWhiteSpace(searchEmail))
                query = query.Where(c => c.Email.Contains(searchEmail));

            return await query
                .OrderBy(c => c.Name)
                .ToPagedResultAsync(page, pageSize);
        }

        public async Task<Client?> GetByIdAsync(int id, bool includeRentals = false)
        {
            var query = _context.Clients.AsQueryable();
            if (includeRentals)
            {
                query = query.Include(c => c.Rentals).ThenInclude(r => r.Costume);
            }
            return await query.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<bool> CreateAsync(Client client)
        {
            _context.Add(client);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Client client)
        {
            _context.Update(client);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Rentals)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null) return (false, "Клиентът не е намерен.");

            // Бизнес логика: Проверка за активни наеми
            bool hasActiveRentals = client.Rentals.Any(r =>
                r.Status != RentalStatus.Returned && r.Status != RentalStatus.Lost);

            if (hasActiveRentals)
                return (false, "Клиентът не може да бъде изтрит, докато не върне всички наети костюми!");

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Clients.AnyAsync(e => e.Id == id);
        }
    }
}