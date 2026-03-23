using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace CostumeRentalSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Costume> Costumes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Уникален Имейл и Телефон за Клиент
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email).IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.PhoneNumber).IsUnique();

            // 2. Уникално име на Категория
            modelBuilder.Entity<Category>()
                .HasIndex(cat => cat.Name).IsUnique();
        }
    }
}