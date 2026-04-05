namespace CostumeRentalSystem.ViewModels
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string PageAction { get; set; } 
        public string PageController { get; set; } 

        public Dictionary<string, string> RouteValues { get; set; } = new();
    }
}
