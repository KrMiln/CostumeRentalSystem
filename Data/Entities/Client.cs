using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Data.Entities
{
    public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string? Notes { get; set; }

        public ICollection<Rental>? Rentals { get; set; }
    }
}