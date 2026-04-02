using System.ComponentModel.DataAnnotations;

namespace CostumeRentalSystem.Common.Enums
{
    public enum CostumeSize
    {
        [Display(Name = "XS (Много малък)")]
        XS,

        [Display(Name = "S (Малък)")]
        S,

        [Display(Name = "M (Среден)")]
        M,

        [Display(Name = "L (Голям)")]
        L,

        [Display(Name = "XL (Много голям)")]
        XL,

        [Display(Name = "Детски")]
        Kids
    }
}
