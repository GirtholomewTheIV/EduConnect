using EduConnect.Data;
using EduConnect.Models;
using EduConnect.Services;
using EduConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;

namespace EduConnect.Controllers
{
    [AllowAnonymous]
    public class PasswordResetController : Controller
    {
        private const int TokenMinutes = 30;

        private readonly EduConnectDbContext db;
        private readonly IDistributedCache cache;
        private readonly IEmailService email;

        public PasswordResetController(
            EduConnectDbContext context,
            IDistributedCache distributedCache,
            IEmailService emailService)
        {
            db = context;
            cache = distributedCache;
            email = emailService;
        }

        [HttpGet]
        public IActionResult Forgot()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Forgot(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string address = model.Email.Trim();
            User? user = db.Users.FirstOrDefault(x => x.Email.ToLower() == address.ToLower());

            if (user != null && user.IsActive)
            {
                string token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

                await cache.SetStringAsync(
                    CacheKey(token),
                    user.UserId,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(TokenMinutes)
                    });

                string link = Url.Action(nameof(Reset), "PasswordReset", new { token }, Request.Scheme)!;
                await SendResetEmailAsync(user, link);

                AddAudit(user.UserId, "PasswordResetRequested", "A password reset link was requested");
                db.SaveChanges();
            }

            // The same answer is shown whether or not the email exists,
            // so the form cannot be used to find out who has an account.
            model.Sent = true;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Reset(string? token)
        {
            if (string.IsNullOrWhiteSpace(token) || await GetUserIdAsync(token) == null)
            {
                return View(new ResetPasswordViewModel { Invalid = true });
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset(ResetPasswordViewModel model)
        {
            string? userId = string.IsNullOrWhiteSpace(model.Token)
                ? null
                : await GetUserIdAsync(model.Token);

            if (userId == null)
            {
                return View(new ResetPasswordViewModel { Invalid = true });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User? user = db.Users.FirstOrDefault(x => x.UserId == userId);
            if (user == null || !user.IsActive)
            {
                return View(new ResetPasswordViewModel { Invalid = true });
            }

            user.SetPassword(model.NewPassword);

            // The password itself is never written to the audit log
            AddAudit(user.UserId, "PasswordReset", "Password was reset using a reset link");
            db.SaveChanges();

            // Single use: the link stops working straight away
            await cache.RemoveAsync(CacheKey(model.Token));

            return View("Done");
        }

        // ---------- Helpers ----------

        private static string CacheKey(string token)
        {
            // Only a hash of the token is used as the cache key
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return "pwreset:" + Convert.ToHexString(hash);
        }

        private async Task<string?> GetUserIdAsync(string token)
        {
            return await cache.GetStringAsync(CacheKey(token));
        }

        private Task SendResetEmailAsync(User user, string link)
        {
            string body =
                "Hello " + user.FullName + ",\n\n" +
                "We received a request to reset your EduConnect password.\n\n" +
                "Open this link to choose a new password. It works once and is valid for " +
                TokenMinutes + " minutes:\n" + link + "\n\n" +
                "If you did not ask for this, you can ignore this email and your password will not change.\n";

            return email.SendAsync(user.Email, "Reset your EduConnect password", body);
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
    }
}