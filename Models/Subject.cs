using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class Subject
    {
        [Key]
        public int SubjectId { get; set; }

        [Required]
        [Display(Name = "Subject Name")]
        public string SubjectName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Subject Code")]
        public string SubjectCode { get; set; } = string.Empty;
    }
}
