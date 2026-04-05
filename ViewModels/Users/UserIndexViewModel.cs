using CostumeRentalSystem.ViewModels.Users;

namespace CostumeRentalSystem.ViewModels.Users
{
    public class UserIndexViewModel
    {
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public int TotalUsersCount { get; set; }

        public IEnumerable<UserDetailsViewModel> Users { get; set; } = [];
        public PaginationViewModel Pagination { get; set; } = null!;
    }
}