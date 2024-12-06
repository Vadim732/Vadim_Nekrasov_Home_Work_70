using System.ComponentModel.DataAnnotations;

namespace HeadHunter.Models;

public class Vacancy
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Введите название")]
    public string Title { get; set; }
    
    [Required(ErrorMessage = "Введите заработную плату")]
    public decimal Salary { get; set; }
    
    [Required(ErrorMessage = "Введите описание")]
    public string Description { get; set; }
    
    [Required(ErrorMessage = "Введите опыт работы(от)")]
    public int ExperienceRequiredFrom { get; set; }
    
    [Required(ErrorMessage = "Введите опыт работы(до)")]
    public int ExperienceRequiredTo { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublished { get; set; }
    
    [Required(ErrorMessage = "Выберите категорию")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public int EmployerId { get; set; }
    public User? Employer { get; set; }
}