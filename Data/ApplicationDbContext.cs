using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CostumeRentalSystem.Models;
using CostumeRentalSystem.Data.Entities;

namespace CostumeRentalSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Costume> Costumes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            builder.Entity<Client>()
                .HasIndex(c => c.Email)
                .IsUnique();

            builder.Entity<Client>()
                .HasIndex(c => c.PhoneNumber)
                .IsUnique();

            builder.Entity<Client>()
                .HasOne(c => c.User)
                .WithOne(u => u.Client)
                .HasForeignKey<Client>(c => c.UserId)
                .IsRequired(false);
        }
    }
}