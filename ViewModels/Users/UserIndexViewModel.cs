using CostumeRentalSystem.ViewModels; // Където се намира PaginationViewModel
using CostumeRentalSystem.ViewModels.NewFolder;

namespace CostumeRentalSystem.ViewModels.Users
{
    public class UserIndexViewModel
    {
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }

        // Данните за таблицата
        public IEnumerable<UserDetailsViewModel> Users { get; set; } = [];

        // Броячът, който искаше за всички акаунти
        public int TotalUsersCount { get; set; }

        // Новият стандартен обект за пагинация
        public PaginationViewModel Pagination { get; set; } = null!;
    }
}