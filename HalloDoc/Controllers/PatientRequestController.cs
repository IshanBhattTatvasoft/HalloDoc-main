
using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using HalloDoc.DataLayer.Data;

namespace HalloDoc.Controllers
{
    public class PatientRequestController : Controller
    {
        //private readonly ILogger<PatientRequestController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IPatientRequest _patientRequest;
        private readonly IFamilyRequest _familyRequest;
        private readonly IBusinessRequest _businessRequest;
        private readonly IConciergeRequest _conciergeRequest;
        [ActivatorUtilitiesConstructor]
        public PatientRequestController(ApplicationDbContext context, IPatientRequest patientRequest, IFamilyRequest familyRequest, IBusinessRequest businessRequest, IConciergeRequest conciergeRequest)
        {
            /* _logger = logger;*/
            _context = context;
            _patientRequest = patientRequest;
            _familyRequest = familyRequest;
            _businessRequest = businessRequest;
            _conciergeRequest = conciergeRequest;
        }

        /*public PatientRequestController(ILogger<PatientRequestController> logger)
        {
            _logger = logger;
        }*/

        [HttpPost]
        public async Task<IActionResult> CreatePatientRequest(PatientRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            User user = new User();
            Request request = new Request();
            Region region2 = new Region();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();

            var region = _patientRequest.ValidateRegion(model);


            if (region == null)
            {
                ModelState.AddModelError("State", "Currently we are not serving in this region");
                return View(model);
            }
            var blockedUser = _patientRequest.ValidateBlockRequest(model);
            if (blockedUser != null)
            {
                ModelState.AddModelError("Email", "This patient is blocked.");
                return View(model);
            }

            if (model.ImageContent != null && model.ImageContent.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", model.ImageContent.FileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await model.ImageContent.CopyToAsync(stream);

                }
            }

            var existingUser = _patientRequest.ValidateAspNetUser(model);
            bool userExists = true;
            if(ModelState.IsValid)
            {
                _patientRequest.InsertDataPatientRequest(model);
                return RedirectToAction("PatientSite", "Login");
            }

            else
            {
                return View("CreatePatientRequest");
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateFamilyFriendRequest(FamilyRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            User user = new User();
            Request request = new Request();
            RequestClient requestClient = new RequestClient();
            Region region2 = new Region();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();

            var region = _familyRequest.ValidateRegion(model);
            if (region == null)
            {
                ModelState.AddModelError("State", "Currently we are not serving in this region");
                return View(model);
            }

            if (model.ImageContent != null && model.ImageContent.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", model.ImageContent.FileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await model.ImageContent.CopyToAsync(stream);
                }
            }

            var existingUser = _familyRequest.ValidateAspNetUser(model);
            bool userExists = true;

            if(ModelState.IsValid)
            {
                _familyRequest.InsertDataFamilyRequest(model);
                return RedirectToAction("PatientSite", "Login");

            }

            else
            {
                return View("CreateFamilyFriendRequest");
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateConciergeRequest(ConceirgeRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            User user = new User();
            Request request = new Request();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();
            Concierge concierge = new Concierge();
            RequestConcierge requestConcierge = new RequestConcierge();
            Region region = new Region();

            var existingUser = _conciergeRequest.ValidateAspNetUser(model);
            bool userExists = true;

            if(ModelState.IsValid)
            {
                _conciergeRequest.InsertDataConciergeRequest(model);

                return RedirectToAction("PatientSite", "Login");

            }

            else
            {
                return View("CreateConciergeRequest");
            }

        }

        [HttpPost]
        public async Task <IActionResult> CreateBusinessRequest(BusinessRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            User user = new User();
            Request request = new Request();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();
            Business business = new Business();
            RequestBusiness requestBusiness = new RequestBusiness();
            Region region2 = new Region();


            var existingUser = _businessRequest.ValidateAspNetUser(model);
            bool userExists = true;

            var region = _businessRequest.ValidateRegion(model);
            if (region == null)
            {
                ModelState.AddModelError("State", "Currently we are not serving in this region");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                _businessRequest.InsertDataBusinessRequest(model);
                return RedirectToAction("PatientSite", "Login");

            }

            else
            {
                return View("CreateBusinessRequest");
            }
        }

        

        public IActionResult PatientCheck(string email)
        {
            var existingUser = _context.AspNetUsers.SingleOrDefault(u => u.Email == email);
            bool isValidEmail;
            if (existingUser == null)
            {
                isValidEmail = false;
            }
            else
            {
                isValidEmail = true;
            }
            return Json(new { isValid = isValidEmail });
        }



        public IActionResult CreatePatientRequest()
        {
            return View();
        }

        public IActionResult CreateFamilyFriendRequest()
        {
            return View();
        }

        public IActionResult CreateBusinessRequest()
        {
            return View();
        }

        public IActionResult CreateConciergeRequest()
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