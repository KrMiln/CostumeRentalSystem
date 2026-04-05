using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.ViewModels.Categories
{
    public class CategoryIndexViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}