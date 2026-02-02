using CostumeRentalSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CostumeRentalSystem.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // 1. Създаване на Роли
        string[] roles = { "Administrator", "Employee", "Client" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Създаване на Администратор
        var adminEmail = "admin@costumes.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Name = "Главен Администратор", // Добавено поле
                UserRole = Role.Administrator    // Добавено поле
            };

            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }

        // 3. Създаване на Служител
        var employeeEmail = "employee@costumes.local";
        var employeeUser = await userManager.FindByEmailAsync(employeeEmail);
        if (employeeUser == null)
        {
            employeeUser = new ApplicationUser
            {
                UserName = employeeEmail,
                Email = employeeEmail,
                EmailConfirmed = true,
                Name = "Иван Служител", // Добавено поле
                UserRole = Role.Employee    // Добавено поле
            };

            await userManager.CreateAsync(employeeUser, "Employee123!");
            await userManager.AddToRoleAsync(employeeUser, "Employee");
        }

        // 4. Категории
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Карнавални" },
                new Category { Name = "Исторически" },
                new Category { Name = "Детски" }
            );
            await context.SaveChangesAsync();
        }

        // 5. Костюми
        if (!context.Costumes.Any())
        {
            var carnival = await context.Categories.FirstAsync(c => c.Name == "Карнавални");
            var historic = await context.Categories.FirstAsync(c => c.Name == "Исторически");

            context.Costumes.AddRange(
                new Costume
                {
                    Name = "Пират",
                    CategoryId = carnival.Id,
                    Size = Costume.CostumeSize.M,
                    PricePerDay = 15.00m,
                    IsAvailable = true,
                    ImageUrl = "/images/default_pirate.jpg" // Добра практика е да имаш път
                },
                new Costume
                {
                    Name = "Рицар",
                    CategoryId = historic.Id,
                    Size = Costume.CostumeSize.L,
                    PricePerDay = 20.00m,
                    IsAvailable = true,
                    ImageUrl = "/images/default_knight.jpg"
                }
            );
            await context.SaveChangesAsync();
        }

        // 6. Клиенти (за таблицата Clients)
        if (!context.Clients.Any())
        {
            context.Clients.AddRange(
                new Client
                {
                    Name = "Иван Иванов",
                    PhoneNumber = "0888123456",
                    Email = "ivan@example.com",
                    Notes = "Редовен клиент"
                },
                new Client
                {
                    Name = "Мария Петрова",
                    PhoneNumber = "0888765432",
                    Email = "maria@example.com"
                }
            );
            await context.SaveChangesAsync();
        }

        // 7. Наеми
        if (!context.Rentals.Any())
        {
            var client = await context.Clients.FirstAsync();
            var costume = await context.Costumes.FirstAsync(c => c.IsAvailable);

            context.Rentals.Add(new Rental
            {
                ClientId = client.Id,
                CostumeId = costume.Id,
                RentDate = DateTime.Today.AddDays(-1),
                ReturnDate = DateTime.Today.AddDays(1),
                Status = RentalStatus.Active
            });

            // Автоматично маркиране на костюма като зает
            costume.IsAvailable = false;
            await context.SaveChangesAsync();
        }
    }
}