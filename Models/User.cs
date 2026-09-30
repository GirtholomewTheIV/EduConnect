using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace EduConnect.Models
{
    public class User
    {
        [Key]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public void SetPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32);

            PasswordHash = Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        public bool ValidatePassword(string password)
        {
            try
            {
                string[] parts = PasswordHash.Split('.');
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHash = Convert.FromBase64String(parts[1]);

                byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    100000,
                    HashAlgorithmName.SHA256,
                    storedHash.Length);

                return CryptographicOperations.FixedTimeEquals(hash, storedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
