namespace EduConnect.ViewModels
{
    public class AttendanceViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ReasonForAbsence { get; set; }
    }
}
