using System.ComponentModel.DataAnnotations;

namespace EduConnect.ViewModels
{
    public class MessageViewModel
    {
        [Required]
        [Display(Name = "Recipient Email")]
        public string ReceiverEmail { get; set; } = string.Empty;

        [Required]
        public string MessageText { get; set; } = string.Empty;
    }
}
