using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class PopiaAuditLog
    {
        [Key]
        public int LogId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string? TargetUserId { get; set; }

        public string ResourceType { get; set; } = string.Empty;

        public string ActionType { get; set; } = string.Empty;

        public string? IpAddress { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string? Details { get; set; }
    }
}
