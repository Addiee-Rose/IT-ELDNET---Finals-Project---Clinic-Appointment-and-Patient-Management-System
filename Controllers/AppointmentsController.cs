using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClinicSystem.Data;
using ClinicSystem.Models;
using ClinicSystem.ViewModels;

namespace ClinicSystem.Controllers
{
    // Appointment Management and Tracking (APP-1 to APP-7, TRK-1 to TRK-4).
    //   Patient: requests appointments and sees their own.
    //   Staff:   sees all, approves, reschedules, and cancels.
    //   Doctor:  sees their own.
    [Authorize]
    public class AppointmentsController : ClinicControllerBase
    {
        public AppointmentsController(ClinicDbContext context) : base(context)
        {
        }

        // READ (list) + TRK-3 filter by status:  /Appointments  or  /Appointments?status=Pending
        public IActionResult Index(string? status)
        {
            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor);

            // Each role only sees what it is allowed to see (APP-7)
            if (User.IsInRole(Roles.Patient))
            {
                int patientId = CurrentPatient?.PatientId ?? 0;
                query = query.Where(a => a.PatientId == patientId);
            }
            else if (User.IsInRole(Roles.Doctor))
            {
                int doctorId = CurrentDoctor?.DoctorId ?? 0;
                query = query.Where(a => a.DoctorId == doctorId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            ViewData["StatusFilter"] = status;

            List<Appointment> appointments = query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            return View(appointments);
        }

        // READ (one): /Appointments/Details/5
        public IActionResult Details(int id)
        {
            Appointment? appointment = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.ConsultationRecord)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();
            if (!CanView(appointment)) return Forbid();

            return View(appointment);
        }

        // ---------------- PATIENT: request an appointment (APP-1) ----------------

        [HttpGet]
        [Authorize(Roles = Roles.Patient)]
        public IActionResult Create()
        {
            var vm = new RequestAppointmentViewModel { AppointmentDate = DateTime.Today.AddDays(1) };
            FillDoctorList(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Patient)]
        public IActionResult Create(RequestAppointmentViewModel vm)
        {
            Patient? patient = CurrentPatient;
            if (patient == null) return Forbid();

            Doctor? doctor = _context.Doctors.Find(vm.DoctorId);
            if (doctor == null)
            {
                ModelState.AddModelError("DoctorId", "Please choose a doctor.");
            }
            else
            {
                // APP-2: date not in the past, and inside the doctor's available days and hours
                string? problem = CheckSchedule(doctor, vm.AppointmentDate, vm.AppointmentTime);
                if (problem != null) ModelState.AddModelError(string.Empty, problem);
            }

            if (!ModelState.IsValid)
            {
                FillDoctorList(vm);
                return View(vm);
            }

            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                DoctorId = vm.DoctorId,
                AppointmentDate = vm.AppointmentDate.Date,
                AppointmentTime = vm.AppointmentTime,
                Reason = vm.Reason,
                Status = AppointmentStatus.Pending    // a new request is always Pending (TRK-2)
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            TempData["Success"] = "Your appointment request was sent. Please wait for the clinic to approve it.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- STAFF: approve, reschedule, cancel (APP-3 to APP-5) ----------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Approve(int id)
        {
            Appointment? appointment = _context.Appointments.Find(id);
            if (appointment == null) return NotFound();

            // BR-5: only a Pending appointment can be approved
            if (appointment.Status != AppointmentStatus.Pending)
            {
                TempData["Error"] = "Only pending appointments can be approved.";
                return RedirectToAction(nameof(Index));
            }

            // APP-6 / BR-3: the doctor must not already have an approved appointment at that time
            if (HasApprovedConflict(appointment.DoctorId, appointment.AppointmentDate, appointment.AppointmentTime, appointment.AppointmentId))
            {
                TempData["Error"] = "The doctor already has an approved appointment at that date and time. Reschedule this one instead.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Status = AppointmentStatus.Approved;
            _context.SaveChanges();

            TempData["Success"] = "Appointment approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Reschedule(int id)
        {
            Appointment? appointment = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();

            // BR-4: completed or cancelled appointments cannot be rescheduled
            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Approved)
            {
                TempData["Error"] = "Only pending or approved appointments can be rescheduled.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new RescheduleViewModel
            {
                AppointmentId = appointment.AppointmentId,
                PatientName = appointment.Patient?.FullName,
                DoctorName = appointment.Doctor?.FullName,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Reschedule(RescheduleViewModel vm)
        {
            Appointment? appointment = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.AppointmentId == vm.AppointmentId);

            if (appointment == null || appointment.Doctor == null) return NotFound();

            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Approved)
            {
                TempData["Error"] = "Only pending or approved appointments can be rescheduled.";
                return RedirectToAction(nameof(Index));
            }

            string? problem = CheckSchedule(appointment.Doctor, vm.AppointmentDate, vm.AppointmentTime);

            if (problem == null && appointment.Status == AppointmentStatus.Approved &&
                HasApprovedConflict(appointment.DoctorId, vm.AppointmentDate, vm.AppointmentTime, appointment.AppointmentId))
            {
                problem = "The doctor already has an approved appointment at that date and time.";
            }

            if (problem != null) ModelState.AddModelError(string.Empty, problem);

            if (!ModelState.IsValid)
            {
                vm.PatientName = appointment.Patient?.FullName;
                vm.DoctorName = appointment.Doctor.FullName;
                return View(vm);
            }

            appointment.AppointmentDate = vm.AppointmentDate.Date;
            appointment.AppointmentTime = vm.AppointmentTime;
            _context.SaveChanges();

            TempData["Success"] = "Appointment rescheduled.";
            return RedirectToAction(nameof(Index));
        }

        // Cancel: first a confirmation page...
        [HttpGet]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Cancel(int id)
        {
            Appointment? appointment = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // ...then the real cancellation
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult CancelConfirmed(int id)
        {
            Appointment? appointment = _context.Appointments.Find(id);
            if (appointment == null) return NotFound();

            // BR-4: only Pending or Approved appointments can be cancelled
            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Approved)
            {
                TempData["Error"] = "Only pending or approved appointments can be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Status = AppointmentStatus.Cancelled;
            _context.SaveChanges();

            TempData["Success"] = "Appointment cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- helper methods (private) ----------------

        // Fills the doctor dropdown with "Name - Specialization (days)" (DOC-4)
        private void FillDoctorList(RequestAppointmentViewModel vm)
        {
            List<Doctor> doctors = _context.Doctors.OrderBy(d => d.FullName).ToList();

            vm.Doctors = doctors.Select(d => new SelectListItem
            {
                Value = d.DoctorId.ToString(),
                Text = d.FullName + " - " + d.Specialization + " (" + d.AvailableDays + ")"
            }).ToList();
        }

        // Checks the date and time against today and the doctor's schedule. Returns an
        // error message, or null when everything is fine. (APP-2, BR-2)
        private static string? CheckSchedule(Doctor doctor, DateTime date, TimeSpan time)
        {
            if (date.Date < DateTime.Today)
            {
                return "The appointment date cannot be in the past.";
            }

            if (date.Date == DateTime.Today && time <= DateTime.Now.TimeOfDay)
            {
                return "That time has already passed today. Please choose a later time or another day.";
            }

            if (!doctor.IsAvailableOn(date))
            {
                return "The doctor is not available on " + date.ToString("dddd") + ". Available days: " + doctor.AvailableDays + ".";
            }

            if (time < doctor.StartTime || time >= doctor.EndTime)
            {
                return "Please choose a time between " + doctor.StartTime.ToString(@"hh\:mm") + " and " + doctor.EndTime.ToString(@"hh\:mm") + ".";
            }

            return null;
        }

        // Does this doctor already have an APPROVED appointment at the same date and time? (BR-3)
        private bool HasApprovedConflict(int doctorId, DateTime date, TimeSpan time, int ignoreAppointmentId)
        {
            DateTime day = date.Date;

            return _context.Appointments.Any(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate == day &&
                a.AppointmentTime == time &&
                a.Status == AppointmentStatus.Approved &&
                a.AppointmentId != ignoreAppointmentId);
        }

        // Who may open this appointment? Staff: any. Patient/Doctor: only their own.
        private bool CanView(Appointment appointment)
        {
            if (User.IsInRole(Roles.Staff)) return true;

            if (User.IsInRole(Roles.Patient))
            {
                Patient? patient = CurrentPatient;
                return patient != null && patient.PatientId == appointment.PatientId;
            }

            if (User.IsInRole(Roles.Doctor))
            {
                Doctor? doctor = CurrentDoctor;
                return doctor != null && doctor.DoctorId == appointment.DoctorId;
            }

            return false;
        }
    }
}
