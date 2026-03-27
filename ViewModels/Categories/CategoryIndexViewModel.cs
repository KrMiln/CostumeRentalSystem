using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.ViewModels.Categories
{
    public class CategoryIndexViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Име на категорията")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Брой костюми")]
        public int CostumesCount { get; set; }
    }
}