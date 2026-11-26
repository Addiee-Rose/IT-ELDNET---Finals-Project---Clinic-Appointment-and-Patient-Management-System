using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Date")]
        public DateTime AppointmentDate { get; set; }

        [DataType(DataType.Time)]
        [Column(TypeName = "time")]
        [Display(Name = "Time")]
        public TimeSpan AppointmentTime { get; set; }

        [Required(ErrorMessage = "Please give the reason for the visit.")]
        [StringLength(300, ErrorMessage = "Reason can have at most 300 characters.")]
        public string Reason { get; set; } = string.Empty;

        // Pending, Approved, Completed, or Cancelled (see AppointmentStatus)
        [Required]
        public string Status { get; set; } = AppointmentStatus.Pending;

        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public ConsultationRecord? ConsultationRecord { get; set; }
    }
}
