using CostumeRentalSystem.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Role UserRole { get; set; }

        public int? ClientId { get; set; }

        public Client? Client { get; set; }
    }
}