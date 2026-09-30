using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class Teacher
    {
        [Key]
        [Display(Name = "Staff Number")]
        public string StaffNumber { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public User User { get; set; } = null!;

        [Display(Name = "Subject Specialization")]
        public string? SubjectSpecialization { get; set; }
    }
}
