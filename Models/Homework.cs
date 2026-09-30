using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class Homework
    {
        [Key]
        public int HomeworkId { get; set; }

        public int ClassroomId { get; set; }
        public int SubjectId { get; set; }
        public string TeacherId { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        public bool IsOverdue
        {
            get { return DateTime.Today > DueDate.Date; }
        }
    }
}
