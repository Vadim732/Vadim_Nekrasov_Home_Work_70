using System.ComponentModel.DataAnnotations;

namespace HeadHunter.Models;

public class Response
{
    public int Id { get; set; }

    [Required]
    public int ResumeId { get; set; }
    public Resume? Resume { get; set; }

    [Required]
    public int VacancyId { get; set; }
    public Vacancy Vacancy { get; set; }

    public DateTime SentAt { get; set; } = DateTime.Now;

    [Required]
    public int ApplicantId { get; set; }
    public User Applicant { get; set; }
}