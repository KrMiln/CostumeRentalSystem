using Microsoft.AspNetCore.Mvc.Rendering;
using CostumeRentalSystem.Data.Entities;

namespace CostumeRentalSystem.ViewModels.Rentals
{
    public class CostumeIndexViewModel
    {
        // Филтри
        public string? SearchName { get; set; }
        public int? CategoryId { get; set; }
        public bool OnlyAvailable { get; set; }
        public string? SelectedSize { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Данни за изгледа
        public IEnumerable<Costume> Costumes { get; set; } = [];
        public SelectList Categories { get; set; } = null!;
        public SelectList SizeList { get; set; } = null!;
        public PaginationViewModel Pagination { get; set; } = null!;
    }
}