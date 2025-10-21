using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using LecturerClaimSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LecturerClaimSystem.Controllers
{
    public class AccountController : Controller
    {
        // Simple fake user store (replace later with database)
        private static readonly List<User> users = new List<User>
        {
            new User { Username = "coordinator", Password = "123", Role = "ProgrammeCoordinator" },
            new User { Username = "manager", Password = "123", Role = "AcademicManager" },
            new User { Username = "lecturer", Password = "123", Role = "Lecturer" }
        };

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = users.Find(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                // ✅ Use System.Security.Claims.Claim explicitly
                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
