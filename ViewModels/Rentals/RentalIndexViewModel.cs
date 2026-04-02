using CostumeRentalSystem.Common.Enums;
using CostumeRentalSystem.Data.Entities;

namespace CostumeRentalSystem.ViewModels
{
    public class RentalIndexViewModel
    {
        public string? SearchString { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public RentalStatus? Status { get; set; }

        public IEnumerable<Rental> Rentals { get; set; } = [];
        public PaginationViewModel Pagination { get; set; } = null!;
    }
}