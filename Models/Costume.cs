using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Models
{
    public class Costume
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името на костюма е задължително.")]
        [StringLength(200, ErrorMessage = "Името на костюма не може да надвишава 200 символа.")]
        [Display(Name = "Име на костюм")]
        public string Name { get; set; }

        [Display(Name = "Категория")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required(ErrorMessage = "Размерът е задължителен.")]
        [StringLength(50, ErrorMessage = "Размерът не може да надвишава 50 символа.")]
        [Display(Name = "Размер")]
        public string Size { get; set; }

        [Required(ErrorMessage = "Цената на ден е задължителна.")]
        [Range(0.01, 1000.00, ErrorMessage = "Цената на ден трябва да е между 0.01 и 1000.00.")]
        [Display(Name = "Цена на ден")]
        public decimal PricePerDay { get; set; }

        [Display(Name = "Наличност")]
        public bool IsAvailable { get; set; }

        [Url(ErrorMessage = "Моля, въведете валиден URL адрес на изображение.")]
        [StringLength(500, ErrorMessage = "URL адресът не може да надвишава 500 символа.")]
        [Display(Name = "URL на изображение")]
        public string? ImageUrl { get; set; }

        [StringLength(500, ErrorMessage = "Бележките не могат да надвишават 500 символа.")]
        [Display(Name = "Бележки")]
        public string? Notes { get; set; }

        public ICollection<Rental>? Rentals { get; set; }
    }
}