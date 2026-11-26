using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Models
{
    public class ConsultationRecord
    {
        [Key]
        public int RecordId { get; set; }

        public int AppointmentId { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Consultation Date")]
        public DateTime ConsultationDate { get; set; }

        [Required(ErrorMessage = "Diagnosis is required.")]
        [StringLength(500, ErrorMessage = "Diagnosis can have at most 500 characters.")]
        public string Diagnosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Treatment notes are required.")]
        [StringLength(1000, ErrorMessage = "Treatment notes can have at most 1000 characters.")]
        [Display(Name = "Treatment Notes")]
        public string TreatmentNotes { get; set; } = string.Empty;

        public Appointment? Appointment { get; set; }
    }
}
