using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.Models
{
    public class ClinicStaff
    {
        [Key]
        public int StaffId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Position is required.")]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        public UserAccount? User { get; set; }
    }
}
