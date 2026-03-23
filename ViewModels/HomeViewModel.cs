using CostumeRentalSystem.Data.Entities;

namespace CostumeRentalSystem.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Costume> FeaturedCostumes { get; set; } = [];
        public int TotalCostumes { get; set; }
        public int AvailableCount { get; set; }
        public int TotalClients { get; set; }
    }
}