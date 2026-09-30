using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class Parent
    {
        [Key]
        public string UserId { get; set; } = string.Empty;

        public User User { get; set; } = null!;

        [Display(Name = "Notification Preference")]
        public string Preference { get; set; } = "Both";

        [Display(Name = "Residential Address")]
        public string? ResidentialAddress { get; set; }
    }
}
