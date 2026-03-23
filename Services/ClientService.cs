using CostumeRentalSystem.Data;
using CostumeRentalSystem.Data.Entities;
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

        public async Task<(bool Success, string ErrorMessage)> CreateAsync(Client client)
        {
            // 1. Проверка за дублиран имейл
            bool emailExists = await _context.Clients.AnyAsync(c => c.Email == client.Email);
            if (emailExists)
            {
                return (false, "Този имейл вече е регистриран.");
            }

            // 2. Проверка за дублиран телефон
            bool phoneExists = await _context.Clients.AnyAsync(c => c.PhoneNumber == client.PhoneNumber);
            if (phoneExists)
            {
                return (false, "Този телефонен номер вече съществува в системата.");
            }

            _context.Add(client);
            bool saved = await _context.SaveChangesAsync() > 0;

            return (saved, saved ? string.Empty : "Възникна грешка при записването в базата данни.");
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Client client)
        {
            // 1. Проверка дали ДРУГ клиент вече ползва този имейл
            bool emailExists = await _context.Clients
                .AnyAsync(c => c.Email == client.Email && c.Id != client.Id);

            if (emailExists)
            {
                return (false, "Друг клиент вече използва този имейл.");
            }

            // 2. Проверка дали ДРУГ клиент вече ползва този телефон
            bool phoneExists = await _context.Clients
                .AnyAsync(c => c.PhoneNumber == client.PhoneNumber && c.Id != client.Id);

            if (phoneExists)
            {
                return (false, "Друг клиент вече използва този телефонен номер.");
            }

            _context.Update(client);
            bool updated = await _context.SaveChangesAsync() > 0;

            return (updated, updated ? string.Empty : "Не бяха открити промени за запис или възникна грешка.");
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