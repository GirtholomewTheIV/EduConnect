using EduConnect.Data;
using EduConnect.Models;
using EduConnect.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly EduConnectDbContext db;

        public SettingsController(EduConnectDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            User? user = GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(BuildPage(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileInput profile)
        {
            User? user = GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                SettingsPageViewModel page = BuildPage(user);
                page.Profile = profile;
                return View("Index", page);
            }

            user.FullName = profile.FullName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(profile.PhoneNumber)
                ? null
                : profile.PhoneNumber.Trim();

            AddAudit(user.UserId, "ProfileUpdate", "User updated their name or phone number");
            db.SaveChanges();

            // Refresh the sign-in cookie so the sidebar shows the new name straight away
            await SignInAgain(user);

            TempData["Message"] = "Your profile has been updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(PasswordInput password)
        {
            User? user = GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid && !user.ValidatePassword(password.CurrentPassword))
            {
                ModelState.AddModelError("Password.CurrentPassword", "Your current password is incorrect.");
            }

            if (ModelState.IsValid && password.NewPassword == password.CurrentPassword)
            {
                ModelState.AddModelError("Password.NewPassword", "The new password must be different from your current one.");
            }

            if (!ModelState.IsValid)
            {
                // Passwords are never sent back to the page
                return View("Index", BuildPage(user));
            }

            user.SetPassword(password.NewPassword);

            // The password itself is never written to the audit log
            AddAudit(user.UserId, "PasswordChange", "User changed their password");
            db.SaveChanges();

            TempData["Message"] = "Your password has been changed.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateNotifications(NotificationInput notifications)
        {
            User? user = GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Parent? parent = db.Parents.FirstOrDefault(p => p.UserId == user.UserId);
            if (parent == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                SettingsPageViewModel page = BuildPage(user);
                page.Notifications = notifications;
                return View("Index", page);
            }

            parent.Preference = notifications.Preference;

            AddAudit(user.UserId, "NotificationPreferenceUpdate",
                "Parent set notification preference to " + notifications.Preference);
            db.SaveChanges();

            TempData["Message"] = "Your notification preference has been saved.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Helpers ----------

        private User? GetCurrentUser()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return db.Users.FirstOrDefault(x => x.UserId == userId);
        }

        private SettingsPageViewModel BuildPage(User user)
        {
            Parent? parent = db.Parents.FirstOrDefault(p => p.UserId == user.UserId);

            return new SettingsPageViewModel
            {
                Email = user.Email,
                Role = user.Role,
                IsParent = parent != null,
                Profile = new ProfileInput
                {
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber
                },
                Notifications = new NotificationInput
                {
                    Preference = parent?.Preference ?? "Both"
                }
            };
        }

        private void AddAudit(string userId, string actionType, string details)
        {
            db.PopiaAuditLogs.Add(new PopiaAuditLog
            {
                UserId = userId,
                ResourceType = "Account",
                ActionType = actionType,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = details
            });
        }

        private async Task SignInAgain(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "EduConnectCookie");
            await HttpContext.SignInAsync("EduConnectCookie", new ClaimsPrincipal(identity));
        }
    }
}