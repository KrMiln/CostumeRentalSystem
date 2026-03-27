using System.ComponentModel.DataAnnotations;
using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Enums;

namespace CostumeRentalSystem.ViewModels
{
    public class RentalFormViewModel : IValidatableObject
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Моля, изберете клиент.")]
        public int ClientId { get; set; }
        public string? ClientName { get; set; }

        [Required(ErrorMessage = "Моля, изберете костюм.")]
        public int CostumeId { get; set; }

        public string? CostumeName { get; set; }
        public string? CostumeImagePath { get; set; }
        public decimal PricePerDay { get; set; }

        [Required(ErrorMessage = "Моля, изберете начална дата.")]
        [Display(Name = "Дата на наемане")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Моля, изберете дата на връщане.")]
        [Display(Name = "Дата на връщане")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Моля, задайте статус.")]
        public RentalStatus Status { get; set; } = RentalStatus.Active;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "Датата на връщане трябва да е след датата на наемане.",
                    new[] { nameof(EndDate) }
                );
            }
        }
    }
}