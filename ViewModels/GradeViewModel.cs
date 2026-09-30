using System.ComponentModel.DataAnnotations;

namespace EduConnect.ViewModels
{
    public class GradeViewModel
    {
        public int MarkId { get; set; }

        [Display(Name = "Learner")]
        public string StudentName { get; set; } = string.Empty;

        [Display(Name = "Subject")]
        public string SubjectName { get; set; } = string.Empty;

        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }

        [Display(Name = "Date")]
        public DateTime AssessmentDate { get; set; }

        public string? Notes { get; set; }
    }
}
