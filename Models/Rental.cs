using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CostumeRentalSystem.Models
{
    public enum RentalStatus
    {
        Активен = 0,
        Върнат = 1,
        Загубен = 2,
        Повреден = 3
    }

    public class Rental : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Клиент")]
        [Required(ErrorMessage = "Клиентът е задължителен.")]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Display(Name = "Костюм")]
        [Required(ErrorMessage = "Костюмът е задължителен.")]
        public int CostumeId { get; set; }
        public Costume? Costume { get; set; }

        [Required(ErrorMessage = "Датата на наемане е задължителна.")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата на наемане")]
        public DateTime RentDate { get; set; }

        [Required(ErrorMessage = "Датата на връщане е задължителна.")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата на връщане")]
        public DateTime ReturnDate { get; set; }

        [Required(ErrorMessage = "Статусът е задължителен.")]
        [Display(Name = "Статус")]
        public RentalStatus Status { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ReturnDate <= RentDate)
            {
                yield return new ValidationResult(
                    "Датата на връщане трябва да е след датата на наемане.",
                    new[] { nameof(ReturnDate) });
            }
        }
    }
}