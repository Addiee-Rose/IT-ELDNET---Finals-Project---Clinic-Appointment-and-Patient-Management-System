using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicSystem.Data;
using ClinicSystem.Models;
using ClinicSystem.ViewModels;

namespace ClinicSystem.Controllers
{
    // Consultation Records (CON-1 to CON-4). These are sensitive, so every action checks
    // WHO is asking:  Doctor = their own,  Patient = their own (read-only),  Staff = all (read-only).
    [Authorize]
    public class ConsultationRecordsController : ClinicControllerBase
    {
        public ConsultationRecordsController(ClinicDbContext context) : base(context)
        {
        }

        // READ (list): each role sees only the records it is allowed to see (CON-4)
        public IActionResult Index()
        {
            IQueryable<ConsultationRecord> query = _context.ConsultationRecords
                .Include("Appointment.Patient")
                .Include("Appointment.Doctor");

            if (User.IsInRole(Roles.Patient))
            {
                int patientId = CurrentPatient?.PatientId ?? 0;
                query = query.Where(c => c.Appointment!.PatientId == patientId);
            }
            else if (User.IsInRole(Roles.Doctor))
            {
                int doctorId = CurrentDoctor?.DoctorId ?? 0;
                query = query.Where(c => c.Appointment!.DoctorId == doctorId);
            }

            return View(query.OrderByDescending(c => c.ConsultationDate).ToList());
        }

        // READ (one)
        public IActionResult Details(int id)
        {
            ConsultationRecord? record = _context.ConsultationRecords
                .Include("Appointment.Patient")
                .Include("Appointment.Doctor")
                .FirstOrDefault(c => c.RecordId == id);

            if (record == null) return NotFound();
            if (!CanView(record)) return Forbid();

            return View(record);
        }

        // ---------------- DOCTOR: add a record (CON-1, CON-2) ----------------

        // /ConsultationRecords/Create?appointmentId=5
        [HttpGet]
        [Authorize(Roles = Roles.Doctor)]
        public IActionResult Create(int appointmentId)
        {
            Appointment? appointment = _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.AppointmentId == appointmentId);

            string? problem = CheckCanCreate(appointment);
            if (problem != null || appointment == null)
            {
                TempData["Error"] = problem ?? "Appointment not found.";
                return RedirectToAction("Index", "Appointments");
            }

            var vm = new ConsultationFormViewModel
            {
                AppointmentId = appointment.AppointmentId,
                PatientName = appointment.Patient?.FullName,
                ConsultationDate = DateTime.Today
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Doctor)]
        public IActionResult Create(ConsultationFormViewModel vm)
        {
            Appointment? appointment = _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.AppointmentId == vm.AppointmentId);

            string? problem = CheckCanCreate(appointment);
            if (problem != null || appointment == null)
            {
                TempData["Error"] = problem ?? "Appointment not found.";
                return RedirectToAction("Index", "Appointments");
            }

            if (vm.ConsultationDate.Date > DateTime.Today)
            {
                ModelState.AddModelError("ConsultationDate", "Consultation date cannot be in the future.");
            }

            if (!ModelState.IsValid)
            {
                vm.PatientName = appointment.Patient?.FullName;
                return View(vm);
            }

            var record = new ConsultationRecord
            {
                AppointmentId = appointment.AppointmentId,
                ConsultationDate = vm.ConsultationDate.Date,
                Diagnosis = vm.Diagnosis,
                TreatmentNotes = vm.TreatmentNotes
            };

            _context.ConsultationRecords.Add(record);

            // BR-6: saving the record marks the appointment as Completed
            appointment.Status = AppointmentStatus.Completed;

            _context.SaveChanges();

            TempData["Success"] = "Consultation record saved. The appointment is now Completed.";
            return RedirectToAction("Index", "Appointments");
        }

        // ---------------- DOCTOR: edit their own record (CON-3) ----------------

        [HttpGet]
        [Authorize(Roles = Roles.Doctor)]
        public IActionResult Edit(int id)
        {
            ConsultationRecord? record = _context.ConsultationRecords
                .Include("Appointment.Patient")
                .FirstOrDefault(c => c.RecordId == id);

            if (record == null) return NotFound();
            if (!IsTreatingDoctor(record)) return Forbid();

            var vm = new ConsultationFormViewModel
            {
                RecordId = record.RecordId,
                AppointmentId = record.AppointmentId,
                PatientName = record.Appointment?.Patient?.FullName,
                ConsultationDate = record.ConsultationDate,
                Diagnosis = record.Diagnosis,
                TreatmentNotes = record.TreatmentNotes
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Doctor)]
        public IActionResult Edit(ConsultationFormViewModel vm)
        {
            ConsultationRecord? record = _context.ConsultationRecords
                .Include("Appointment.Patient")
                .FirstOrDefault(c => c.RecordId == vm.RecordId);

            if (record == null) return NotFound();
            if (!IsTreatingDoctor(record)) return Forbid();

            if (vm.ConsultationDate.Date > DateTime.Today)
            {
                ModelState.AddModelError("ConsultationDate", "Consultation date cannot be in the future.");
            }

            if (!ModelState.IsValid)
            {
                vm.PatientName = record.Appointment?.Patient?.FullName;
                return View(vm);
            }

            record.ConsultationDate = vm.ConsultationDate.Date;
            record.Diagnosis = vm.Diagnosis;
            record.TreatmentNotes = vm.TreatmentNotes;
            _context.SaveChanges();

            TempData["Success"] = "Consultation record updated.";
            return RedirectToAction(nameof(Details), new { id = record.RecordId });
        }

        // ---------------- helper methods (private) ----------------

        // May a record be created for this appointment? Returns an error message, or null if yes.
        private string? CheckCanCreate(Appointment? appointment)
        {
            Doctor? doctor = CurrentDoctor;

            if (appointment == null || doctor == null || appointment.DoctorId != doctor.DoctorId)
            {
                return "You can only record consultations for your own appointments.";
            }

            if (appointment.Status != AppointmentStatus.Approved)
            {
                return "A consultation can only be recorded for an approved appointment.";
            }

            if (_context.ConsultationRecords.Any(c => c.AppointmentId == appointment.AppointmentId))
            {
                return "This appointment already has a consultation record.";
            }

            return null;
        }

        // Is the logged-in doctor the one who treated this patient?
        private bool IsTreatingDoctor(ConsultationRecord record)
        {
            Doctor? doctor = CurrentDoctor;
            return doctor != null && record.Appointment != null && record.Appointment.DoctorId == doctor.DoctorId;
        }

        // CON-4: who may read a record? The treating doctor, staff, and the patient it belongs to.
        private bool CanView(ConsultationRecord record)
        {
            if (User.IsInRole(Roles.Staff)) return true;
            if (record.Appointment == null) return false;

            if (User.IsInRole(Roles.Doctor))
            {
                return IsTreatingDoctor(record);
            }

            if (User.IsInRole(Roles.Patient))
            {
                Patient? patient = CurrentPatient;
                return patient != null && patient.PatientId == record.Appointment.PatientId;
            }

            return false;
        }
    }
}
