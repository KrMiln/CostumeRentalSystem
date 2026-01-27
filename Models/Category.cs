using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името на категорията е задължително.")]
        [StringLength(100, ErrorMessage = "Името на категорията не може да надвишава 100 символа.")]
        [Display(Name = "Име на категория")]
        public string Name { get; set; }

        public ICollection<Costume>? Costumes { get; set; }
    }
}