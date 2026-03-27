using CostumeRentalSystem.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Role UserRole { get; set; }
    }
}