using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HeadHunter.Models;

public class HeadHunterContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Resume> Resumes { get; set; }
    public DbSet<Vacancy> Vacancies { get; set; }
    public DbSet<WorkExperience> WorkExperiences { get; set; }
    public DbSet<EducationOrCourse> EducationOrCourses { get; set; }
    
    public HeadHunterContext(DbContextOptions<HeadHunterContext> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}