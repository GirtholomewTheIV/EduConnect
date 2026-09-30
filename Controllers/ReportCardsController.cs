using EduConnect.Data;
using EduConnect.Helpers;
using EduConnect.Models;
using EduConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace EduConnect.Controllers
{
    [Authorize]
    public class ReportCardsController : Controller
    {
        private const long MaxFileBytes = 10 * 1024 * 1024;

        private readonly EduConnectDbContext db;
        private readonly IWebHostEnvironment environment;

        public ReportCardsController(EduConnectDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            db = context;
            environment = webHostEnvironment;
        }

        // Report cards are stored OUTSIDE wwwroot so they cannot be opened by guessing a URL.
        // They are only served through the Download action, which checks who is asking.
        private string StorageFolder
        {
            get { return Path.Combine(environment.ContentRootPath, "App_Data", "report-cards"); }
        }

        public IActionResult Index()
        {
            IQueryable<ReportCard> reportCards = db.ReportCards;

            List<string>? visible = StudentAccess.VisibleStudentUserIds(db, User, HttpContext.Session);

            if (visible != null)
            {
                reportCards = reportCards.Where(x => visible.Contains(x.StudentId));
            }

            List<ReportCard> cards = reportCards.OrderByDescending(x => x.UploadedAt).ToList();

            List<string> studentIds = cards.Select(x => x.StudentId).Distinct().ToList();

            Dictionary<string, string> names = db.Users
                .Where(x => studentIds.Contains(x.UserId))
                .ToDictionary(x => x.UserId, x => x.FullName);

            List<ReportCardViewModel> rows = cards
                .Select(x => new ReportCardViewModel
                {
                    ReportCardId = x.ReportCardId,
                    StudentName = names.TryGetValue(x.StudentId, out string? name) ? name : "Unknown student",
                    Term = x.Term,
                    AcademicYear = x.AcademicYear,
                    Version = x.Version,
                    UploadedAt = x.UploadedAt
                })
                .ToList();

            return View(rows);
        }

        public IActionResult Download(int id)
        {
            ReportCard? card = db.ReportCards.FirstOrDefault(x => x.ReportCardId == id);

            if (card == null)
            {
                return NotFound();
            }

            List<string>? visible = StudentAccess.VisibleStudentUserIds(db, User, HttpContext.Session);

            if (visible != null && !visible.Contains(card.StudentId))
            {
                return Forbid();
            }

            // Only the file name is used, so a stored path can never point outside the folder.
            string path = Path.Combine(StorageFolder, Path.GetFileName(card.PdfFilePath));

            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            AddAudit(card.StudentId, "Download", "Report card " + card.ReportCardId + " downloaded");
            db.SaveChanges();

            return PhysicalFile(path, "application/pdf");
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult Upload()
        {
            LoadStudents();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(string studentId, string term, IFormFile file)
        {
            term = (term ?? string.Empty).Trim();

            if (term.Length == 0)
            {
                ModelState.AddModelError("", "Please enter the term.");
            }

            if (!db.Students.Any(x => x.UserId == studentId))
            {
                ModelState.AddModelError("", "Please select a student.");
            }

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a PDF file.");
            }
            else if (file.Length > MaxFileBytes)
            {
                ModelState.AddModelError("", "The file is too large (10 MB maximum).");
            }
            else if (Path.GetExtension(file.FileName).ToLower() != ".pdf" || !LooksLikePdf(file))
            {
                ModelState.AddModelError("", "Only PDF files are allowed.");
            }

            if (ModelState.IsValid && file != null)
            {
                Directory.CreateDirectory(StorageFolder);

                string fileName = Guid.NewGuid().ToString() + ".pdf";
                string filePath = Path.Combine(StorageFolder, fileName);

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                int version = db.ReportCards
                    .Where(x => x.StudentId == studentId && x.Term == term)
                    .Select(x => (int?)x.Version)
                    .Max() ?? 0;

                db.ReportCards.Add(new ReportCard
                {
                    StudentId = studentId,
                    Term = term,
                    AcademicYear = DateTime.Now.Year.ToString(),
                    PdfFilePath = fileName,
                    Version = version + 1
                });

                AddAudit(studentId, "Upload", "Report card uploaded for " + term);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            LoadStudents();
            return View();
        }

        // A file name ending in .pdf proves nothing, so also check the "%PDF-" header.
        private static bool LooksLikePdf(IFormFile file)
        {
            byte[] header = new byte[5];

            using (Stream stream = file.OpenReadStream())
            {
                int read = stream.Read(header, 0, header.Length);
                return read == header.Length && Encoding.ASCII.GetString(header) == "%PDF-";
            }
        }

        private void LoadStudents()
        {
            ViewBag.Students = db.Students
                .Include(x => x.User)
                .OrderBy(x => x.User.FullName)
                .ToList();
        }

        private void AddAudit(string targetUserId, string action, string details)
        {
            db.PopiaAuditLogs.Add(new PopiaAuditLog
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                TargetUserId = targetUserId,
                ResourceType = "ReportCard",
                ActionType = action,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = details
            });
        }
    }
}
