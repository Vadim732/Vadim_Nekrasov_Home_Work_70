using HeadHunter.Models;

namespace HeadHunter.ViewModels;

public class IndexViewModel
{
    public List<Vacancy> Vacancies { get; set; } = new();
    public List<Resume> Resumes { get; set; } = new();
}