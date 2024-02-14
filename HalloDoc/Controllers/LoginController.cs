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
            var userId = HttpContext.Session.GetInt32("id");

            var data = (
        from req in _context.Requests
        join file in _context.RequestWiseFiles on req.RequestId equals file.RequestId into files
        from file in files.DefaultIfEmpty()
        where req.UserId == userId
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
                requests = data,
                Username = _context.Users.FirstOrDefault(t => t.UserId == userId).FirstName
            };

            return View(viewModel);
        }

        public IActionResult PatientDashboardViewDocuments(int requestid)
        {
            var user_id = HttpContext.Session.GetInt32("id");

            // include() method creates object of RequestClient table where Request.RequestClientId = RequestClient.RequestClientId and this object is added to the Request table (kind of join operation). only those records are present in the variable 'request' whose requestId matches with the id passed in argument
            var request = _context.Requests.Include(r => r.RequestClient).FirstOrDefault(u => u.RequestId == requestid);
            
            // Similarly, we include the records of Admin and Physician where Admin.AdminId = RequestWiseFiles.AdminId and Physician.PhysicianId = Admin.AdminId and only those records are present in the variable 'documents' whose requestId matches with the id passed in argument
            var documents = _context.RequestWiseFiles.Include(u => u.Admin).Include(u => u.Physician).Where(u => u.RequestId == requestid).ToList();
            
            var user = _context.Users.FirstOrDefault(u => u.UserId == user_id);


            ViewDocumentModel viewDocumentModal = new ViewDocumentModel()
            {
                patient_name = string.Concat(request.RequestClient.FirstName, ' ', request.RequestClient.LastName),
                name = string.Concat(user.FirstName, ' ', user.LastName),
                confirmation_number = request.ConfirmationNumber,
                requestWiseFiles = documents,
                uploader_name = string.Concat(request.FirstName, ' ', request.LastName),
                Username = _context.Users.FirstOrDefault(t => t.UserId == user_id).FirstName
            };
            return View(viewDocumentModal);
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