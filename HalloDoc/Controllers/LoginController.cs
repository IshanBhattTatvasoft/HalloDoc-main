using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace HalloDoc.Controllers
{
    public class LoginController : Controller
    {
        /* private readonly ILogger<LoginController> _logger;*/

        //public LoginController(ILogger<LoginController> logger)
        //{
        //    _logger = logger;
        //}
        //private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _sescontext;
        private readonly ILoginPage _loginPage;
        private readonly IEmailSender _emailSender;
        private readonly IPatientDashboard _patientDashboard;
        private readonly IViewDocuments _viewDocuments;
        private readonly IPatientProfile _profile;
        private readonly ICreateRequestForMe _createRequestForMe;
        private readonly ICreateRequestForSomeoneElse _createRequestForSomeoneElse;
        private readonly IPatientRequest _patientRequest;
        private readonly IConfiguration _configuration;
        private readonly IJwtToken _jwtToken;
        public LoginController(IHttpContextAccessor sescontext, ILoginPage loginPage, IEmailSender emailSender, IPatientDashboard patientDashboard, IViewDocuments viewDocuments, IPatientProfile profile, ICreateRequestForMe createRequestForMe, ICreateRequestForSomeoneElse createRequestForSomeoneElse, IPatientRequest patientRequest, IConfiguration configuration, IJwtToken jwtToken)
        {
            /* _logger = logger;*/
            //_context = context;
            _sescontext = sescontext;
            _loginPage = loginPage;
            _emailSender = emailSender;
            _patientDashboard = patientDashboard;
            _viewDocuments = viewDocuments;
            _profile = profile;
            _createRequestForMe = createRequestForMe;
            _createRequestForSomeoneElse = createRequestForSomeoneElse;
            _patientRequest = patientRequest;
            _configuration = configuration;
            _jwtToken = jwtToken;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PatientLoginPage(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {

                //var user = _loginPage.ValidateAspNetUser(model);
                var user = new AuthManager().Login(model.UserName, model.PasswordHash);
                if (user != null)
                {
                    var token = _jwtToken.GenerateJwtToken(user);
                    if (model.PasswordHash == user.PasswordHash)
                    {
                        var user2 = _loginPage.ValidateUsers(model);
                        HttpContext.Session.SetInt32("id", user2.UserId);
                        HttpContext.Session.SetString("Name", user2.FirstName);
                        HttpContext.Session.SetString("IsLoggedIn", "true");
                        Response.Cookies.Append("token", token.ToString());
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
            //try
            //{
            //    var response = await _emailSender.SendEmail(model);

            //    if (response.IsSuccess)
            //    {
            //        return RedirectToAction("PatientLoginPage");
            //    }
            //    else
            //    {
            //        ModelState.AddModelError("Email", response.ErrorMessage);
            //        return RedirectToAction("ForgotPassword");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return RedirectToAction("ForgotPassword");
            //}
            PasswordReset passwordReset = new PasswordReset();

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

                passwordReset.Token = resetToken;
                passwordReset.CreatedDate = DateTime.Now;
                passwordReset.Email = model.UserName;
                passwordReset.IsModified = false;

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "HalloDoc"),
                    Subject = "Set up your Account",
                    IsBodyHtml = true,
                    Body = $"Please click the following link to reset your password: <a href='{resetLink}'>Click Here</a>"
                };
                var user = _loginPage.ValidateAspNetUser(model);
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

        [HttpPost]
        public IActionResult CreatePassword(string token)
        {

            var useremail = _sescontext.HttpContext.Session.GetString("Token");
            PasswordReset pr = _loginPage.ValidateToken(token);

            if (pr == null || pr.IsModified == true)
            {
                return NotFound();
            }

            TimeSpan diff = DateTime.Now.Subtract(pr.CreatedDate);
            double hours = diff.TotalHours;
            if (hours > 24)
            {
                return NotFound();
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {

            var useremail = _sescontext.HttpContext.Session.GetString("UserEmail");
            AspNetUser user = _loginPage.ValidateUserForResetPassword(model, useremail);
            if (user != null && model.Password == model.ConfirmPassword)
            {
                //user.PasswordHash = model.Password;
                //_context.SaveChanges();
                _loginPage.SetPasswordForResetPassword(user, model);
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

        [CustomAuthorize("Patient")]
        public IActionResult PatientDashboardAndMedicalHistory()
        {
            var userId = HttpContext.Session.GetInt32("id");

            var data = _patientDashboard.GetDashboardData((int)userId);



            var viewModel = new DashboardViewModel
            {
                requests = data,
                Username = _patientDashboard.ValidateUsername((int)userId)
            };

            return View(viewModel);
        }

        public IActionResult RequestForMe()
        {
            var user_id = HttpContext.Session.GetInt32("id");
            User user = _createRequestForMe.ValidateUser((int)user_id);
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
            Request request = new Request();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();

            var user = HttpContext.Session.GetInt32("id");
            var temp = model.State.ToLower().Trim();
            var region = _createRequestForMe.ValidateRegion(model);

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

            _createRequestForMe.RequestForMe(model, (int)user, region);

            return RedirectToAction("PatientDashboardAndMedicalHistory");
        }

        [HttpPost]
        public async Task<IActionResult> RelativePatientRequest(PatientRequestSomeoneElse model)
        {

            Request request = new Request();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();
            var user = HttpContext.Session.GetInt32("id");
            var region = _createRequestForSomeoneElse.ValidateRegion(model);
            var user_id = HttpContext.Session.GetInt32("id");
            var users = _createRequestForSomeoneElse.ValidateUser(model, (int)user_id);
            int day = (int)users.IntDate;
            string month = users.StrMonth;
            int year = (int)users.IntYear;
            int monthNumber = DateTime.ParseExact(month, "MMMM", null).Month;
            if (region == null)
            {
                ModelState.AddModelError("State", "Currently we are not serving in this region");
                return View(model);
            }
            BlockRequest blockedUser = _createRequestForSomeoneElse.CheckForBlockedRequest(model);
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

            _createRequestForSomeoneElse.RequestForSomeoneElse(model, (int)user_id, users, region);
            return RedirectToAction("PatientDashboardAndMedicalHistory");
        }

        public IActionResult RequestForSomeoneElse()
        { return View(); }

        public IActionResult PatientDashboardViewDocuments(int requestid)
        {
            var user_id = HttpContext.Session.GetInt32("id");

            // include() method creates object of RequestClient table where Request.RequestClientId = RequestClient.RequestClientId and this object is added to the Request table (kind of join operation). only those records are present in the variable 'request' whose requestId matches with the id passed in argument
            var request = _viewDocuments.GetRequestWithClient(requestid);

            // Similarly, we include the records of Admin and Physician where Admin.AdminId = RequestWiseFiles.AdminId and Physician.PhysicianId = Admin.AdminId and only those records are present in the variable 'documents' whose requestId matches with the id passed in argument
            var documents = _viewDocuments.ValidateFile(requestid);

            var user = _viewDocuments.ValidateUser((int)user_id);


            ViewDocumentModel viewDocumentModal = new ViewDocumentModel()
            {
                requestId = requestid,
                patient_name = string.Concat(request.RequestClient.FirstName, ' ', request.RequestClient.LastName),
                name = string.Concat(user.FirstName, ' ', user.LastName),
                confirmation_number = request.ConfirmationNumber,
                requestWiseFiles = documents,
                uploader_name = string.Concat(request.FirstName, ' ', request.LastName),
                Username = _viewDocuments.UserFirstName((int)user_id)
            };
            return View(viewDocumentModal);
        }
        [HttpPost]
        public IActionResult SetImageContent(ViewDocumentModel model, int requestId)
        {
            var user_id = HttpContext.Session.GetInt32("id");
            var request = _viewDocuments.GetRequestWithUser(requestId);

            var viewModel = new ViewDocumentModel
            {
                ImageContent = model.ImageContent,
            };
            if (model.ImageContent != null && model.ImageContent.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", model.ImageContent.FileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    model.ImageContent.CopyTo(stream);

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
                //_context.RequestWiseFiles.Add(requestWiseFile);
                //_context.SaveChanges();
                _viewDocuments.AddFile(requestWiseFile);
            }

            return RedirectToAction("PatientDashboardViewDocuments", new { requestID = model.requestId });
        }

        public IActionResult PatientProfile()
        {
            var user_id = HttpContext.Session.GetInt32("id");
            //var request = _context.Requests.Include(r => r.RequestClient).FirstOrDefault(u => u.RequestId == requestid);
            var user = _profile.ValidateUser((int)user_id);
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
            var user = _profile.ValidateUser((int)user_id);

            _profile.EditPatientData(model, (int)user_id);
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

        public IActionResult CreatePassword()
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