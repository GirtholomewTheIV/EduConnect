using System.ComponentModel.DataAnnotations;

namespace EduConnect.ViewModels
{
    public class MaterialViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Subject")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a subject.")]
        public int SubjectId { get; set; }

        public string? Description { get; set; }

        [Required]
        [Display(Name = "File Path")]
        public IFormFile? File { get; set; }
    }
}
