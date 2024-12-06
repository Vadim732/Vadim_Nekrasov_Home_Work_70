using HeadHunter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

    public IActionResult IndexCategories()
    {
        return View(_context.Categories.ToList());
    }
}