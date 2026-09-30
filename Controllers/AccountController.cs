using EduConnect.Data;
using EduConnect.Models;
using EduConnect.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly EduConnectDbContext db;

        public AccountController(EduConnectDbContext context)
        {
            db = context;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                string email = model.Email.Trim();
                User? user = db.Users.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());

                if (user != null && user.IsActive && user.ValidatePassword(model.Password))
                {
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId),
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    ClaimsIdentity identity = new ClaimsIdentity(
                        claims,
                        "EduConnectCookie");

                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("EduConnectCookie", principal);

                    db.PopiaAuditLogs.Add(new PopiaAuditLog
                    {
                        UserId = user.UserId,
                        ResourceType = "Account",
                        ActionType = "Login",
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Details = "User signed in"
                    });

                    db.SaveChanges();

                    // Send the user back to the page they originally asked for (local URLs only).
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    if (user.Role == "Teacher")
                    {
                        return RedirectToAction("Index", "Teacher");
                    }

                    if (user.Role == "Parent")
                    {
                        return RedirectToAction("Index", "Parent");
                    }

                    return RedirectToAction("Index", "Student");
                }

                // Record failed attempts against known accounts (no PII is stored for unknown emails).
                if (user != null)
                {
                    db.PopiaAuditLogs.Add(new PopiaAuditLog
                    {
                        UserId = user.UserId,
                        ResourceType = "Account",
                        ActionType = "LoginFailed",
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Details = "Failed sign-in attempt"
                    });

                    db.SaveChanges();
                }

                ModelState.AddModelError("", "Invalid email or password");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("EduConnectCookie");
            return RedirectToAction("Index", "Home");
        }
    }
}
