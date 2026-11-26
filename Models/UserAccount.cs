using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.Models
{
    // A login account (table: Users). Every patient, doctor, and staff member has one.
    public class UserAccount
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // The password is NEVER stored as plain text, only as a hash (ACC-3).
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Patient, Doctor, or Staff (see Roles.cs)
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty;
    }
}
