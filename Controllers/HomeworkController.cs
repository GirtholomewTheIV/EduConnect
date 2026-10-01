using EduConnect.Data;
using EduConnect.Helpers;
using EduConnect.Models;
using EduConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    [Authorize]
    public class HomeworkController : Controller
    {
        private readonly EduConnectDbContext db;

        public HomeworkController(EduConnectDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            IQueryable<Homework> homeworks = db.Homeworks;

            // Students and parents only see homework for their own class(es).
            List<string>? visible = StudentAccess.VisibleStudentUserIds(db, User, HttpContext.Session);

            if (visible != null)
            {
                List<int> classroomIds = StudentAccess.ClassroomIds(db, visible);
                homeworks = homeworks.Where(x => classroomIds.Contains(x.ClassroomId));
            }

            return View(homeworks.OrderBy(x => x.DueDate).ToList());
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            ViewBag.Subjects = db.Subjects.OrderBy(x => x.SubjectName).ToList();
            return View(new HomeworkViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(HomeworkViewModel model)
        {
            string teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            Classroom? classroom = null;
            Teacher? teacher = db.Teachers.FirstOrDefault(x => x.UserId == teacherId);

            if (teacher == null)
            {
                ModelState.AddModelError("", "Your account does not have a teacher profile yet.");
            }
            else
            {
                classroom = db.Classrooms.FirstOrDefault(x => x.ClassTeacherId == teacher.StaffNumber);

                if (classroom == null)
                {
                    ModelState.AddModelError("", "You are not assigned as a class teacher, so there is no class to set homework for.");
                }
            }

            if (!db.Subjects.Any(x => x.SubjectId == model.SubjectId))
            {
                ModelState.AddModelError(nameof(model.SubjectId), "Please select a subject.");
            }

            if (ModelState.IsValid && classroom != null)
            {
                db.Homeworks.Add(new Homework
                {
                    ClassroomId = classroom.ClassroomId,
                    SubjectId = model.SubjectId,
                    TeacherId = teacherId,
                    Title = model.Title,
                    DueDate = model.DueDate
                });

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Subjects = db.Subjects.OrderBy(x => x.SubjectName).ToList();
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            Homework? homework = db.Homeworks.FirstOrDefault(x => x.HomeworkId == id);
            if (homework == null) return NotFound();

            db.Homeworks.Remove(homework);

            db.PopiaAuditLogs.Add(new PopiaAuditLog
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                TargetUserId = homework.TeacherId,
                ResourceType = "Homework",
                ActionType = "Delete",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = "Admin deleted homework: " + homework.Title
            });

            db.SaveChanges();

            TempData["Message"] = "Homework deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
