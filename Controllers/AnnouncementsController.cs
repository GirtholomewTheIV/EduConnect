using EduConnect.Data;
using EduConnect.Models;
using EduConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly EduConnectDbContext db;

        public AnnouncementsController(EduConnectDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            List<Announcement> announcements = db.Announcements
                .OrderByDescending(x => x.DatePosted)
                .ToList();

            return View(announcements);
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AnnouncementViewModel model)
        {
            if (ModelState.IsValid)
            {
                string teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                db.Announcements.Add(new Announcement
                {
                    TeacherId = teacherId,
                    Title = model.Title,
                    Content = model.Content,
                    DatePosted = DateTime.Now,
                    IsMeeting = model.IsMeeting,
                    SendSms = model.SendSms
                });

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // State-changing action: POST + anti-forgery token (a GET link could be triggered by any page).
        [HttpPost]
        [Authorize(Roles = "Parent")]
        [ValidateAntiForgeryToken]
        public IActionResult Rsvp(int id, string status)
        {
            if (status != "Going" && status != "Not Going")
            {
                return BadRequest();
            }

            if (!db.Announcements.Any(x => x.AnnouncementId == id && x.IsMeeting))
            {
                return NotFound();
            }

            string parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            AnnouncementRsvp? response = db.AnnouncementRsvps
                .FirstOrDefault(x => x.AnnouncementId == id && x.ParentId == parentId);

            if (response == null)
            {
                response = new AnnouncementRsvp
                {
                    AnnouncementId = id,
                    ParentId = parentId
                };

                db.AnnouncementRsvps.Add(response);
            }

            response.Status = status;
            response.HasRead = true;
            response.RespondedAt = DateTime.Now;

            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
