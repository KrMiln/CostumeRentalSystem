using CostumeRentalSystem.Common.Enums;
using CostumeRentalSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Data.Entities
{
    public class Costume
    {
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