using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.ViewModels.Categories
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Моля, въведете име на категория.")]
        [StringLength(100, ErrorMessage = "Името на категорията не може да надвишава 100 символа.")]
        [Display(Name = "Име на категория")]
        public string Name { get; set; } = string.Empty;
    }
}