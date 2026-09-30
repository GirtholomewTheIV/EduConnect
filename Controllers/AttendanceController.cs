using EduConnect.Data;
using EduConnect.Helpers;
using EduConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly EduConnect.Data.EduConnectDbContext db;

        public AttendanceController(EduConnectDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            List<string>? visible = StudentAccess.VisibleStudentUserIds(db, User, HttpContext.Session);

            IQueryable<Models.AttendanceRecord> records = db.AttendanceRecords;

            if (visible != null)
            {
                records = records.Where(x => visible.Contains(x.StudentId));
            }

            List<AttendanceViewModel> result = records
                .Join(db.Users,
                    record => record.StudentId,
                    student => student.UserId,
                    (record, student) => new AttendanceViewModel
                    {
                        StudentName = student.FullName,
                        Date = record.Date,
                        Status = record.Status,
                        ReasonForAbsence = record.ReasonForAbsence
                    })
                .OrderByDescending(x => x.Date)
                .ToList();

            return View(result);
        }
    }
}
