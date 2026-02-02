using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Models
{
    public enum Role
    {
        Administrator,
        Employee,
        Client
    }

    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Името е задължително.")]
        [StringLength(200, ErrorMessage = "Името не може да надвишава 200 символа.")]
        [Display(Name = "Име")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Ролята е задължителна.")]
        [Display(Name = "Роля")]
        public Role UserRole { get; set; }
    }
}