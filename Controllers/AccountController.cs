using Microsoft.AspNetCore.Mvc;
using MvcAuthApp.Data;
using MvcAuthApp.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    public IActionResult Register(User user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        if (_context.Users.Any(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Пользователь с таким email уже существует.");
            return View(user);
        }

        user.RegistrationDate = DateTime.Now;
        if (user.Id == Guid.Empty)
            user.Id = Guid.NewGuid();

        _context.Users.Add(user);
        _context.SaveChanges();

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("Username", user.Username);

        TempData["SuccessMessage"] = "Регистрация прошла успешно!";
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Auth
    [HttpGet]
    public IActionResult Auth()
    {
        return View("Login");
    }

    // POST: /Account/Auth
    [HttpPost]
    public IActionResult Auth(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
        if (user == null)
        {
            ModelState.AddModelError("", "Неверный Email или Пароль.");
            return View("Login");
        }

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("Username", user.Username);

        return RedirectToAction("Profile");
    }

    // GET: /Account/Profile
    [HttpGet]
    public IActionResult Profile()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            return RedirectToAction("Auth");
        }

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            return RedirectToAction("Auth");
        }

        return View(user);
    }

    // GET: /Account/ProfileSettings
    [HttpGet]
    public IActionResult ProfileSettings()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            return RedirectToAction("Auth");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return RedirectToAction("Auth");

        return View(user);
    }

    // POST: /Account/ProfileSettings
    [HttpPost]
    public IActionResult ProfileSettings(User updatedUser)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            return RedirectToAction("Auth");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return RedirectToAction("Auth");

        if (!ModelState.IsValid)
            return View(updatedUser);

        if (updatedUser.Email != user.Email && _context.Users.Any(u => u.Email == updatedUser.Email))
        {
            ModelState.AddModelError("Email", "Email уже используется другим пользователем.");
            return View(updatedUser);
        }

        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;

        if (!string.IsNullOrWhiteSpace(updatedUser.Password))
        {
            user.Password = updatedUser.Password;
        }

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Настройки сохранены";
        return RedirectToAction("ProfileSettings");
    }

    // Выход
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Auth");
    }
}

// Отдельный контроллер HomeController (вынесен из AccountController)
public class HomeController : Controller
{
    public IActionResult Index()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            return RedirectToAction("Auth", "Account");
        }

        ViewBag.Username = HttpContext.Session.GetString("Username");
        return View();
    }
}
