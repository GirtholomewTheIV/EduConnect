using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class TimetableEntry
    {
        [Key]
        public int TimetableId { get; set; }

        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; } = null!;

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public string TeacherId { get; set; } = string.Empty;
        public Teacher Teacher { get; set; } = null!;

        public string Day { get; set; } = string.Empty;

        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Room Number")]
        public string RoomNumber { get; set; } = string.Empty;
    }
}
