using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class CourseMaterial
    {
        [Key]
        public int MaterialId { get; set; }

        public int SubjectId { get; set; }
        public string TeacherId { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Display(Name = "File Path")]
        public string FilePath { get; set; } = string.Empty;
        
    }
}
