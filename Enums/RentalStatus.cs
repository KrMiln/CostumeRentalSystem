using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Enums
{
    public enum RentalStatus
    {
        [Display(Name = "Нает")]
        Active,

        [Display(Name = "Върнат")]
        Returned,

        [Display(Name = "Изгубен")]
        Lost,

        [Display(Name = "Повреден")]
        Damaged
    }

}
