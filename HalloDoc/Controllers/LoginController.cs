using Azure.Core;
using HalloDoc.Data;
using HalloDoc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;

namespace HalloDoc.Controllers
{
    public class LoginController : Controller
    {
       /* private readonly ILogger<LoginController> _logger;*/

        //public LoginController(ILogger<LoginController> logger)
        //{
        //    _logger = logger;
        //}
        private readonly ApplicationDbContext _context;
        public LoginController(ApplicationDbContext context)
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
                Debug.WriteLine(model.UserName);
                var user = _context.AspNetUsers.FirstOrDefault(u => u.UserName == model.UserName);
                if (user != null)
                {
                    if (model.PasswordHash == user.PasswordHash)
                    {
                        var user2 = _context.Users.Where(x => x.Email == model.UserName);
                        User users = user2.ToList().First();
                        HttpContext.Session.SetInt32("id", users.UserId);
                        HttpContext.Session.SetString("Name", users.FirstName);
                        HttpContext.Session.SetString("IsLoggedIn", "true");
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
            var data = (
        from req in _context.Requests
        join file in _context.RequestWiseFiles on req.RequestId equals file.RequestId into files
        from file in files.DefaultIfEmpty()
        group file by new { req.RequestId, req.CreatedDate, req.Status } into fileGroup
        select new TableContent
        {
            RequestId = fileGroup.Key.RequestId,
            CreatedDate = fileGroup.Key.CreatedDate,
            Status = fileGroup.Key.Status,
            Count = fileGroup.Count()
        }).ToList();

            var viewModel = new DashboardViewModel
            {
                requests = data
            };

            return View(viewModel);
        }

        public IActionResult PatientDashboardViewDocuments()
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