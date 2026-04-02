using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Common.Enums
{
    public enum Role
    {
        [Display(Name = "Администратор")]
        Administrator,

        [Display(Name = "Служител")]
        Employee,

        [Display(Name = "Клиент")]
        Client
    }

}
