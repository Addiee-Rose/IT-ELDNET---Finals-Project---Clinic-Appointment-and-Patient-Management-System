namespace ClinicSystem.Models
{
    // The three user roles of the system. Using constants avoids typing mistakes
    // such as "Doctr" and lets us write [Authorize(Roles = Roles.Staff)].
    public static class Roles
    {
        public const string Patient = "Patient";
        public const string Doctor = "Doctor";
        public const string Staff = "Staff";
    }

    // The four appointment statuses (TRK-1).
    public static class AppointmentStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }
}
