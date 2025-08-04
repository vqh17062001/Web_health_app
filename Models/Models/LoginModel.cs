using System.ComponentModel.DataAnnotations;

namespace Web_health_app.Models.Models
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
    public class FirstChangePasswordModel
    {
        [Required(ErrorMessage = "Current Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
