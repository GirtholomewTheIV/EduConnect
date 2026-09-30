using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class ReportCard
    {
        [Key]
        public int ReportCardId { get; set; }

        public string StudentId { get; set; } = string.Empty;

        public string Term { get; set; } = string.Empty;

        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; } = string.Empty;

        [Display(Name = "PDF File")]
        public string PdfFilePath { get; set; } = string.Empty;

        public int Version { get; set; } = 1;

        [Display(Name = "Uploaded At")]
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
