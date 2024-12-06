using System.ComponentModel.DataAnnotations;

namespace HeadHunter.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Введите название категории")]
    public string Name { get; set; }
}