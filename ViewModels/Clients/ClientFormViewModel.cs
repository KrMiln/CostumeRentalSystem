namespace CostumeRentalSystem.ViewModels.Clients;

using System.ComponentModel.DataAnnotations;

public class ClientFormViewModel
{
    public int Id { get; set; } 

    [Required(ErrorMessage = "Моля, въведете име.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Името трябва да е между 2 и 100 символа.")]
    [Display(Name = "Пълно име")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Моля, въведете телефонен номер.")]
    [RegularExpression(@"^(\+359|0)8[789]\d{7}$", ErrorMessage = "Невалиден телефонен номер!")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Моля, въведете имейл адрес.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Невалиден имейл адрес!")]
    [Display(Name = "Имейл")]
    public string? Email { get; set; }

    [StringLength(500, ErrorMessage = "Бележките не могат да надвишават 500 символа.")]
    [Display(Name = "Бележки")]
    public string? Notes { get; set; }
}