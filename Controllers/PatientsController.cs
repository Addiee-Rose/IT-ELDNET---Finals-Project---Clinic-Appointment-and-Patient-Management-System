using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ClinicSystem.Data;
using ClinicSystem.Models;
using ClinicSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Controllers
{
    // Patient Management (PAT-1 to PAT-6): CRUD for staff, "My Information" for patients.
    [Authorize]
    public class PatientsController : ClinicControllerBase
    {
        private readonly IPasswordHasher<UserAccount> _hasher;

        public PatientsController(ClinicDbContext context, IPasswordHasher<UserAccount> hasher) : base(context)
        {
            _hasher = hasher;
        }

        // READ (list): /Patients  or  /Patients?search=juan
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Index(string? search)
        {
            IQueryable<Patient> patients = _context.Patients;

            if (!string.IsNullOrWhiteSpace(search))
            {
                patients = patients.Where(p => p.FullName.Contains(search));
            }

            ViewData["Search"] = search;
            return View(patients.OrderBy(p => p.FullName).ToList());
        }

        // READ (one): /Patients/Details/3
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Details(int id)
        {
            Patient? patient = _context.Patients.Include(p => p.User).FirstOrDefault(p => p.PatientId == id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        // PAT-6: a patient views ONLY their own information
        [Authorize(Roles = Roles.Patient)]
        public IActionResult MyInformation()
        {
            Patient? patient = CurrentPatient;
            if (patient == null) return NotFound();

            return View(patient);
        }

        // CREATE (show the empty form)
        [HttpGet]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Create()
        {
            return View(new PatientCreateViewModel { BirthDate = DateTime.Today.AddYears(-18) });
        }

        // CREATE (save): creates the login account AND the patient record
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Create(PatientCreateViewModel vm)
        {
            CheckBirthDate(vm.BirthDate);

            if (_context.Users.Any(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "This email is already used by another account.");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);   // show the form again with the error messages (PAT-2)
            }

            var account = new UserAccount { Email = vm.Email, Role = Roles.Patient };
            account.PasswordHash = _hasher.HashPassword(account, vm.Password);

            var patient = new Patient
            {
                User = account,          // EF Core saves the account together with the patient
                FullName = vm.FullName,
                BirthDate = vm.BirthDate.Date,
                Gender = vm.Gender,
                ContactNumber = vm.ContactNumber,
                Address = vm.Address
            };

            _context.Patients.Add(patient);
            _context.SaveChanges();

            TempData["Success"] = "Patient added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // UPDATE (show the form filled with current data)
        [HttpGet]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Edit(int id)
        {
            Patient? patient = _context.Patients.Find(id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        // UPDATE (save)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Edit(int id, Patient patient)
        {
            if (id != patient.PatientId) return NotFound();

            CheckBirthDate(patient.BirthDate);

            if (!ModelState.IsValid)
            {
                return View(patient);
            }

            Patient? existing = _context.Patients.Find(id);
            if (existing == null) return NotFound();

            // Copy only the fields staff may change (the login account stays untouched)
            existing.FullName = patient.FullName;
            existing.BirthDate = patient.BirthDate.Date;
            existing.Gender = patient.Gender;
            existing.ContactNumber = patient.ContactNumber;
            existing.Address = patient.Address;

            _context.SaveChanges();

            TempData["Success"] = "Patient updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE (ask for confirmation first, PAT-5)
        [HttpGet]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Delete(int id)
        {
            Patient? patient = _context.Patients.Find(id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        // DELETE (really delete). [ActionName("Delete")] lets this method answer to /Patients/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult DeleteConfirmed(int id)
        {
            Patient? patient = _context.Patients.Find(id);
            if (patient == null) return NotFound();

            // BR-8: a patient with pending, approved, or completed appointments cannot be deleted
            bool hasActiveAppointments = _context.Appointments
                .Any(a => a.PatientId == id && a.Status != AppointmentStatus.Cancelled);

            if (hasActiveAppointments)
            {
                TempData["Error"] = "This patient still has appointments. Cancel them first before deleting the patient.";
                return RedirectToAction(nameof(Index));
            }

            // Cancelled appointments no longer matter, so remove them, then the patient, then the login account
            List<Appointment> oldAppointments = _context.Appointments.Where(a => a.PatientId == id).ToList();
            _context.Appointments.RemoveRange(oldAppointments);

            UserAccount? account = _context.Users.Find(patient.UserId);
            _context.Patients.Remove(patient);
            if (account != null) _context.Users.Remove(account);

            _context.SaveChanges();

            TempData["Success"] = "Patient deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Birth date rules (PAT-2)
        private void CheckBirthDate(DateTime birthDate)
        {
            if (birthDate.Date > DateTime.Today)
            {
                ModelState.AddModelError("BirthDate", "Birth date cannot be in the future.");
            }
            else if (birthDate.Year < 1900)
            {
                ModelState.AddModelError("BirthDate", "Please enter a valid birth date.");
            }
        }
    }
}
