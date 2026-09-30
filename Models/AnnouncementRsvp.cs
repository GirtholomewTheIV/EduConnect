using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class AnnouncementRsvp
    {
        [Key]
        public int RsvpId { get; set; }

        public int AnnouncementId { get; set; }
        public string ParentId { get; set; } = string.Empty;

        public bool HasRead { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime RespondedAt { get; set; } = DateTime.Now;
    }
}
