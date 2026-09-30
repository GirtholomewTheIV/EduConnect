using EduConnect.Data;
using EduConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly EduConnectDbContext db;

        public StudentController(EduConnectDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            Student? student = db.Students
                .Include(x => x.User)
                .Include(x => x.Classroom)
                .FirstOrDefault(x => x.UserId == userId);

            if (student == null)
            {
                return View("NoProfile");
            }

            return View(student);
        }
    }
}
