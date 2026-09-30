using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class TestMark
    {
        [Key]
        public int MarkId { get; set; }

        public string StudentId { get; set; } = string.Empty;
        public int SubjectId { get; set; }

        [Display(Name = "Score")]
        [Range(typeof(decimal), "0", "1000", ErrorMessage = "Score must be between 0 and 1000.")]
        public decimal Score { get; set; }

        [Display(Name = "Maximum Score")]
        [Range(typeof(decimal), "1", "1000", ErrorMessage = "Maximum score must be between 1 and 1000.")]
        public decimal MaxScore { get; set; } = 100;

        [Display(Name = "Assessment Date")]
        [DataType(DataType.Date)]
        public DateTime AssessmentDate { get; set; } = DateTime.Now;

        [Display(Name = "Progress Notes")]
        [StringLength(1000, ErrorMessage = "Notes can be at most 1000 characters.")]
        public string? Notes { get; set; }

        public decimal CalculatePercentage()
        {
            if (MaxScore == 0)
            {
                return 0;
            }

            return Math.Round(Score / MaxScore * 100, 2);
        }
    }
}