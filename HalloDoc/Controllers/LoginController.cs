using HalloDoc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HalloDoc.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;

        //public LoginController(ILogger<LoginController> logger)
        //{
        //    _logger = logger;
        //}
        private readonly HalloDocDbContext _context;
        public LoginController(HalloDocDbContext context)
        {
            /* _logger = logger;*/
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PatientLoginPage(LoginViewModel model)
        {
            

            if (ModelState.IsValid)
            {
                var user = _context.AspNetUsers.FirstOrDefault(u => u.UserName == model.UserName);
                if (user != null)
                {
                    if (model.Password == user.PasswordHash)
                    {
                        return RedirectToAction("PatientDashboardAndMedicalHistory");
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Incorrect Password");
                    }
                }
                else
                {
                    ModelState.AddModelError("Username", "Incorrect Username");
                }
            }

            // If we reach here, something went wrong, return the same view with validation errors
            return View(model);
        }

        public IActionResult PatientSite()
        {
            return View();
        }

        public IActionResult PatientDashboardAndMedicalHistory()
        {
            return View();
        }

        public IActionResult PatientLoginPage()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult SubmitRequestScreen()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}