using System.ComponentModel.DataAnnotations;

namespace EduConnect.ViewModels
{
    public class SettingsPageViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsParent { get; set; }
        public ProfileInput Profile { get; set; } = new ProfileInput();
        public PasswordInput Password { get; set; } = new PasswordInput();
        public NotificationInput Notifications { get; set; } = new NotificationInput();
    }

    public class ProfileInput
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(100, ErrorMessage = "Name can be at most 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number can be at most 20 characters.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }

    public class PasswordInput
    {
        [Required(ErrorMessage = "Please enter your current password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a new password.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "The new password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the new password.")]
        [Compare(nameof(NewPassword), ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class NotificationInput
    {
        [Required(ErrorMessage = "Please choose how you want to be notified.")]
        [RegularExpression("^(Portal|SMS|Both)$", ErrorMessage = "Please choose Portal, SMS or Both.")]
        [Display(Name = "Notification Preference")]
        public string Preference { get; set; } = "Both";
    }
}