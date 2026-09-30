namespace EduConnect.ViewModels
{
    public class MessageListItemViewModel
    {
        public DateTime SentAt { get; set; }
        public string FromName { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public bool IsRead { get; set; }
    }
}
