using HeadHunter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Controllers;

public class HeadHunterController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly HeadHunterContext _context;

    public HeadHunterController(UserManager<User> userManager, HeadHunterContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    public async Task<IActionResult> Index()
    {
        List<Vacancy> vacancies = _context.Vacancies.Where(v => v.IsPublished == true).ToList();
        if (User.IsInRole("applicant"))
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.Resumes = _context.Resumes.Where(a => a.UserId == user.Id).ToList();
        }
        return View(vacancies);
    }
    [Authorize(Roles = "admin")]
    public IActionResult CreateCategory()
    {
        return View();
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult CreateCategory(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View();
    }
    [Authorize(Roles = "admin")]
    public IActionResult IndexCategories()
    {
        return View(_context.Categories.ToList());
    }
    
    [Authorize(Roles = "employer")]
    public IActionResult CreateVacancy()
    {
        ViewBag.Categories = _context.Categories.ToList();
        return View();
    }
    [HttpPost]
    [Authorize(Roles = "employer")]
    public async Task<IActionResult> CreateVacancy(Vacancy vacancy)
    {
        if (ModelState.IsValid)
        {
            var creator = await _userManager.GetUserAsync(User);
            vacancy.UpdatedAt = DateTime.UtcNow;
            vacancy.EmployerId = creator.Id;
            _context.Vacancies.Add(vacancy);
            _context.SaveChanges();
            return RedirectToAction("Profile", "Account");
        }
        ViewBag.Categories = _context.Categories.ToList();
        return View(vacancy);
    }
    
    [Authorize(Roles = "applicant")]
    public IActionResult CreateResume()
    {
        ViewBag.Categories = _context.Categories.ToList();
        return View();
    }
    [HttpPost]
    [Authorize(Roles = "applicant")]
    public async Task<IActionResult> CreateResume(Resume resume)
    {
        if (ModelState.IsValid)
        {
            var creator = await _userManager.GetUserAsync(User);
            resume.LastUpdated = DateTime.UtcNow;
            resume.UserId = creator.Id;
            _context.Resumes.Add(resume);
            _context.SaveChanges();
            return RedirectToAction("Profile", "Account");
        }
        ViewBag.Categories = _context.Categories.ToList();
        return View(resume);
    }
    
    [Authorize(Roles = "employer")]
    public async Task<IActionResult> EditVacancy(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }
        var vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == id);
        if (vacancy == null)
        {
            return NotFound("Такая вакансия не найдена");
        }
        if (vacancy.EmployerId != user.Id)
        {
            return Unauthorized();
        }
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(vacancy);
    }

    [HttpPost]
    [Authorize(Roles = "employer")]
    public async Task<IActionResult> EditVacancy(Vacancy vacancy)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }
        var existingVacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == vacancy.Id);
        if (existingVacancy == null)
        {
            return NotFound("Такая вакансия не найдена");
        }
        if (existingVacancy.EmployerId != user.Id)
        {
            return Unauthorized();
        }
        existingVacancy.Title = vacancy.Title;
        existingVacancy.Description = vacancy.Description;
        existingVacancy.CategoryId = vacancy.CategoryId;
        existingVacancy.UpdatedAt = DateTime.UtcNow;
        _context.Vacancies.Update(existingVacancy);
        await _context.SaveChangesAsync();

        return RedirectToAction("Profile", "Account");
    }
    [Authorize(Roles = "applicant")]
    public async Task<IActionResult> EditResume(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }
        var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == id);
        if (resume == null)
        {
            return NotFound("Такое резюме не найдено");
        }
        if (resume.UserId != user.Id)
        {
            return Unauthorized();
        }
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(resume);
    }

    [Authorize(Roles = "applicant")]
    [HttpPost]
    public async Task<IActionResult> EditResume(Resume resume)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }
        var existingResume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == resume.Id);
        if (existingResume == null)
        {
            return NotFound("Такое резюме не найдено");
        }
        if (existingResume.UserId != user.Id)
        {
            return Unauthorized();
        }
        existingResume.Title = resume.Title;
        existingResume.Email = resume.Email;
        existingResume.PhoneNumber = resume.PhoneNumber;
        existingResume.CategoryId = resume.CategoryId;
        existingResume.Telegram = resume.Telegram;
        existingResume.ExpectedSalary = resume.ExpectedSalary;
        existingResume.LastUpdated = DateTime.UtcNow;
        existingResume.LinkedInLink = resume.LinkedInLink;
        existingResume.FacebookLink = resume.FacebookLink;
        _context.Resumes.Update(existingResume);
        await _context.SaveChangesAsync();
        return RedirectToAction("Profile", "Account");
    }

    public IActionResult DetailsVacancy(int id)
    {
        var vacancy = _context.Vacancies
            .Include(c => c.Category)
            .FirstOrDefault(v => v.Id == id);
        if (vacancy == null)
        {
            return NotFound("Такая вакансия не найдена");
        }
        return View(vacancy);
    }
    [Authorize(Roles = "employer")]
    public IActionResult PublicationVacancy(int id)
    {
        var vacancy = _context.Vacancies.FirstOrDefault(v => v.Id == id);
        if (vacancy == null)
        {
            return NotFound("Такая вакансия не найдена");
        }
        if (vacancy.IsPublished == true)
        {
            return NotFound("Эта вакансия уже опубликована");
        }
        vacancy.IsPublished = true;
        _context.Update(vacancy);
        _context.SaveChanges();
        return RedirectToAction("Profile", "Account");  
    }
    
    [Authorize(Roles = "employer")]
    public IActionResult UpdateVacancy(int id)
    {
        var vacancy = _context.Vacancies.FirstOrDefault(v => v.Id == id);
        if (vacancy == null)
        {
            return NotFound("Такая вакансия не найдена");
        }
        vacancy.UpdatedAt = DateTime.UtcNow;
        _context.Update(vacancy);
        _context.SaveChanges();
        return RedirectToAction("Profile", "Account");
    }
    
    [Authorize(Roles = "applicant")]
    public IActionResult UpdateResume(int id)
    {
        var resume = _context.Resumes.FirstOrDefault(r => r.Id == id);
        if (resume == null)
        {
            return NotFound("Такое резюме не найдено");
        }
        resume.LastUpdated = DateTime.UtcNow;
        _context.Update(resume);
        _context.SaveChanges();
        return RedirectToAction("Profile", "Account");
    }

    [Authorize (Roles = "applicant")]
    public async Task<IActionResult> SendResponse()
    {
        return RedirectToAction("Index");
    }
    
    [Authorize(Roles = "applicant")]
    public async Task<IActionResult> MyFeedback() // Метод для показа всех откликов соискателя вместе с отправленными резюме
    {
        return RedirectToAction("Profile", "Account");
    }
    
    [Authorize(Roles = "employer")]
    public async Task<IActionResult> AllResponsesVacancies()
    {
        User user = await _userManager.GetUserAsync(User);
        if (User != null)
        {
            var employerVacancies = await _context.Vacancies
                .Where(v => v.EmployerId == user.Id)
                .Include(v => v.Category)
                .Include(v => v.Employer)
                .ToListAsync();
            ViewBag.Vacancies = employerVacancies;
            
            var responses = await _context.Resumes
                .Where(r => employerVacancies.Select(v => v.Id).Contains(r.CategoryId))
                .Include(r => r.User)
                .ToListAsync();

            ViewBag.Vacancies = employerVacancies;
            ViewBag.Responses = responses;

            return View(employerVacancies);
        }
        return RedirectToAction("Index");
    }
}