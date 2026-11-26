using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.ViewModels
{
    // A ViewModel = "what this particular page needs" (as taught in class).
    // The Login page only needs an email, a password, and a Remember Me checkbox.
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
