using EduConnect.Data;
using EduConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly EduConnectDbContext db;

        public AdminController(EduConnectDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            return View(db.Users.OrderBy(x => x.Role).ThenBy(x => x.FullName).ToList());
        }

        public IActionResult Users()
        {
            return View(db.Users.OrderBy(x => x.Role).ThenBy(x => x.FullName).ToList());
        }

        public IActionResult Classes()
        {
            List<Classroom> classrooms = db.Classrooms
                .Include(x => x.ClassTeacher)
                .ThenInclude(x => x.User)
                .ToList();

            return View(classrooms);
        }

        public IActionResult Audit()
        {
            List<PopiaAuditLog> logs = db.PopiaAuditLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(50)
                .ToList();

            // Show user names instead of raw ids.
            List<string> ids = logs.Select(x => x.UserId).Distinct().ToList();

            ViewBag.UserNames = db.Users
                .Where(x => ids.Contains(x.UserId))
                .ToDictionary(x => x.UserId, x => x.FullName);

            return View(logs);
        }
    }
}
