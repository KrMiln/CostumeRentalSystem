using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името на клиента е задължително.")]
        [StringLength(200, ErrorMessage = "Името на клиента не може да надвишава 200 символа.")]
        [Display(Name = "Име на клиент")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Телефонният номер е задължителен.")]
        [Phone(ErrorMessage = "Невалиден телефонен номер.")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Имейл адресът е задължителен.")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
        [Display(Name = "Имейл")]
        public string Email { get; set; }

        [StringLength(500, ErrorMessage = "Бележките не могат да надвишават 500 символа.")]
        [Display(Name = "Бележки")]
        public string? Notes { get; set; }

        public ICollection<Rental>? Rentals { get; set; }
    }
}