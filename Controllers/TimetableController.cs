using EduConnect.Data;
using EduConnect.Helpers;
using EduConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    [Authorize]
    public class TimetableController : Controller
    {
        private static readonly string[] DayOrder =
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };

        private readonly EduConnectDbContext db;

        public TimetableController(EduConnectDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            IQueryable<TimetableEntry> query = db.TimetableEntries
                .Include(x => x.Subject)
                .Include(x => x.Classroom);

            List<string>? visible = StudentAccess.VisibleStudentUserIds(db, User, HttpContext.Session);

            if (visible != null)
            {
                // Students and parents: only the class(es) of the students they may see.
                List<int> classroomIds = StudentAccess.ClassroomIds(db, visible);
                query = query.Where(x => classroomIds.Contains(x.ClassroomId));
            }
            else if (User.IsInRole("Teacher"))
            {
                // Teachers: only the lessons they teach.
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                string? staffNumber = db.Teachers
                    .Where(x => x.UserId == userId)
                    .Select(x => x.StaffNumber)
                    .FirstOrDefault();

                query = query.Where(x => x.TeacherId == staffNumber);
            }

            // Sort by weekday order, not alphabetically (Friday would otherwise come before Monday).
            List<TimetableEntry> timetable = query
                .ToList()
                .OrderBy(x => Array.IndexOf(DayOrder, x.Day))
                .ThenBy(x => x.StartTime)
                .ToList();

            return View(timetable);
        }
    }
}
