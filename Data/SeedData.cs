using CostumeRentalSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        // Roles
        string[] roles = { "Administrator", "Employee", "Client" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Admin user
        var adminEmail = "admin@costumes.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }

        // Employee user
        var employeeEmail = "employee@costumes.local";
        var employeeUser = await userManager.FindByEmailAsync(employeeEmail);
        if (employeeUser == null)
        {
            employeeUser = new ApplicationUser
            {
                UserName = employeeEmail,
                Email = employeeEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(employeeUser, "Employee123!");
            await userManager.AddToRoleAsync(employeeUser, "Employee");
        }

        // Client user
        var clientEmail = "client@costumes.local";
        var clientUser = await userManager.FindByEmailAsync(clientEmail);
        if (clientUser == null)
        {
            clientUser = new ApplicationUser
            {
                UserName = clientEmail,
                Email = clientEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(clientUser, "Client123!");
            await userManager.AddToRoleAsync(clientUser, "Client");
        }

        // Sample categories
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Карнавални" },
                new Category { Name = "Исторически" },
                new Category { Name = "Детски" }
            );
            await context.SaveChangesAsync();
        }

        // Sample costumes
        if (!context.Costumes.Any())
        {
            var carnival = await context.Categories.FirstAsync(c => c.Name == "Карнавални");
            var historic = await context.Categories.FirstAsync(c => c.Name == "Исторически");

            context.Costumes.AddRange(
                new Costume
                {
                    Name = "Пират",
                    CategoryId = carnival.Id,
                    Size = "M",
                    PricePerDay = 15.00m,
                    IsAvailable = true
                },
                new Costume
                {
                    Name = "Рицар",
                    CategoryId = historic.Id,
                    Size = "L",
                    PricePerDay = 20.00m,
                    IsAvailable = true
                }
            );
            await context.SaveChangesAsync();
        }

        // Sample clients
        if (!context.Clients.Any())
        {
            context.Clients.AddRange(
                new Client
                {
                    Name = "Иван Иванов",
                    Phone = "0888123456",
                    Email = "ivan@example.com"
                },
                new Client
                {
                    Name = "Мария Петрова",
                    Phone = "0888765432",
                    Email = "maria@example.com"
                }
            );
            await context.SaveChangesAsync();
        }

        // Sample rentals
        if (!context.Rentals.Any())
        {
            var client = await context.Clients.FirstAsync();
            var costume = await context.Costumes.FirstAsync();

            context.Rentals.Add(new Rental
            {
                ClientId = client.Id,
                CostumeId = costume.Id,
                RentDate = DateTime.Today.AddDays(-1),
                ReturnDate = DateTime.Today.AddDays(1),
                Status = RentalStatus.Активен
            });

            costume.IsAvailable = false;

            await context.SaveChangesAsync();
        }
    }
}

