using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicSystem.Models;

namespace ClinicSystem.Controllers
{
    // Pages that everyone can open, even without logging in.
    [AllowAnonymous]
    public class HomeController : Controller
    {
        // Home page
        public IActionResult Index()
        {
            return View();
        }

        // Privacy page (comes with the standard MVC template; the front-end team may delete it)
        public IActionResult Privacy()
        {
            return View();
        }

        // About page (ABT-1): the application description
        public IActionResult About()
        {
            return View();
        }

        // Contact page (ABT-2): the authors and their contact details
        public IActionResult Contact()
        {
            return View();
        }

        // Shows an error page. (ErrorViewModel comes with the standard MVC project template.)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
