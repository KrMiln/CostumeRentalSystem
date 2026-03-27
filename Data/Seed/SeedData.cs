using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Enums;
using CostumeRentalSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CostumeRentalSystem.Data.Seed;

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
        foreach (var roleName in Enum.GetNames(typeof(Role)))
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Създаване на администратор
        var adminEmail = "admin@costumes.local";
        var adminUsername = "admin1234";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true,
                UserRole = Role.Administrator  
            };

            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }

        // 3. Създаване на двама служители
        var employeeEmail = "employee1@costumes.local";
        var employeeUsername = "employee1";
        var employeeUser = await userManager.FindByEmailAsync(employeeEmail);
        if (employeeUser == null)
        {
            employeeUser = new ApplicationUser
            {
                UserName = employeeUsername,
                Email = employeeEmail,
                EmailConfirmed = true,
                UserRole = Role.Employee  
            };

            await userManager.CreateAsync(employeeUser, "Employee123!");
            await userManager.AddToRoleAsync(employeeUser, "Employee");
        }

        var e1Email = "employee2@costumes.local";
        var e1Username = "employee2";
        var e1User = await userManager.FindByEmailAsync(e1Email);
        if (e1User == null)
        {
            e1User = new ApplicationUser
            {
                UserName = e1Username,
                Email = e1Email,
                EmailConfirmed = true,
                UserRole = Role.Employee   
            };

            await userManager.CreateAsync(e1User, "Employee123!");
            await userManager.AddToRoleAsync(e1User, "Employee");
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
                    Size = CostumeSize.M,
                    PricePerDay = 15.00m,
                    IsAvailable = true,
                    ImagePath = "images/default_pirate.jpg" 
                },
                new Costume
                {
                    Name = "Рицар",
                    CategoryId = historic.Id,
                    Size = CostumeSize.L,
                    PricePerDay = 20.00m,
                    IsAvailable = true,
                    ImagePath = "images/default_knight.jpg"
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

            costume.IsAvailable = false;
            await context.SaveChangesAsync();
        }
    }
}