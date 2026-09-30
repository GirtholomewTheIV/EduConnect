namespace EduConnect.ViewModels
{
    public class ReportCardViewModel
    {
        public int ReportCardId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
