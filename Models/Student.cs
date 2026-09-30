using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class Student
    {
        [Key]
        [Display(Name = "Student Number")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public User User { get; set; } = null!;

        [Display(Name = "Grade Level")]
        public int GradeLevel { get; set; }

        [Display(Name = "Classroom")]
        public int ClassroomId { get; set; }

        public Classroom Classroom { get; set; } = null!;

        [Required]
        public string ParentId { get; set; } = string.Empty;

        public Parent Parent { get; set; } = null!;
    }
}
