using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be 2 to 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birth date is required.")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Choose Male, Female, or Other.")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^[0-9+\-\s]{7,15}$", ErrorMessage = "Enter a valid contact number (7 to 15 digits).")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address can have at most 200 characters.")]
        public string Address { get; set; } = string.Empty;

        // Navigation properties (the "?" means they may be empty; this keeps them
        // from being treated as required form fields).
        public UserAccount? User { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
