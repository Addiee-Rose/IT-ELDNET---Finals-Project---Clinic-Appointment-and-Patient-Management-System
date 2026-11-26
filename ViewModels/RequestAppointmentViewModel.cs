using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicSystem.ViewModels
{
    // Used by the patient's "Request Appointment" form.
    public class RequestAppointmentViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a doctor.")]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime AppointmentDate { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Time")]
        public TimeSpan AppointmentTime { get; set; }

        [Required(ErrorMessage = "Please give the reason for your visit.")]
        [StringLength(300, ErrorMessage = "Reason can have at most 300 characters.")]
        public string Reason { get; set; } = string.Empty;

        // The doctor dropdown list. It is filled by the controller, not typed by the user.
        [ValidateNever]
        public List<SelectListItem> Doctors { get; set; } = new List<SelectListItem>();
    }
}
