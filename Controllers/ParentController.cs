using EduConnect.Data;
using EduConnect.Helpers;
using EduConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    [Authorize(Roles = "Parent")]
    public class ParentController : Controller
    {
        private readonly EduConnectDbContext db;

        public ParentController(EduConnectDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            List<Student> children = db.Students
                .Where(x => x.ParentId == userId)
                .Include(x => x.User)
                .Include(x => x.Classroom)
                .ToList();

            string? selected = HttpContext.Session.GetString(StudentAccess.SelectedChildKey);
            ViewBag.SelectedUserId = children.Any(x => x.UserId == selected) ? selected : null;

            return View(children);
        }

        public IActionResult SwitchChild(string id)
        {
            string parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // "all" clears the selection so every child is shown again.
            if (id == "all")
            {
                HttpContext.Session.Remove(StudentAccess.SelectedChildKey);
                return RedirectToAction("Index");
            }

            Student? child = db.Students
                .Include(x => x.User)
                .FirstOrDefault(x => x.StudentNumber == id && x.ParentId == parentId);

            if (child == null)
            {
                return NotFound();
            }

            HttpContext.Session.SetString(StudentAccess.SelectedChildKey, child.UserId);

            return RedirectToAction("Index");
        }
    }
}
