using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicSystem.Data;
using ClinicSystem.Models;
using ClinicSystem.ViewModels;

namespace ClinicSystem.Controllers
{
    // Doctor Management (DOC-1 to DOC-4): CRUD for staff, "My Schedule" for doctors.
    [Authorize]
    public class DoctorsController : ClinicControllerBase
    {
        private readonly IPasswordHasher<UserAccount> _hasher;

        public DoctorsController(ClinicDbContext context, IPasswordHasher<UserAccount> hasher) : base(context)
        {
            _hasher = hasher;
        }

        // READ (list)
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Index()
        {
            return View(_context.Doctors.OrderBy(d => d.FullName).ToList());
        }

        // READ (one)
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Details(int id)
        {
            Doctor? doctor = _context.Doctors.Include(d => d.User).FirstOrDefault(d => d.DoctorId == id);
            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // DOC-3: a doctor sees their own profile, schedule, and appointments
        [Authorize(Roles = Roles.Doctor)]
        public IActionResult MySchedule()
        {
            Doctor? current = CurrentDoctor;
            if (current == null) return NotFound();

            Doctor? doctor = _context.Doctors
                .Include("Appointments.Patient")
                .FirstOrDefault(d => d.DoctorId == current.DoctorId);

            return View(doctor);
        }

        // CREATE (show the empty form)
        [HttpGet]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Create()
        {
            return View(new DoctorCreateViewModel
            {
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(17, 0, 0)
            });
        }

        // CREATE (save): creates the login account AND the doctor profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Create(DoctorCreateViewModel vm)
        {
            CheckSchedule(vm.AvailableDays, vm.StartTime, vm.EndTime);

            if (_context.Users.Any(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "This email is already used by another account.");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var account = new UserAccount { Email = vm.Email, Role = Roles.Doctor };
            account.PasswordHash = _hasher.HashPassword(account, vm.Password);

            var doctor = new Doctor
            {
                User = account,
                FullName = vm.FullName,
                Specialization = vm.Specialization,
                AvailableDays = vm.AvailableDays,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime
            };

            _context.Doctors.Add(doctor);
            _context.SaveChanges();

            TempData["Success"] = "Doctor added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // UPDATE (show the form)
        [HttpGet]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Edit(int id)
        {
            Doctor? doctor = _context.Doctors.Find(id);
            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // UPDATE (save)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Edit(int id, Doctor doctor)
        {
            if (id != doctor.DoctorId) return NotFound();

            CheckSchedule(doctor.AvailableDays, doctor.StartTime, doctor.EndTime);

            if (!ModelState.IsValid)
            {
                return View(doctor);
            }

            Doctor? existing = _context.Doctors.Find(id);
            if (existing == null) return NotFound();

            existing.FullName = doctor.FullName;
            existing.Specialization = doctor.Specialization;
            existing.AvailableDays = doctor.AvailableDays;
            existing.StartTime = doctor.StartTime;
            existing.EndTime = doctor.EndTime;

            _context.SaveChanges();

            TempData["Success"] = "Doctor updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE (confirmation page)
        [HttpGet]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult Delete(int id)
        {
            Doctor? doctor = _context.Doctors.Find(id);
            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // DELETE (really delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Staff)]
        public IActionResult DeleteConfirmed(int id)
        {
            Doctor? doctor = _context.Doctors.Find(id);
            if (doctor == null) return NotFound();

            // BR-8: a doctor with pending, approved, or completed appointments cannot be deleted
            bool hasActiveAppointments = _context.Appointments
                .Any(a => a.DoctorId == id && a.Status != AppointmentStatus.Cancelled);

            if (hasActiveAppointments)
            {
                TempData["Error"] = "This doctor still has appointments. Cancel them first before deleting the doctor.";
                return RedirectToAction(nameof(Index));
            }

            List<Appointment> oldAppointments = _context.Appointments.Where(a => a.DoctorId == id).ToList();
            _context.Appointments.RemoveRange(oldAppointments);

            UserAccount? account = _context.Users.Find(doctor.UserId);
            _context.Doctors.Remove(doctor);
            if (account != null) _context.Users.Remove(account);

            _context.SaveChanges();

            TempData["Success"] = "Doctor deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Schedule rules (DOC-2): valid day names, and the start time must be before the end time
        private void CheckSchedule(string availableDays, TimeSpan startTime, TimeSpan endTime)
        {
            if (Doctor.ParseDays(availableDays) == null)
            {
                ModelState.AddModelError("AvailableDays", "Use day names like: Mon, Tue, Wed.");
            }

            if (startTime >= endTime)
            {
                ModelState.AddModelError("EndTime", "End time must be later than start time.");
            }
        }
    }
}
