using CostumeRentalSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Data.Entities
{
    public class Costume
    {
        public enum CostumeSize
        {
            [Display(Name = "XS (Много малък)")]
            XS,

            [Display(Name = "S (Малък)")]
            S,

            [Display(Name = "M (Среден)")]
            M,

            [Display(Name = "L (Голям)")]
            L,

            [Display(Name = "XL (Много голям)")]
            XL,

            [Display(Name = "Детски")]
            Kids
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public CostumeSize? Size { get; set; }

        public decimal PricePerDay { get; set; }

        public bool IsAvailable { get; set; }

        public string? ImagePath { get; set; }

        public string? Notes { get; set; }

        public ICollection<Rental>? Rentals { get; set; }
    }
}