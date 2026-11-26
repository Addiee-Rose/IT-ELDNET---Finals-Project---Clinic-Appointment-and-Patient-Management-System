using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be 2 to 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialization is required.")]
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        // Days the doctor works, written like "Mon, Wed, Fri" (full day names also work).
        [Required(ErrorMessage = "Available days are required (example: Mon, Wed, Fri).")]
        [StringLength(100)]
        [Display(Name = "Available Days")]
        public string AvailableDays { get; set; } = string.Empty;

        [DataType(DataType.Time)]
        [Column(TypeName = "time")]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [DataType(DataType.Time)]
        [Column(TypeName = "time")]
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        public UserAccount? User { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }

        // ---- Business logic lives in the Model (as taught in class) ----

        // Turns "Mon, Wed, Fri" into a list of days. Returns null if the text is not valid.
        public static List<DayOfWeek>? ParseDays(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var days = new List<DayOfWeek>();
            string[] parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (string part in parts)
            {
                DayOfWeek? match = null;
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    // "Mon" and "Monday" both match Monday (at least 3 letters needed)
                    if (part.Length >= 3 && day.ToString().StartsWith(part, StringComparison.OrdinalIgnoreCase))
                    {
                        match = day;
                    }
                }
                if (match == null) return null;   // unknown day name
                days.Add(match.Value);
            }

            return days.Count > 0 ? days : null;
        }

        // Is this doctor working on the given date? (used for APP-2 / BR-2)
        public bool IsAvailableOn(DateTime date)
        {
            List<DayOfWeek>? days = ParseDays(AvailableDays);
            return days != null && days.Contains(date.DayOfWeek);
        }
    }
}
