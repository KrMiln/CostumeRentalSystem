using CostumeRentalSystem.Common.Enums;
using CostumeRentalSystem.Data.Entities;
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

        foreach (var roleName in Enum.GetNames(typeof(Role)))
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

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
            };

            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }

        var e1Email = "employee1@costumes.local";
        var e1Username = "employee1";
        var e1User = await userManager.FindByEmailAsync(e1Email);
        if (e1User == null)
        {
            e1User = new ApplicationUser
            {
                UserName = e1Username,
                Email = e1Email,
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(e1User, "Employee123!");
            await userManager.AddToRoleAsync(e1User, "Employee");
        }

        var e2Email = "employee2@costumes.local";
        var e2Username = "employee2";
        var e2User = await userManager.FindByEmailAsync(e2Email);
        if (e2User == null)
        {
            e2User = new ApplicationUser
            {
                UserName = e2Username,
                Email = e2Email,
                EmailConfirmed = true,  
            };

            await userManager.CreateAsync(e2User, "Employee123!");
            await userManager.AddToRoleAsync(e2User, "Employee");
        }

        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Карнавални" },
                new Category { Name = "Исторически" },
                new Category { Name = "Детски" }
            );
            await context.SaveChangesAsync();
        }

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