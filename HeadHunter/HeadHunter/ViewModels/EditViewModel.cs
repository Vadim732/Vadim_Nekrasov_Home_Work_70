﻿using System.ComponentModel.DataAnnotations;

namespace HeadHunter.ViewModels;

public class EditViewModel
{
    [Required(ErrorMessage = "Укажите логин!")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Требуется адрес электронной почты!")]
    [EmailAddress(ErrorMessage = "Неверный адрес электронной почты!")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Обязательно укажите дату вашего рождения!")]
    public DateTime DateOfBirth { get; set; }
    
    [Required(ErrorMessage = "Укажите номер телефона!")]
    [RegularExpression(@"^(\+?\d{1,4}[\s-]?)?(\(?\d{2,5}\)?[\s-]?)?[\d\s-]{5,15}$", ErrorMessage = "Введите корректный номер телефона!")]
    public string PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "Неверная ссылка на аватар!")]
    [Url]
    public string Avatar { get; set; }

    [Required(ErrorMessage = "Требуется пароль!")]
    [MinLength(6, ErrorMessage = "Пароль должен содержать не менее 6 символов!")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$", ErrorMessage = "Пароль должен содержать минимум 1 букву верхнего регистра, 1 букву нижнего регистра и 1 цифру!")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}