using EduConnect.Data;
using EduConnect.Helpers;
using EduConnect.Models;
using EduConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    [Authorize]
    public class GradesController : Controller
    {
        private readonly EduConnectDbContext db;

        public GradesController(EduConnectDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            List<string>? visible = StudentAccess.VisibleStudentUserIds(db, User, HttpContext.Session);

            IQueryable<TestMark> marks = db.TestMarks;

            if (visible != null)
            {
                marks = marks.Where(x => visible.Contains(x.StudentId));
            }

            List<GradeViewModel> result = marks
                .Join(db.Users,
                    mark => mark.StudentId,
                    student => student.UserId,
                    (mark, student) => new { mark, student })
                .Join(db.Subjects,
                    item => item.mark.SubjectId,
                    subject => subject.SubjectId,
                    (item, subject) => new GradeViewModel
                    {
                        MarkId = item.mark.MarkId,
                        StudentName = item.student.FullName,
                        SubjectName = subject.SubjectName,
                        Score = item.mark.Score,
                        MaxScore = item.mark.MaxScore,
                        Percentage = item.mark.CalculatePercentage(),
                        AssessmentDate = item.mark.AssessmentDate,
                        Notes = item.mark.Notes
                    })
                .OrderByDescending(x => x.AssessmentDate)
                .ToList();

            return View(result);
        }

        // ---------- Add a mark (teachers only) ----------

        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View(new TestMark { AssessmentDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public IActionResult Create(
            [Bind("StudentId,SubjectId,Score,MaxScore,AssessmentDate,Notes")] TestMark mark)
        {
            if (mark.Score > mark.MaxScore)
            {
                ModelState.AddModelError(nameof(TestMark.Score),
                    "Score cannot be higher than the maximum score.");
            }

            if (!db.Users.Any(u => u.UserId == mark.StudentId && u.Role == "Student"))
            {
                ModelState.AddModelError(nameof(TestMark.StudentId), "Please select a learner.");
            }

            if (!db.Subjects.Any(s => s.SubjectId == mark.SubjectId))
            {
                ModelState.AddModelError(nameof(TestMark.SubjectId), "Please select a subject.");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(mark);
            }

            mark.Notes = string.IsNullOrWhiteSpace(mark.Notes) ? null : mark.Notes.Trim();
            db.TestMarks.Add(mark);
            db.SaveChanges();

            TempData["Message"] = "Mark saved.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Edit a mark (teachers only) ----------

        [Authorize(Roles = "Teacher")]
        public IActionResult Edit(int id)
        {
            TestMark? mark = db.TestMarks.FirstOrDefault(x => x.MarkId == id);
            if (mark == null)
            {
                return NotFound();
            }

            LoadNames(mark);
            return View(mark);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public IActionResult Edit(int id,
            [Bind("Score,MaxScore,AssessmentDate,Notes")] TestMark posted)
        {
            TestMark? mark = db.TestMarks.FirstOrDefault(x => x.MarkId == id);
            if (mark == null)
            {
                return NotFound();
            }

            // Learner and subject are fixed once a mark exists
            ModelState.Remove(nameof(TestMark.StudentId));
            ModelState.Remove(nameof(TestMark.SubjectId));

            if (posted.Score > posted.MaxScore)
            {
                ModelState.AddModelError(nameof(TestMark.Score),
                    "Score cannot be higher than the maximum score.");
            }

            if (!ModelState.IsValid)
            {
                posted.MarkId = id;
                posted.StudentId = mark.StudentId;
                posted.SubjectId = mark.SubjectId;
                LoadNames(mark);
                return View(posted);
            }

            mark.Score = posted.Score;
            mark.MaxScore = posted.MaxScore;
            mark.AssessmentDate = posted.AssessmentDate;
            mark.Notes = string.IsNullOrWhiteSpace(posted.Notes) ? null : posted.Notes.Trim();
            db.SaveChanges();

            TempData["Message"] = "Mark updated.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Helpers ----------

        private void LoadDropdowns()
        {
            ViewBag.Students = db.Users
                .Where(u => u.Role == "Student" && u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => new SelectListItem { Value = u.UserId, Text = u.FullName })
                .ToList();

            ViewBag.Subjects = db.Subjects
                .OrderBy(s => s.SubjectName)
                .Select(s => new SelectListItem
                {
                    Value = s.SubjectId.ToString(),
                    Text = s.SubjectName
                })
                .ToList();
        }

        private void LoadNames(TestMark mark)
        {
            ViewBag.StudentName = db.Users
                .Where(u => u.UserId == mark.StudentId)
                .Select(u => u.FullName)
                .FirstOrDefault() ?? "Unknown";

            ViewBag.SubjectName = db.Subjects
                .Where(s => s.SubjectId == mark.SubjectId)
                .Select(s => s.SubjectName)
                .FirstOrDefault() ?? "Unknown";
        }
    }
}