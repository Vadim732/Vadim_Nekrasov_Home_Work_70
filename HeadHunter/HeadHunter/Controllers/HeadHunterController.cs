using HeadHunter.Models;
using HeadHunter.ViewModels;
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
    public async Task<IActionResult> Index(int pageNumber = 1)
    {
        List<Vacancy> vacancies = _context.Vacancies
            .Where(v => v.IsPublished == true)
            .OrderByDescending(v => v.UpdatedAt)
            .ToList();

        if (User.IsInRole("applicant"))
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.Resumes = _context.Resumes.Where(a => a.UserId == user.Id).ToList();
        }
        int pageSize = 20;
        int totalVacancies = vacancies.Count();
        int totalPages = (int)Math.Ceiling((double)totalVacancies / pageSize);
        var paginatedVacancies = vacancies.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        var pageViewModel = new PageViewModel(totalVacancies, pageNumber, pageSize);
        ViewBag.PageViewModel = pageViewModel;
        return View(paginatedVacancies);
    }
    [Authorize(Roles = "employer")]
    public async Task<IActionResult> IndexResumes()
    {
        List<Resume> resumes = _context.Resumes
            .Include(r => r.User)
            .Where(r => r.IsPublished == true)
            .OrderByDescending(r => r.LastUpdated)
            .ToList();
        if (User.IsInRole("employer"))
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.Vacancies = _context.Vacancies.Where(a => a.EmployerId == user.Id).ToList();
        }
        return View(resumes);
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
            if (vacancy.ExperienceRequiredTo < vacancy.ExperienceRequiredFrom)
            {
                ModelState.AddModelError(string.Empty, "Опыт работы (до) не может быть меньше опыта работы (от).");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(vacancy);
            }
            var creator = await _userManager.GetUserAsync(User);
            vacancy.UpdatedAt = DateTime.UtcNow;
            vacancy.EmployerId = creator.Id;
            _context.Vacancies.Add(vacancy);
            await _context.SaveChangesAsync();
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
        if (vacancy.ExperienceRequiredTo < vacancy.ExperienceRequiredFrom)
        {
            ModelState.AddModelError("ExperienceRequiredTo", "Опыт работы (до) не может быть меньше опыта работы (от).");
        }
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(vacancy);
        }
        existingVacancy.Title = vacancy.Title;
        existingVacancy.Description = vacancy.Description;
        existingVacancy.CategoryId = vacancy.CategoryId;
        existingVacancy.ExperienceRequiredFrom = vacancy.ExperienceRequiredFrom;
        existingVacancy.ExperienceRequiredTo = vacancy.ExperienceRequiredTo;
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

    public IActionResult DetailsResume(int id)
    {
        var resume = _context.Resumes
            .Include(c => c.Category)
            .FirstOrDefault(r => r.Id == id);
        if (resume == null)
        {
            return NotFound("Такое резюме не найдено");
        }
        return View(resume);
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

    [Authorize(Roles = "applicant")]
    public IActionResult PublicationResume(int id)
    {
        var resume = _context.Resumes.FirstOrDefault(r => r.Id == id);
        if (resume == null)
        {
            return NotFound("Такое резюме не найдено");
        }

        if (resume.IsPublished == true)
        {
            return NotFound("Это резюме уже опубликовано");
        }
        resume.IsPublished = true;
        _context.Update(resume);
        _context.SaveChanges();
        return RedirectToAction("Profile", "Account");
    }
    
    [Authorize(Roles = "employer")]
    public IActionResult UnpublishVacancy(int id)
    {
        var vacancy = _context.Vacancies.FirstOrDefault(v => v.Id == id);
        if (vacancy == null)
        {
            return NotFound("Такая вакансия не найдена");
        }
        if (vacancy.IsPublished == false)
        {
            return NotFound("Эта вакансия уже не опубликована");
        }
        vacancy.IsPublished = false;
        _context.Update(vacancy);
        _context.SaveChanges();
        return RedirectToAction("Profile", "Account");
    }

    [Authorize(Roles = "applicant")]
    public IActionResult UnpublishResume(int id)
    {
        var resume = _context.Resumes.FirstOrDefault(r => r.Id == id);
        if (resume == null)
        {
            return NotFound("Такое резюме не найдено");
        }
        if (resume.IsPublished == false)
        {
            return NotFound("Это резюме уже не опубликовано");
        }
        resume.IsPublished = false;
        _context.Update(resume);
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

    [Authorize(Roles = "applicant")]
    [HttpPost]
    public async Task<IActionResult> SendResponse(int vacancyId, int resumeId) 
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }
        var vacancy = await _context.Vacancies.FindAsync(vacancyId);
        var resume = await _context.Resumes.FindAsync(resumeId);

        if (vacancy == null || resume == null || resume.UserId != user.Id)
        {
            return NotFound("Вакансия или резюме не найдены, либо вы пытаетесь использовать чужое резюме.");
        }
        var existingResponse = await _context.Responses
            .FirstOrDefaultAsync(r => r.VacancyId == vacancyId && r.ResumeId == resumeId);
        if (existingResponse != null)
        {
            return RedirectToAction("Chat", new { vacancyid = vacancyId, resumeId = resume.Id });
        }
        var response = new Response
        {
            ResumeId = resumeId,
            VacancyId = vacancyId,
            SentAt = DateTime.UtcNow,
            ApplicantId = user.Id
        };
        _context.Responses.Add(response);
        await _context.SaveChangesAsync();

        return RedirectToAction("Chat", new { vacancyid = vacancyId, resumeId = resume.Id } );
    }
    
    [Authorize(Roles = "employer")]
    [HttpPost]
    public async Task<IActionResult> SendEmployerResponse(int resumeId, int vacancyId) 
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }
        var resume = await _context.Resumes.FindAsync(resumeId);
        var vacancy = await _context.Vacancies.FindAsync(vacancyId);
        if (resume == null || vacancy == null || vacancy.EmployerId != user.Id)
        {
            return NotFound("Резюме или вакансия не найдены, либо вы пытаетесь использовать чужую вакансию.");
        }
        var existingResponse = await _context.Responses
            .FirstOrDefaultAsync(r => r.VacancyId == vacancyId && r.ResumeId == resumeId);
        if (existingResponse != null)
        {
            return RedirectToAction("Chat", new { vacancyid = vacancyId, resumeId = resume.Id });
        }
        var response = new Response
        {
            ResumeId = resumeId,
            VacancyId = vacancyId,
            SentAt = DateTime.UtcNow,
            ApplicantId = resume.UserId
        };
        _context.Responses.Add(response);
        await _context.SaveChangesAsync();
        return RedirectToAction("Chat", new { vacancyid = vacancyId, resumeId = resume.Id } );
    }

    
    [Authorize(Roles = "applicant")]
    public async Task<IActionResult> MyFeedback()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }
        var responses = await _context.Responses
            .Include(r => r.Resume)
            .Include(r => r.Vacancy)
            .Where(r => r.ApplicantId == user.Id)
            .ToListAsync();
        return View(responses);
    }

    
    [Authorize(Roles = "employer")]
    public async Task<IActionResult> AllResponsesVacancies()
    {
        User user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var employerVacancies = await _context.Vacancies
                .Where(v => v.EmployerId == user.Id)
                .Include(v => v.Category)
                .Include(v => v.Employer)
                .ToListAsync();
            ViewBag.Vacancies = employerVacancies;
            
            var responses = await _context.Responses
                .Where(r => employerVacancies.Select(v => v.Id).Contains(r.VacancyId))
                .Include(r => r.Resume)
                .ThenInclude(res => res.User)
                .ToListAsync();

            ViewBag.Responses = responses;

            return View(employerVacancies);
        }
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "applicant")]
    public async Task<IActionResult> DeleteResume(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == id);
            if (resume != null)
            {
                _context.Resumes.Remove(resume);
                await _context.SaveChangesAsync();
    
                return RedirectToAction("Profile", "Account");
            }
    
            return NotFound("Ошибка: Резюме не найдено!");
        }
        
        return Unauthorized();
    }
    
    [Authorize(Roles = "employer")]
    public async Task<IActionResult> DeleteVacancy(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == id);
            if (vacancy != null)
            {
                _context.Vacancies.Remove(vacancy);
                await _context.SaveChangesAsync();
    
                return RedirectToAction("Profile", "Account");
            }
            
            return NotFound("Ошибка: Вакансия не найдена!");
        }
        
        return Unauthorized();
    }
    
        public IActionResult Chat(int vacancyId, int resumeId)
    {
        var messages = _context.Messages
            .Include(m => m.User)
            .Where(m => m.VacancyId == vacancyId && m.ResumeId == resumeId)
            .OrderBy(m => m.DateOfDispatch)
            .ToList();

        ViewBag.VacancyId = vacancyId;
        ViewBag.ResumeId = resumeId;

        return View(messages);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMessage(int vacancyId, int resumeId, string inscription)
    {
        if (string.IsNullOrWhiteSpace(inscription))
        {
            return BadRequest(new { error = "Сообщение не может быть пустым." });
        }

        var creator = await _userManager.GetUserAsync(User);
        if (creator == null)
        {
            return Unauthorized(new { error = "Не удалось определить пользователя." });
        }

        var message = new Message
        {
            Inscription = inscription,
            DateOfDispatch = DateTime.UtcNow,
            UserId = creator.Id,
            VacancyId = vacancyId,
            ResumeId = resumeId
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Json(new
        {
            avatar = creator.Avatar,
            dateOfDispatch = message.DateOfDispatch.ToString("dd.MM.yyyy HH:mm:ss"),
            userName = creator.UserName,
            userid = creator.Id,
            inscription = message.Inscription,
            messageId = message.Id
        });
    }
    
    [HttpGet]
    public IActionResult GetLatestMessages(int vacancyId, int resumeId, DateTime? lastMessageTime)
    {
        IQueryable<Message> query = _context.Messages
            .Include(m => m.User)
            .Where(m => m.VacancyId == vacancyId && m.ResumeId == resumeId)
            .OrderByDescending(m => m.DateOfDispatch);

        if (lastMessageTime.HasValue)
        {
            query = query.Where(m => m.DateOfDispatch > lastMessageTime.Value);
        }

        var messages = query
            .Select(m => new
            {
                m.Id,
                m.Inscription,
                m.DateOfDispatch,
                UserName = m.User.UserName,
                Avatar = m.User.Avatar,
                m.UserId
            })
            .OrderBy(m => m.DateOfDispatch)
            .ToList();

        return Json(new { messages, currentUser = User.Identity.Name });
    }
}