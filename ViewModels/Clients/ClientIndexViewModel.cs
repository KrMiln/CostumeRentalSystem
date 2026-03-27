using CostumeRentalSystem.Data.Entities;

namespace CostumeRentalSystem.ViewModels
{
    public class ClientIndexViewModel
    {
        public string? SearchName { get; set; }
        public string? SearchPhone { get; set; }
        public string? SearchEmail { get; set; }

        public IEnumerable<Client> Clients { get; set; } = [];
        public PaginationViewModel Pagination { get; set; } = null!;
    }
}