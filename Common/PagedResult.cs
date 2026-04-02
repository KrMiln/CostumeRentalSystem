using CostumeRentalSystem.ViewModels;

namespace CostumeRentalSystem.Common
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        public PaginationViewModel ToPaginationConfig(string controller, string action, Dictionary<string, string?> routeValues)
        {
            return new PaginationViewModel
            {
                CurrentPage = this.CurrentPage,
                TotalPages = this.TotalPages,
                PageController = controller,
                PageAction = action,
                RouteValues = routeValues
            };
        }
    }
}
