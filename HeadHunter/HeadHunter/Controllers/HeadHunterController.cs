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
    public IActionResult Index()
    {
        return View();
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
}