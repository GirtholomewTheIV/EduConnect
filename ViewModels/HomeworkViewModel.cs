using System.ComponentModel.DataAnnotations;

namespace EduConnect.ViewModels
{
    public class HomeworkViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

        [Display(Name = "Subject")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a subject.")]
        public int SubjectId { get; set; }
    }
}
