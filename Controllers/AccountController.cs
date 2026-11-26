using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ClinicSystem.Data;
using ClinicSystem.Models;
using ClinicSystem.ViewModels;

namespace ClinicSystem.Controllers
{
    // Handles Login and Logout (ACC-1 to ACC-5).
    public class AccountController : Controller
    {
        private readonly ClinicDbContext _context;
        private readonly IPasswordHasher<UserAccount> _hasher;

        public AccountController(ClinicDbContext context, IPasswordHasher<UserAccount> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // GET: /Account/Login  -> shows the login form
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Appointments");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Account/Login  -> checks the email and password
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Look for the account with this email
            UserAccount? account = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            // Check the password against the stored hash
            bool passwordIsCorrect = account != null &&
                _hasher.VerifyHashedPassword(account, account.PasswordHash, model.Password) != PasswordVerificationResult.Failed;

            if (account == null || !passwordIsCorrect)
            {
                // Same message for a wrong email or a wrong password, so attackers learn nothing (ACC-2)
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // Login succeeded: store who the user is (a "claim") inside a secure cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.UserId.ToString()),
                new Claim(ClaimTypes.Name, GetDisplayName(account)),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = model.RememberMe });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Appointments");
        }

        // POST: /Account/Logout  -> ends the session (ACC-5)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Shown when a logged-in user opens a page that their role is not allowed to use (ACC-4)
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Finds the person's full name so the page can say "Welcome, Maria Santos"
        private string GetDisplayName(UserAccount account)
        {
            string? name = null;

            if (account.Role == Roles.Patient)
            {
                name = _context.Patients.Where(p => p.UserId == account.UserId).Select(p => p.FullName).FirstOrDefault();
            }
            else if (account.Role == Roles.Doctor)
            {
                name = _context.Doctors.Where(d => d.UserId == account.UserId).Select(d => d.FullName).FirstOrDefault();
            }
            else if (account.Role == Roles.Staff)
            {
                name = _context.StaffMembers.Where(s => s.UserId == account.UserId).Select(s => s.FullName).FirstOrDefault();
            }

            return name ?? account.Email;
        }
    }
}
