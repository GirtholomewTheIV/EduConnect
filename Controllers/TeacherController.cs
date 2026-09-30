using EduConnect.Data;
using EduConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly EduConnectDbContext db;

        public TeacherController(EduConnectDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            Teacher? teacher = db.Teachers
                .Include(x => x.User)
                .FirstOrDefault(x => x.UserId == userId);

            if (teacher == null)
            {
                return View("NoProfile");
            }

            return View(teacher);
        }
    }
}
