using HalloDoc.Data;
using HalloDoc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

using Region = HalloDoc.Models.Region;
using System.Drawing;

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
        public async Task<IActionResult> CreatePatientRequest(PatientRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            User user = new User();
            Request request = new Request();
            Region region2 = new Region();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();

            var temp = model.State.ToLower().Trim();
            var region = _context.Regions.FirstOrDefault(u => u.Name.ToLower().Trim().Equals(temp));


            if (region == null)
            {
                ModelState.AddModelError("State", "Currently we are not serving in this region");
                return View(model);
            }
            var blockedUser = _context.BlockRequests.FirstOrDefault(u => u.Email == model.Email);
            if (blockedUser != null)
            {
                ModelState.AddModelError("Email", "This patient is blocked.");
                return View(model);
            }

            if (model.File != null && model.File.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", model.ImageContent.FileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await model.ImageContent.CopyToAsync(stream);
                }
            }

            var existingUser = _context.AspNetUsers.SingleOrDefault(u => u.Email == model.Email);
            bool userExists = true;
            if(ModelState.IsValid)
            {
                if (existingUser == null)
                {
                    userExists = false;
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
                }

                requestClient.FirstName = model.FirstName;
                requestClient.LastName = model.LastName;
                requestClient.PhoneNumber = model.PhoneNumber;
                requestClient.Location = model.City;
                requestClient.Address = model.Street;
                requestClient.RegionId = 1;
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

                int requests = _context.Requests.Where(u => u.CreatedDate == DateTime.Now.Date).Count();
                string ConfirmationNumber = string.Concat(region2.Abbreviation, model.FirstName.Substring(0, 2).ToUpper(), model.LastName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));

                request.RequestTypeId = 1;
                if (!userExists)
                {
                    request.UserId = user.UserId;
                }
                request.FirstName = model.FirstName;
                request.LastName = model.LastName;
                request.Email = model.Email;
                request.PhoneNumber = model.PhoneNumber;
                request.ConfirmationNumber = ConfirmationNumber;
                request.Status = 1;
                request.CreatedDate = DateTime.Now;
                request.RequestClientId = requestClient.RequestClientId;
                _context.Requests.Add(request);
                await _context.SaveChangesAsync();

                if (model.ImageContent != null && model.ImageContent.Length > 0)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", model.ImageContent.FileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await model.ImageContent.CopyToAsync(stream);
                    }
                    var filePath = "/uploads/" + model.ImageContent.FileName;

                    requestWiseFile.RequestId = request.RequestId;
                    requestWiseFile.FileName = filePath;
                    requestWiseFile.CreatedDate = request.CreatedDate;
                    _context.RequestWiseFiles.Add(requestWiseFile);
                    await _context.SaveChangesAsync();
                }

                requestStatusLog.RequestId = request.RequestId;
                requestStatusLog.Status = 1;
                requestStatusLog.Notes = model.Symptoms;
                requestStatusLog.CreatedDate = DateTime.Now;
                _context.RequestStatusLogs.Add(requestStatusLog);
                await _context.SaveChangesAsync();
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

            var region = _context.Regions.FirstOrDefault(u => u.Name == model.State.Trim().ToLower().Replace(" ", ""));
            if (region == null)
            {
                ModelState.AddModelError("State", "Currently we are not serving in this region");
                return View(model);
            }

            if (model.File != null && model.File.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", model.ImageContent.FileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await model.ImageContent.CopyToAsync(stream);
                }
            }

            var existingUser = _context.AspNetUsers.SingleOrDefault(u => u.Email == model.Email);
            bool userExists = true;

            if(ModelState.IsValid)
            {
                if (existingUser == null)
                {
                    userExists = false;
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
                }

                requestClient.FirstName = model.FirstName;
                requestClient.LastName = model.LastName;
                requestClient.PhoneNumber = model.PhoneNumber;
                requestClient.Location = model.City;
                requestClient.Address = model.Street;
                requestClient.RegionId = 1;
                if (model.Symptoms != null)
                {
                    requestClient.Notes = model.Symptoms;
                }
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

                int requests = _context.Requests.Where(u => u.CreatedDate == DateTime.Now.Date).Count();
                string ConfirmationNumber = string.Concat(region2.Abbreviation, model.FirstName.Substring(0, 2).ToUpper(), model.LastName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));

                request.RequestTypeId = 2;
                if (!userExists)
                {
                    request.UserId = user.UserId;
                }
                request.FirstName = model.FamilyFirstName;
                request.LastName = model.FamilyLastName;
                request.Email = model.FamilyEmail;
                request.ConfirmationNumber = ConfirmationNumber;
                request.PhoneNumber = model.FamilyPhoneNumber;
                request.Status = 1;
                request.CreatedDate = DateTime.Now;
                request.RequestClientId = requestClient.RequestClientId;
                _context.Requests.Add(request);
                await _context.SaveChangesAsync();

                if (model.File != null)
                {
                    requestWiseFile.RequestId = request.RequestId;
                    requestWiseFile.FileName = model.File;
                    requestWiseFile.CreatedDate = DateTime.Now;
                    _context.RequestWiseFiles.Add(requestWiseFile);
                    await _context.SaveChangesAsync();
                }

                requestStatusLog.RequestId = request.RequestId;
                requestStatusLog.Status = 1;
                requestStatusLog.Notes = model.Symptoms;
                requestStatusLog.CreatedDate = DateTime.Now;
                _context.RequestStatusLogs.Add(requestStatusLog);
                await _context.SaveChangesAsync();
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

            var existingUser = _context.AspNetUsers.SingleOrDefault(u => u.Email == model.Email);
            bool userExists = true;

            if(ModelState.IsValid)
            {
                if (existingUser == null)
                {
                    userExists = false;
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
                }
                requestClient.FirstName = model.FirstName;
                requestClient.LastName = model.LastName;
                requestClient.PhoneNumber = model.PhoneNumber;
                requestClient.Location = model.City;
                requestClient.Address = model.Street;
                requestClient.RegionId = 1;
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

                int requests = _context.Requests.Where(u => u.CreatedDate == DateTime.Now.Date).Count();
                string ConfirmationNumber = string.Concat(region.Abbreviation, model.FirstName.Substring(0, 2).ToUpper(), model.LastName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));

                request.RequestTypeId = 3;
                if (!userExists)
                {
                    request.UserId = user.UserId;
                }
                request.FirstName = model.ConciergeFirstName;
                request.LastName = model.ConciergeLastName;
                request.Email = model.ConciergeEmail;
                request.ConfirmationNumber = ConfirmationNumber;
                request.PhoneNumber = model.ConciergePhoneNumber;
                request.Status = 1;
                request.CreatedDate = DateTime.Now;
                request.RequestClientId = requestClient.RequestClientId;
                _context.Requests.Add(request);
                await _context.SaveChangesAsync();

                if (model.File != null)
                {
                    requestWiseFile.RequestId = request.RequestId;
                    requestWiseFile.FileName = model.File;
                    requestWiseFile.CreatedDate = DateTime.Now;
                    _context.RequestWiseFiles.Add(requestWiseFile);
                    await _context.SaveChangesAsync();
                }

                requestStatusLog.RequestId = request.RequestId;
                requestStatusLog.Status = 1;
                requestStatusLog.Notes = model.Symptoms;
                requestStatusLog.CreatedDate = DateTime.Now;
                _context.RequestStatusLogs.Add(requestStatusLog);
                await _context.SaveChangesAsync();

                concierge.ConciergeName = model.ConciergeFirstName;
                concierge.Address = model.ConciergePropertyName;
                concierge.Street = model.ConciergeStreet;
                concierge.City = model.ConciergeCity;
                concierge.State = model.ConciergeState;
                concierge.ZipCode = model.ConciergeZipcode;
                concierge.CreatedDate = DateTime.Now;
                _context.Concierges.Add(concierge);
                await _context.SaveChangesAsync();

                requestConcierge.RequestId = request.RequestId;
                requestConcierge.ConciergeId = concierge.ConciergeId;
                _context.RequestConcierges.Add(requestConcierge);
                await _context.SaveChangesAsync();

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


            var existingUser = _context.AspNetUsers.SingleOrDefault(u => u.Email == model.Email);
            bool userExists = true;

            var region = _context.Regions.FirstOrDefault(u => u.Name == model.State.Trim().ToLower().Replace(" ", ""));
            if (region == null)
            {
                ModelState.AddModelError("State", "Currently we are not serving in this region");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                if (existingUser == null)
                {
                    userExists = false;
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
                }

                requestClient.FirstName = model.FirstName;
                requestClient.LastName = model.LastName;
                requestClient.PhoneNumber = model.PhoneNumber;
                requestClient.Location = model.City;
                requestClient.Address = model.Street;
                requestClient.RegionId = 1;
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

                int requests = _context.Requests.Where(u => u.CreatedDate == DateTime.Now.Date).Count();
                string ConfirmationNumber = string.Concat(region2.Abbreviation, model.FirstName.Substring(0, 2).ToUpper(), model.LastName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));

                request.RequestTypeId = 4;
                if (!userExists)
                {
                    request.UserId = user.UserId;
                }
                request.FirstName = model.BusinessFirstName;
                request.LastName = model.BusinessLastName;
                request.Email = model.BusinessEmail;
                request.ConfirmationNumber = ConfirmationNumber;
                request.PhoneNumber = model.BusinessPhoneNumber;
                request.Status = 1;
                request.CreatedDate = DateTime.Now;
                request.RequestClientId = requestClient.RequestClientId;
                _context.Requests.Add(request);
                await _context.SaveChangesAsync();

                if (model.File != null)
                {
                    requestWiseFile.RequestId = request.RequestId;
                    requestWiseFile.FileName = model.File;
                    requestWiseFile.CreatedDate = DateTime.Now;
                    _context.RequestWiseFiles.Add(requestWiseFile);
                    await _context.SaveChangesAsync();
                }

                requestStatusLog.RequestId = request.RequestId;
                requestStatusLog.Status = 1;
                requestStatusLog.Notes = model.Symptoms;
                requestStatusLog.CreatedDate = DateTime.Now;
                _context.RequestStatusLogs.Add(requestStatusLog);
                await _context.SaveChangesAsync();

                business.Name = model.BusinessFirstName + " " + model.BusinessLastName;
                business.Address1 = model.BusinessPropertyName;
                business.Address2 = model.BusinessPropertyName;
                business.City = model.BusinessPropertyName;
                business.ZipCode = "361002";
                //business.PhoneNumber = model.BusinessPhoneNumber;
                business.CreatedDate = DateTime.Now;
                business.RegionId = 1;
                _context.Businesses.Add(business);
                await _context.SaveChangesAsync();

                requestBusiness.RequestId = request.RequestId;
                requestBusiness.BusinessId = business.BusinessId;
                _context.RequestBusinesses.Add(requestBusiness);
                await _context.SaveChangesAsync();

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