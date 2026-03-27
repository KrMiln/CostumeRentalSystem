using CostumeRentalSystem.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CostumeRentalSystem.Data.Entities
{
    public class Rental
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public Client? Client { get; set; }

        public int CostumeId { get; set; }

        public Costume? Costume { get; set; }

        public DateTime RentDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public RentalStatus Status { get; set; }
    }
}