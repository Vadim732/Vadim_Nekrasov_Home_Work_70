namespace HeadHunter.Models;

public class EducationOrCourse
{
    public int Id { get; set; }
    public string InstitutionName { get; set; }
    public string DegreeOrCertification { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int ResumeId { get; set; }
    public Resume? Resume { get; set; }
}