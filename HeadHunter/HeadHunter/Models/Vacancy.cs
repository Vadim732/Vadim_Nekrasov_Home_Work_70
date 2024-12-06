namespace HeadHunter.Models;

public class Vacancy
{
    public int Id { get; set; }
    public string Title { get; set; }
    public decimal Salary { get; set; }
    public string Description { get; set; }
    public int ExperienceRequiredFrom { get; set; }
    public int ExperienceRequiredTo { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublished { get; set; }
    
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    
    public int EmployerId { get; set; }
    public User Employer { get; set; }
}