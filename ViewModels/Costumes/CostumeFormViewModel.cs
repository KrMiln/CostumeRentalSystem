using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using CostumeRentalSystem.Data.Entities;
using CostumeRentalSystem.Enums;

namespace CostumeRentalSystem.ViewModels
{
    public class CostumeFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Моля, въведете име.")]
        [StringLength(100, ErrorMessage = "Името на костюма не може да надвишава 100 символа.")]
        [Display(Name = "Име")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Моля, изберете категория.")]
        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Моля, изберете размер.")]
        [Display(Name = "Размер")]
        public CostumeSize? Size { get; set; }

        [Required(ErrorMessage = "Моля, въведете цена.")]
        [Range(10, 100, ErrorMessage = "Моля, въведете цена в интервала 10 и 100 лв.")]
        [Display(Name = "Цена на ден")]
        public decimal PricePerDay { get; set; }

        public bool IsAvailable { get; set; } = true;

        [StringLength(500, ErrorMessage = "Бележките не могат да надвишават 500 символа.")]
        [Display(Name = "Бележки")]
        public string? Notes { get; set; }

        public string? ExistingImagePath { get; set; }

        [Display(Name = "Качи снимка:")]
        public IFormFile? ImageFile { get; set; }

        public SelectList? Categories { get; set; }
    }
}