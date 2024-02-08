using HalloDoc.Data;
using HalloDoc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HalloDoc.Controllers
{
    public class PatientRequestController : Controller
    {
        //private readonly ILogger<PatientRequestController> _logger;
        private readonly ApplicationDbContext _context;
        public PatientRequestController(ApplicationDbContext context)
        {
            /* _logger = logger;*/
            _context = context;
        }

        /*public PatientRequestController(ILogger<PatientRequestController> logger)
        {
            _logger = logger;
        }*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePatientRequest(PatientRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            User user = new User();
            Request request = new Request();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();

            aspNetUser.UserName = model.Email;
            aspNetUser.Email = model.Email;
            aspNetUser.PhoneNumber = model.PhoneNumber;
            aspNetUser.CreatedDate = DateTime.Now;
            aspNetUser.PasswordHash = model.Password;
            _context.AspNetUsers.Add(aspNetUser);
            await _context.SaveChangesAsync();

            user.AspNetUserId = aspNetUser.Id;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Mobile = model.PhoneNumber;
            user.Street = model.Street;
            user.City = model.City;
            user.State = model.State;
            user.ZipCode = model.Zipcode;
            user.IntDate = model.DOB.Day;
            user.StrMonth = model.DOB.Month.ToString();
            user.IntYear = model.DOB.Year;
            user.CreatedBy = aspNetUser.Id;
            user.CreatedDate = DateTime.Now;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            request.RequestTypeId = 4;
            request.UserId = user.UserId;
            request.FirstName = model.FirstName;
            request.LastName = model.LastName;
            request.Email = model.Email;
            request.PhoneNumber = model.PhoneNumber;
            request.Status = 1;
            request.CreatedDate = DateTime.Now;
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            requestClient.RequestId = request.RequestId;
            requestClient.FirstName = model.FirstName;
            requestClient.LastName = model.LastName;
            requestClient.PhoneNumber = model.PhoneNumber;
            requestClient.Location = model.City;
            requestClient.Address = model.Street;
            requestClient.RegionId = 2;
            requestClient.Notes = model.Symptoms;
            requestClient.Email = model.Email;
            requestClient.IntDate = model.DOB.Day;
            requestClient.StrMonth = model.DOB.Month.ToString();
            requestClient.IntYear = model.DOB.Year;
            requestClient.Street = model.Street;
            requestClient.City = model.City;
            requestClient.State = model.State;
            requestClient.ZipCode = model.Zipcode;
            _context.RequestClients.Add(requestClient);
            await _context.SaveChangesAsync();

            requestWiseFile.RequestId = request.RequestId;
            requestWiseFile.FileName = model.File;
            requestWiseFile.CreatedDate = DateTime.Now;
            _context.RequestWiseFiles.Add(requestWiseFile);
            await _context.SaveChangesAsync();

            requestStatusLog.RequestId = request.RequestId;
            requestStatusLog.Status = 1;
            requestStatusLog.Notes = model.Symptoms;
            requestStatusLog.CreatedDate = DateTime.Now;
            _context.RequestStatusLogs.Add(requestStatusLog);
            await _context.SaveChangesAsync();
            return View();
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