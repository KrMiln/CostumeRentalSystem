using Microsoft.AspNetCore.Identity;

namespace CostumeRentalSystem.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public int? ClientId { get; set; }
        public virtual Client? Client { get; set; }
    }
}