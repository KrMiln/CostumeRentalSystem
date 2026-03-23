using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Data.Entities
{
    public enum Role
    {
        [Display(Name = "Администратор")]
        Administrator,

        [Display(Name = "Служител")]
        Employee,

        [Display(Name = "Клиент")]
        Client
    }

    public class ApplicationUser : IdentityUser
    {
        public Role UserRole { get; set; }
    }
}