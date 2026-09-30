using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class ChatMessage
    {
        [Key]
        public int MessageId { get; set; }

        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Message")]
        public string MessageText { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; }
    }
}
