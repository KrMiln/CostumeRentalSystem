namespace CostumeRentalSystem.ViewModels
{
    public class UserViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; }

        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();
    }
}
