using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class AttendanceRecord
    {
        [Key]
        public int AttendanceId { get; set; }

        public string StudentId { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now.Date;

        [Required]
        public string Status { get; set; } = "Present";

        [Display(Name = "Reason For Absence")]
        public string? ReasonForAbsence { get; set; }
    }
}
