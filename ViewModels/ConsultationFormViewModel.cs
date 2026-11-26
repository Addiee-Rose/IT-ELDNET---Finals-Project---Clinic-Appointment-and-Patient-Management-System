using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.ViewModels
{
    // Used by the doctor's "Add / Edit Consultation Record" form.
    public class ConsultationFormViewModel
    {
        public int RecordId { get; set; }          // 0 when adding a new record
        public int AppointmentId { get; set; }

        // For display only
        public string? PatientName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Consultation Date")]
        public DateTime ConsultationDate { get; set; }

        [Required(ErrorMessage = "Diagnosis is required.")]
        [StringLength(500, ErrorMessage = "Diagnosis can have at most 500 characters.")]
        public string Diagnosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Treatment notes are required.")]
        [StringLength(1000, ErrorMessage = "Treatment notes can have at most 1000 characters.")]
        [Display(Name = "Treatment Notes")]
        public string TreatmentNotes { get; set; } = string.Empty;
    }
}
