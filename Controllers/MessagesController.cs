using EduConnect.Data;
using EduConnect.Models;
using EduConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly EduConnectDbContext db;

        public MessagesController(EduConnectDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            List<ChatMessage> messages = db.ChatMessages
                .Where(x => x.SenderId == userId || x.ReceiverId == userId)
                .OrderByDescending(x => x.SentAt)
                .ToList();

            // Show names instead of raw user ids.
            List<string> ids = messages
                .SelectMany(x => new[] { x.SenderId, x.ReceiverId })
                .Distinct()
                .ToList();

            Dictionary<string, string> names = db.Users
                .Where(x => ids.Contains(x.UserId))
                .ToDictionary(x => x.UserId, x => x.FullName);

            List<MessageListItemViewModel> rows = messages
                .Select(x => new MessageListItemViewModel
                {
                    SentAt = x.SentAt,
                    FromName = names.TryGetValue(x.SenderId, out string? from) ? from : "Unknown user",
                    ToName = names.TryGetValue(x.ReceiverId, out string? to) ? to : "Unknown user",
                    MessageText = x.MessageText,
                    IsRead = x.IsRead
                })
                .ToList();

            // Messages sent to this user are now read (rows above keep the previous state).
            bool changed = false;

            foreach (ChatMessage message in messages.Where(x => x.ReceiverId == userId && !x.IsRead))
            {
                message.IsRead = true;
                changed = true;
            }

            if (changed)
            {
                db.SaveChanges();
            }

            return View(rows);
        }

        public IActionResult Send()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Send(MessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                string senderId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Same case-insensitive, trimmed matching that Login uses.
                string email = model.ReceiverEmail.Trim().ToLower();
                User? receiver = db.Users.FirstOrDefault(x => x.Email.ToLower() == email);

                if (receiver == null || !receiver.IsActive)
                {
                    ModelState.AddModelError("", "Recipient was not found.");
                    return View(model);
                }

                db.ChatMessages.Add(new ChatMessage
                {
                    SenderId = senderId,
                    ReceiverId = receiver.UserId,
                    MessageText = model.MessageText,
                    SentAt = DateTime.Now
                });

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}
