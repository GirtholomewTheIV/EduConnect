using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class Classroom
    {
        [Key]
        public int ClassroomId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Grade Level")]
        public int GradeLevel { get; set; }

        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; } = string.Empty;

        [Display(Name = "Class Teacher")]
        public string ClassTeacherId { get; set; } = string.Empty;

        public Teacher ClassTeacher { get; set; } = null!;
    }
}
