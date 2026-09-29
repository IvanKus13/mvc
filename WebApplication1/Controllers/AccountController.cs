using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
namespace WebApplication1.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        public AuthController(AppDbContext context) { _context = context; }

       
        [HttpGet]
        public IActionResult Register() => View();

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid) return View(user);

            // примітивна перевірка на унікальність логіну
            if (_context.Users.Any(u => u.Login == user.Login))
            {
                ViewBag.Error = "Користувач із таким логіном вже існує";
                return View(user);
            }

            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Login");
        }

        
        [HttpGet]
        public IActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml");
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string login, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user == null)
            {
                ViewBag.Error = "Невірний логін чи пароль";
                return View();
            }

            // Зберігаємо вхід у сесію
            HttpContext.Session.SetString("UserLogin", user.Login);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role);

            //Після авторизації відправляємо клієнта в меню
            return RedirectToAction("Index", "Cars");
        }

       
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
