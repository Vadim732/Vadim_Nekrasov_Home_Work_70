using System.ComponentModel.DataAnnotations;

namespace HeadHunter.Models;

public class Message
{
    public int Id { get; set; }
    [Required] public string Inscription { get; set; }
    public DateTime DateOfDispatch { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    [Required] public int VacancyId { get; set; }
    public Vacancy Vacancy { get; set; }
    [Required] public int ResumeId { get; set; }
    public Resume Resume { get; set; }
}