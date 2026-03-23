using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CostumeRentalSystem.Data.Entities
{
    public enum RentalStatus
    {
        [Display(Name = "Нает")]
        Active,

        [Display(Name = "Върнат")]
        Returned,

        [Display(Name = "Изгубен")]
        Lost,

        [Display(Name = "Повреден")]
        Damaged
    }

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