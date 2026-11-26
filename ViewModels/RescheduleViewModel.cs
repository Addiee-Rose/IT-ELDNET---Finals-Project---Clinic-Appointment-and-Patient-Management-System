using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.ViewModels
{
    // Used by the staff's "Reschedule" form.
    public class RescheduleViewModel
    {
        public int AppointmentId { get; set; }

        // For display only (the "?" means they are not required form fields)
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "New Date")]
        public DateTime AppointmentDate { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "New Time")]
        public TimeSpan AppointmentTime { get; set; }
    }
}
