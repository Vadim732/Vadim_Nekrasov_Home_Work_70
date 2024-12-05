using Microsoft.AspNetCore.Mvc;

namespace Delivery.Controllers;

public class HeadHunterController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}