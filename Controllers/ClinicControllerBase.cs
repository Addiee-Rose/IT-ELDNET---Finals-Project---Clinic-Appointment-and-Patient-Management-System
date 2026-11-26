using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ClinicSystem.Data;
using ClinicSystem.Models;

namespace ClinicSystem.Controllers
{
    // A small "parent" class that our controllers inherit from, so we do not repeat the
    // same code in every controller. It holds the database context (_context) and
    // helpers that tell us WHO is logged in.
    public abstract class ClinicControllerBase : Controller
    {
        protected readonly ClinicDbContext _context;

        protected ClinicControllerBase(ClinicDbContext context)
        {
            _context = context;
        }

        // The UserId of the logged-in user (it was stored in the login cookie).
        protected int CurrentUserId
        {
            get
            {
                string? value = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(value, out int id) ? id : 0;
            }
        }

        // The Patient record of the logged-in user (null if the user is not a patient).
        protected Patient? CurrentPatient
        {
            get { return _context.Patients.FirstOrDefault(p => p.UserId == CurrentUserId); }
        }

        // The Doctor record of the logged-in user (null if the user is not a doctor).
        protected Doctor? CurrentDoctor
        {
            get { return _context.Doctors.FirstOrDefault(d => d.UserId == CurrentUserId); }
        }
    }
}
