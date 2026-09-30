using System.ComponentModel.DataAnnotations;

namespace EduConnect.ViewModels
{
    public class AnnouncementViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Meeting")]
        public bool IsMeeting { get; set; }

        [Display(Name = "Send SMS")]
        public bool SendSms { get; set; }
    }
}
