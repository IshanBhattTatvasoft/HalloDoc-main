using Azure.Core;
using HalloDoc.Data;
using HalloDoc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Mail;
using System.Net;

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
        private readonly IHttpContextAccessor _sescontext;
        public LoginController(ApplicationDbContext context, IHttpContextAccessor sescontext)
        {
            /* _logger = logger;*/
            _context = context;
            _sescontext = sescontext;
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
                        ModelState.AddModelError("PasswordHash", "Incorrect Password");
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

        public async Task<IActionResult> SendMailForSetUpAccount(LoginViewModel model)
        {
            try
            {

                string senderEmail = "tatva.dotnet.ishanbhatt@outlook.com";
                string senderPassword = "Ishan@1503";

                SmtpClient client = new SmtpClient("smtp.office365.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };
                string resetToken = Guid.NewGuid().ToString();
                string resetLink = $"{Request.Scheme}://{Request.Host}/Login/CreatePassword?token={resetToken}";

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "HalloDoc"),
                    Subject = "Set up your Account",
                    IsBodyHtml = true,
                    Body = $"Please click the following link to reset your password: <a href='{resetLink}'>Click Here</a>"
                };
                AspNetUser user = _context.AspNetUsers.FirstOrDefault(r => r.Email == model.UserName);
                if (user != null)
                {
                    mailMessage.To.Add(model.UserName);
                    _sescontext.HttpContext.Session.SetString("Token", resetToken);
                    _sescontext.HttpContext.Session.SetString("UserEmail", model.UserName);
                    await client.SendMailAsync(mailMessage);
                    return RedirectToAction("PatientLoginPage");
                }
                else
                {
                    ModelState.AddModelError("Email", "Invalid Email");
                    return RedirectToAction("ForgotPassword");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ForgotPassword");
            }
        }


        public IActionResult CreatePassword(string token)
        {

            var useremail = _sescontext.HttpContext.Session.GetString("Token");

            if (useremail == token)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Forgot_Password");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {

            var useremail = _sescontext.HttpContext.Session.GetString("UserEmail");
            AspNetUser user = _context.AspNetUsers.FirstOrDefault(x => x.Email == useremail);
            if (user != null && model.Password == model.ConfirmPassword)
            {
                user.PasswordHash = model.Password;
                _context.SaveChanges();
                return RedirectToAction("PatientLoginPage");
            }
            else
            {
                ModelState.AddModelError("Password", "Password Missmatched");
                return RedirectToAction("Forgot_Password");
            }

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

        public IActionResult RequestForMe()
        {
            var user_id = HttpContext.Session.GetInt32("id");
            var user = _context.Users.FirstOrDefault(u => u.UserId == user_id);
            int day = (int)user.IntDate;
            string month = user.StrMonth;
            int year = (int)user.IntYear;
            int monthNumber = Convert.ToInt32(user.StrMonth);

            string dateString = $"{day:00}/{monthNumber:00}/{year}";
            DateTime dob = DateTime.ParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);


            PatientRequestModel mePatientRequest = new PatientRequestModel()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.Mobile,
                Email = user.Email,
                DOB = DateOnly.FromDateTime(dob),
                Street = user.Street,
                City = user.City,
                State = user.State,
                Zipcode = user.ZipCode,
            };
            return View(mePatientRequest);
        }

        [HttpPost]
        public async Task<IActionResult> MePatientRequest(PatientRequestModel model)
        {
            Models.Request request = new Models.Request();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();

            var user = HttpContext.Session.GetInt32("id");
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

            if (model.ImageContent != null && model.ImageContent.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", model.ImageContent.FileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await model.ImageContent.CopyToAsync(stream);

                }
            }

            requestClient.FirstName = model.FirstName;
            requestClient.LastName = model.LastName;
            requestClient.PhoneNumber = model.PhoneNumber;
            requestClient.Location = model.City;
            requestClient.Address = model.Street;
            requestClient.RegionId = 1;

            if(model.Symptoms != null)
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
            string ConfirmationNumber = string.Concat(region.Abbreviation, model.FirstName.Substring(0, 2).ToUpper(), model.LastName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));
            request.RequestTypeId = 1;

            request.UserId = user;
            request.FirstName = model.FirstName;
            request.LastName = model.LastName;
            request.Email = model.Email;
            request.PhoneNumber = model.PhoneNumber;
            request.Status = 1;
            request.CreatedDate = DateTime.Now;
            request.RequestClientId = requestClient.RequestClientId;
            request.ConfirmationNumber = ConfirmationNumber;
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

            return RedirectToAction("PatientDashboardAndMedicalHistory");
        }

        [HttpPost]
        public async Task<IActionResult> RelativePatientRequest(PatientRequestSomeoneElse model)
        {

            Models.Request request = new Models.Request();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();
            var user = HttpContext.Session.GetInt32("id");
            var region = _context.Regions.FirstOrDefault(u => u.Name == model.State.Trim().ToLower().Replace(" ", ""));
            var user_id = HttpContext.Session.GetInt32("id");
            var users = _context.Users.FirstOrDefault(u => u.UserId == user_id);
            int day = (int)users.IntDate;
            string month = users.StrMonth;
            int year = (int)users.IntYear;
            int monthNumber = DateTime.ParseExact(month, "MMMM", null).Month;
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
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", model.File.FileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await model.File.CopyToAsync(stream)
;
                }
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
            requestClient.ZipCode = model.ZipCode;
            _context.RequestClients.Add(requestClient);
            await _context.SaveChangesAsync();

            int requests = _context.Requests.Where(u => u.CreatedDate == DateTime.Now.Date).Count();
            string ConfirmationNumber = string.Concat(region.Abbreviation, users.FirstName.Substring(0, 2).ToUpper(), users.LastName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));
            request.RequestTypeId = 2;

            request.CreatedUserId = users.UserId;
            request.FirstName = users.FirstName;
            request.LastName = users.LastName;
            request.Email = users.Email;
            request.PhoneNumber = users.Mobile;
            request.Status = 1;
            request.CreatedDate = DateTime.Now;
            request.RequestClientId = requestClient.RequestClientId;
            request.ConfirmationNumber = ConfirmationNumber;
            request.RelationName = model.Relation;
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            if (model.File != null)
            {
                requestWiseFile.RequestId = request.RequestId;
                requestWiseFile.FileName = model.File.FileName;
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
            return RedirectToAction("PatientDashboardAndMedicalHistory");
        }

        public IActionResult RequestForSomeoneElse()
        { return View(); }

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
                requestId = requestid,
                patient_name = string.Concat(request.RequestClient.FirstName, ' ', request.RequestClient.LastName),
                name = string.Concat(user.FirstName, ' ', user.LastName),
                confirmation_number = request.ConfirmationNumber,
                requestWiseFiles = documents,
                uploader_name = string.Concat(request.FirstName, ' ', request.LastName),
                Username = _context.Users.FirstOrDefault(t => t.UserId == user_id).FirstName
            };
            return View(viewDocumentModal);
        }
        [HttpPost]
        public async Task<IActionResult> SetImageContent(ViewDocumentModel model, int requestId)
        {
            var user_id = HttpContext.Session.GetInt32("id");
            var request = _context.Requests.Include(r => r.User).FirstOrDefault(u => u.RequestId == requestId);

            var viewModel = new ViewDocumentModel
            {
                ImageContent = model.ImageContent,
            };
            if (model.ImageContent != null && model.ImageContent.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", model.ImageContent.FileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await model.ImageContent.CopyToAsync(stream);

                }
            }

            if (model.ImageContent != null)
            {
                RequestWiseFile requestWiseFile = new RequestWiseFile
                {

                    FileName = model.ImageContent.FileName,
                    CreatedDate = DateTime.Now,
                    RequestId = request.RequestId
                };
                _context.RequestWiseFiles.Add(requestWiseFile);
            }
            _context.SaveChanges();

            return RedirectToAction("PatientDashboardViewDocuments", new { requestID = model.requestId });
        }

        public IActionResult PatientProfile()
        {
            var user_id = HttpContext.Session.GetInt32("id");
            //var request = _context.Requests.Include(r => r.RequestClient).FirstOrDefault(u => u.RequestId == requestid);
            var user = _context.Users.FirstOrDefault(u => u.UserId == user_id);
            int intYear = (int)user.IntYear;
            int intDate = (int)user.IntDate;
            string month = user.StrMonth;
            DateTime date = new DateTime(intYear, int.Parse(month), intDate);
            PatientProfileView ppv = new PatientProfileView()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                DOB = date,
                PhoneNumber = user.Mobile,
                Email = user.Email,
                Street = user.Street,
                City = user.City,
                State = user.State,
                ZipCode = user.ZipCode,
                Username = user.FirstName
            };
            return View(ppv);
        }

        public IActionResult EditPatientProfile(PatientProfileView model)
        {
            var user_id = HttpContext.Session.GetInt32("id");
            var user = _context.Users.FirstOrDefault(u => u.UserId == user_id);
            

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Mobile = model.PhoneNumber;
            user.Street = model.Street;
            user.City = model.City;
            user.State = model.State;
            user.ZipCode = model.ZipCode;
            user.IntDate = model.DOB.Day;
            user.IntYear = model.DOB.Year;
            user.StrMonth = model.DOB.Month.ToString();

            _context.SaveChanges();
            return RedirectToAction("PatientProfile");
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