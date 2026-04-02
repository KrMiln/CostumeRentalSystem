namespace CostumeRentalSystem.ViewModels
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string PageAction { get; set; } // e.g., "Index"
        public string PageController { get; set; } // e.g., "Costumes"

        // For keeping search filters while paging
        public Dictionary<string, string> RouteValues { get; set; } = new();
    }
}
