using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        public string TeacherId { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Date Posted")]
        public DateTime DatePosted { get; set; } = DateTime.Now;

        [Display(Name = "Meeting")]
        public bool IsMeeting { get; set; }

        [Display(Name = "Send SMS")]
        public bool SendSms { get; set; }
    }
}
