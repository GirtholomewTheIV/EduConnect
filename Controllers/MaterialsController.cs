using EduConnect.Data;
using EduConnect.Models;
using EduConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    [Authorize]
    public class MaterialsController : Controller
    {
        private readonly EduConnectDbContext db;
        private const long MaxFileBytes = 10 * 1024 * 1024;
        private readonly IWebHostEnvironment environment;
        private string StorageFolder => Path.Combine(environment.WebRootPath, "uploads", "materials");

        public MaterialsController(EduConnectDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            db = context;
            environment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<CourseMaterial> materials = db.CourseMaterials
                .OrderBy(x => x.Title)
                .ToList();

            return View(materials);
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            ViewBag.Subjects = db.Subjects.OrderBy(x => x.SubjectName).ToList();
            return View(new MaterialViewModel());
        }

        // A view model is used instead of the entity: binding CourseMaterial directly made
        // ModelState invalid (TeacherId is never posted) and allowed over-posting of the key.
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MaterialViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError(nameof(model.File), "Please select a file.");
            }
            else if (model.File.Length > MaxFileBytes)
            {
                ModelState.AddModelError(nameof(model.File), "The file is too large (10 MB maximum).");
            }

            if (ModelState.IsValid && model.File != null)
            {
                Directory.CreateDirectory(StorageFolder);
                string extension = Path.GetExtension(model.File.FileName);
                string safeName = Guid.NewGuid().ToString("N") + extension;
                string path = Path.Combine(StorageFolder, safeName);
                using (FileStream stream = new(path, FileMode.Create)) model.File.CopyTo(stream);

                db.CourseMaterials.Add(new CourseMaterial
                {
                    SubjectId = model.SubjectId,
                    TeacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                    Title = model.Title.Trim(),
                    Description = model.Description?.Trim(),
                    FilePath = safeName
                });
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }


            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            CourseMaterial? material = db.CourseMaterials.FirstOrDefault(x => x.MaterialId == id);
            if (material == null) return NotFound();

            // Remove the physical file first
            string path = Path.Combine(StorageFolder, Path.GetFileName(material.FilePath));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

            db.CourseMaterials.Remove(material);

            db.PopiaAuditLogs.Add(new PopiaAuditLog
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                TargetUserId = material.TeacherId,
                ResourceType = "CourseMaterial",
                ActionType = "Delete",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = "Admin deleted material: " + material.Title
            });

            db.SaveChanges();

            TempData["Message"] = "Material deleted.";
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Download(int id)
        {
            CourseMaterial? material = db.CourseMaterials.FirstOrDefault(x => x.MaterialId == id);
            if (material == null) return NotFound();

            string path = Path.Combine(StorageFolder, Path.GetFileName(material.FilePath));
            if (!System.IO.File.Exists(path)) return NotFound();

            string contentType = "application/octet-stream";
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".pdf") contentType = "application/pdf";
            else if (extension == ".doc" || extension == ".docx") contentType = "application/msword";
            else if (extension == ".ppt" || extension == ".pptx") contentType = "application/vnd.ms-powerpoint";

            return PhysicalFile(path, contentType, Path.GetFileName(material.FilePath));
        }
        public IActionResult Open(int id)
        {
            CourseMaterial? material = db.CourseMaterials
                .FirstOrDefault(x => x.MaterialId == id);

            if (material == null)
            {
                return NotFound();
            }

            // Use the SAME storage location as the Download action
            string path = Path.Combine(
                StorageFolder,
                Path.GetFileName(material.FilePath)
            );

            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            // Work out the correct type of file
            string contentType = "application/octet-stream";

            string extension = Path.GetExtension(path).ToLowerInvariant();

            if (extension == ".pdf")
            {
                contentType = "application/pdf";
            }
            else if (extension == ".jpg" || extension == ".jpeg")
            {
                contentType = "image/jpeg";
            }
            else if (extension == ".png")
            {
                contentType = "image/png";
            }
            else if (extension == ".mp4")
            {
                contentType = "video/mp4";
            }

            // Do NOT provide a download filename here.
            // This allows the browser to display supported files.
            return PhysicalFile(path, contentType);
        }


    }

}
