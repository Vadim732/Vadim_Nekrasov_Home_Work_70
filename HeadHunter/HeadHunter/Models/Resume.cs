using System.ComponentModel.DataAnnotations;

namespace HeadHunter.Models;

public class Resume
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Введите название")]
    public string Title { get; set; }
    public DateTime LastUpdated { get; set; }
    [Required(ErrorMessage = "Введите ожидаемую зарплату")]
    public int ExpectedSalary { get; set; }
    [Required(ErrorMessage = "Введите ссылку на телеграм")]
    public string Telegram { get; set; }
    [Required(ErrorMessage = "Введите почту")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Введите номер телефона")]
    public string PhoneNumber { get; set; }
    public string? FacebookLink { get; set; }
    public string? LinkedInLink { get; set; }
    
    public bool IsPublished { get; set; }
    
    [Required(ErrorMessage = "Выберите категорию")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
    public ICollection<EducationOrCourse> EducationAndCourses { get; set; } = new List<EducationOrCourse>();
}