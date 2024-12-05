using System.ComponentModel.DataAnnotations;

namespace HeadHunter.ViewModels;

public class UserEditViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Укажите логин!")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Требуется адрес электронной почты!")]
    [EmailAddress(ErrorMessage = "Неверный адрес электронной почты!")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Неверная ссылка на аватар!")]
    [Url]
    public string Avatar { get; set; }
    
    [Required(ErrorMessage = "Укажите номер телефона!")]
    [RegularExpression(@"^(\+?\d{1,4}[\s-]?)?(\(?\d{2,5}\)?[\s-]?)?[\d\s-]{5,15}$", ErrorMessage = "Введите корректный номер телефона!")]
    public string PhoneNumber { get; set; }
}