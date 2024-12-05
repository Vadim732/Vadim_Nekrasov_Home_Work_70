using HeadHunter.Models;
using HeadHunter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Delivery.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly HeadHunterContext _context;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, HeadHunterContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }
    
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        User user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            return View(user);
        }
        return RedirectToAction("Login", "Account");
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User user = await _userManager.FindByEmailAsync(model.Identifier) ?? await _userManager.FindByNameAsync(model.Identifier);
        
            if (user != null)
            {
                SignInResult result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "HeadHunter");
                }
            }
            
            ModelState.AddModelError("", "Неверный логин или пароль!");
        }

        return View(model);
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var existingUserEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserEmail != null)
            {
                ViewBag.ErrorMessage = "Ошибка: Этот адрес электронной почты уже используется другим пользователем!";
                return View(model);
            }
            
            var existingUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserName != null)
            {
                ViewBag.ErrorMessage = "Ошибка: Этот логин уже используется другим пользователем!";
                return View(model);
            }
            
            var currentDate = DateTime.UtcNow;
            var userAge = currentDate.Year - model.DateOfBirth.Year;
            if (model.DateOfBirth > currentDate.AddYears(-userAge)) 
            {
                userAge--;
            }
            if (userAge < 18)
            {
                ViewBag.ErrorMessage = "Ошибка: Нельзя зарегистрироваться пользователям моложе 18 лет!";
                return View(model);
            }
            
            if (model.Role != "employer" && model.Role != "applicant")
            {
                ModelState.AddModelError("", "Ошибка: Роль недействительна!");
                return View(model);
            }

            User user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                Avatar = model.Avatar,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth.ToUniversalTime()
                
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "HeadHunter");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }
    
    [HttpGet]
    [Authorize (Roles = "admin")]
    public IActionResult Index()
    {
        List<User> users = _userManager.Users.ToList();
        return View(users);
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        User user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("admin"))
        {
            return RedirectToAction("Profile", "Account");
        }
        var model = new EditViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            Avatar = user.Avatar,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth
        };
        return View(model);
    }
    
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditViewModel model)
    {
        if (ModelState.IsValid)
        {
            User user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("admin"))
            {
                return RedirectToAction("Profile", "Account");
            }
            if (user != null)
            {
                var existingUserEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserEmail != null && existingUserEmail.Id != user.Id)
                {
                    ViewBag.ErrorMessage = "Ошибка: Этот адрес электронной почты уже используется другим пользователем!";
                    return View(model);
                }
            
                var existingUserName = await _userManager.FindByNameAsync(model.UserName);
                if (existingUserName != null && existingUserName.Id != user.Id)
                {
                    ViewBag.ErrorMessage = "Ошибка: Этот логин уже используется другим пользователем!";
                    return View(model);
                }
            
                var currentDate = DateTime.Now;
                var userAge = currentDate.Year - model.DateOfBirth.Year;
                if (model.DateOfBirth > currentDate.AddYears(-userAge)) 
                {
                    userAge--;
                }
                if (userAge < 18)
                {
                    ViewBag.ErrorMessage = "Ошибка: Нельзя зарегистрироваться пользователям моложе 18 лет!";
                    return View(model);
                }
                
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.Avatar = model.Avatar;
                user.PhoneNumber = model.PhoneNumber;
                user.DateOfBirth = DateTime.SpecifyKind(model.DateOfBirth, DateTimeKind.Utc);

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Profile", "Account");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }
    
        return View(model);
    }
    
    [HttpGet]
    [Authorize (Roles = "admin")]
    public IActionResult RegisterUser()
    {
        return View();
    }
    
    [HttpPost]
    [Authorize (Roles = "admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterUser(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUserEmail = await _userManager.FindByEmailAsync(model.Email);
        if (existingUserEmail != null)
        {
            ViewBag.ErrorMessage = "Ошибка: Этот адрес электронной почты уже используется другим пользователем!";
            return View(model);
        }
    
        var existingUserName = await _userManager.FindByNameAsync(model.UserName);
        if (existingUserName != null)
        {
            ViewBag.ErrorMessage = "Ошибка: Этот логин уже используется другим пользователем!";
            return View(model);
        }
        
        var currentDate = DateTime.UtcNow;
        var userAge = currentDate.Year - model.DateOfBirth.Year;
        if (model.DateOfBirth > currentDate.AddYears(-userAge)) 
        {
            userAge--;
        }
        if (userAge < 18)
        {
            ViewBag.ErrorMessage = "Ошибка: Нельзя зарегистрироваться пользователям моложе 18 лет!";
            return View(model);
        }
        
        var user = new User
        {
            UserName = model.UserName,
            Email = model.Email,
            Avatar = model.Avatar,
            PhoneNumber = model.PhoneNumber,
            DateOfBirth = model.DateOfBirth.ToUniversalTime()
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "user");
            return RedirectToAction("Index", "Account");
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }
    
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> BlockUser(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFound($"Пользователь с ID {userId} не найден.");
        }
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("admin"))
        {
            return RedirectToAction("Index", "Account");
        }
        user.LockoutEnd = DateTimeOffset.MaxValue;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Account");
        }
        return RedirectToAction("Index", "Account");
    }

    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UnblockUser(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFound($"Пользователь с ID {userId} не найден.");
        }
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("admin"))
        {
            return RedirectToAction("Index", "Account");
        }

        if (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.Now)
        {
            return RedirectToAction("Index", "Account");
        }
        user.LockoutEnd = null;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Account");
        }
        return RedirectToAction("Index", "Account");
    }
    
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> UserEdit(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var model = new UserEditViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Avatar = user.Avatar
        };

        return View(model);
    }
    
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> UserEdit(int userId, UserEditViewModel uevm)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            var existingEmailUser = await _userManager.FindByEmailAsync(uevm.Email);
            if (existingEmailUser != null && existingEmailUser.Id != user.Id)
            {
                ViewBag.ErrorMessage = "Этот адрес электронной почты уже используется другим пользователем!";
                return View(uevm);
            }

            var existingUserNameUser = await _userManager.FindByNameAsync(uevm.UserName);
            if (existingUserNameUser != null && existingUserNameUser.Id != user.Id)
            {
                ViewBag.ErrorMessage = "Этот логин уже используется другим пользователем!";
                return View(uevm);
            }

            user.UserName = uevm.UserName;
            user.Email = uevm.Email;
            user.Avatar = uevm.Avatar;
            user.PhoneNumber = uevm.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Account");
            }
        }

        return View(uevm);
    }
    
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}