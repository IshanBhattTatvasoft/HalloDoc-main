
using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using HalloDoc.LogicLayer.Patient_Repository;
using System.Net.Mail;
using System.Net;

namespace HalloDoc.Controllers
{
    public class PatientRequestController : Controller
    {
        //private readonly ILogger<PatientRequestController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _sescontext;
        private readonly IPatientRequest _patientRequest;
        private readonly IFamilyRequest _familyRequest;
        private readonly IBusinessRequest _businessRequest;
        private readonly IConciergeRequest _conciergeRequest;
        private readonly IAdminInterface _adminInterface;
        [ActivatorUtilitiesConstructor]
        public PatientRequestController(ApplicationDbContext context, IPatientRequest patientRequest, IFamilyRequest familyRequest, IBusinessRequest businessRequest, IConciergeRequest conciergeRequest, IHttpContextAccessor sescontext, IAdminInterface adminInterface)
        {
            /* _logger = logger;*/
            _context = context;
            _patientRequest = patientRequest;
            _familyRequest = familyRequest;
            _businessRequest = businessRequest;
            _conciergeRequest = conciergeRequest;
            _sescontext = sescontext;
            _adminInterface = adminInterface;
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


            if (ModelState.IsValid)
            {
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

                _patientRequest.InsertDataPatientRequest(model);
                TempData["success"] = "Request created successfully";
                return RedirectToAction("PatientSite", "Login");
            }

            else
            {
                TempData["error"] = "Unable to create the request";
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




            if (ModelState.IsValid)
            {

                if (!PatientCheck(model.Email))
                {
                    int emailSentCount = 1;
                    bool isEmailSent = false;
                    string resetToken = Guid.NewGuid().ToString();
                    string subject = "HalloDoc - Create your account";
                    string platformTitle = "HalloDoc";
                    string resetLink = $"{Request.Scheme}://{Request.Host}/Login/CreatePatientAccount";
                    var body = $"<h3>Hey {model.FirstName + " " + model.LastName}</h3><br> Please click the following link to reset your password:<br> <a href='{resetLink}'>Click Here</a><br /><br />Regards,<br/>{platformTitle}<br/>";
                    string senderEmail = "tatva.dotnet.ishanbhatt@outlook.com";
                    string senderPassword = "Ishan@1503";

                    while (emailSentCount <= 3 && !isEmailSent)
                    {

                        try
                        {


                            SmtpClient client = new SmtpClient("smtp.office365.com")
                            {
                                Port = 587,
                                Credentials = new NetworkCredential(senderEmail, senderPassword),
                                EnableSsl = true,
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                                UseDefaultCredentials = false
                            };

                            MailMessage mailMessage = new MailMessage
                            {
                                From = new MailAddress(senderEmail, "HalloDoc"),
                                Subject = subject,
                                IsBodyHtml = true,
                                Body = body,
                            };
                            mailMessage.To.Add(model.Email);

                            await client.SendMailAsync(mailMessage);
                            isEmailSent = true;
                            DateTime temp = DateTime.Now;
                            _adminInterface.AddEmailLog(body, subject, model.Email, 3, null, null, null, null, null, temp, isEmailSent, emailSentCount);
                        }
                        catch (Exception ex)
                        {
                            if (emailSentCount >= 3)
                            {
                                DateTime temp = DateTime.Now;
                                _adminInterface.AddEmailLog(body, subject, model.Email, 3, null, null, null, null, null, temp, false, emailSentCount);
                            }
                            emailSentCount++;
                            ModelState.AddModelError("Email", "Invalid Email");
                            return RedirectToAction("PatientSite");
                        }
                    }
                }

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

                _familyRequest.InsertDataFamilyRequest(model);
                TempData["success"] = "Request created successfully";
                return RedirectToAction("PatientSite", "Login");

            }

            else
            {
                TempData["error"] = "Unable to create the request";
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



            if (ModelState.IsValid)
            {
                if (!PatientCheck(model.Email))
                {
                    int emailSentCount = 1;
                    bool isEmailSent = false;
                    string resetToken = Guid.NewGuid().ToString();
                    string subject = "HalloDoc - Create your account";
                    string platformTitle = "HalloDoc";
                    string resetLink = $"{Request.Scheme}://{Request.Host}/Login/CreatePatientAccount";
                    var body = $"<h3>Hey {model.FirstName + " " + model.LastName}</h3><br> Please click the following link to reset your password:<br> <a href='{resetLink}'>Click Here</a><br /><br />Regards,<br/>{platformTitle}<br/>";
                    string senderEmail = "tatva.dotnet.ishanbhatt@outlook.com";
                    string senderPassword = "Ishan@1503";

                    while (emailSentCount <= 3 && !isEmailSent)
                    {
                        try
                        {


                            SmtpClient client = new SmtpClient("smtp.office365.com")
                            {
                                Port = 587,
                                Credentials = new NetworkCredential(senderEmail, senderPassword),
                                EnableSsl = true,
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                                UseDefaultCredentials = false
                            };

                            //passwordReset.Token = resetToken;
                            //passwordReset.CreatedDate = DateTime.Now;
                            //passwordReset.Email = model.UserName;
                            //passwordReset.IsModified = false;

                            MailMessage mailMessage = new MailMessage
                            {
                                From = new MailAddress(senderEmail, "HalloDoc"),
                                Subject = "Create account for patient " + model.FirstName,
                                IsBodyHtml = true,
                                Body = body,
                            };
                            mailMessage.To.Add(model.Email);

                            await client.SendMailAsync(mailMessage);
                            isEmailSent = true;
                            DateTime temp = DateTime.Now;
                            _adminInterface.AddEmailLog(body, subject, model.Email, 3, null, null, null, null, null, temp, isEmailSent, emailSentCount);

                        }
                        catch (Exception ex)
                        {
                            if (emailSentCount >= 3)
                            {
                                DateTime temp = DateTime.Now;
                                _adminInterface.AddEmailLog(body, subject, model.Email, 3, null, null, null, null, null, temp, false, emailSentCount);
                            }
                            emailSentCount++;
                            ModelState.AddModelError("Email", "Invalid Email");
                            return RedirectToAction("PatientSite");
                        }
                    }
                }

                var existingUser = _conciergeRequest.ValidateAspNetUser(model);
                bool userExists = true;

                _conciergeRequest.InsertDataConciergeRequest(model);
                TempData["success"] = "Request created successfully";
                return RedirectToAction("PatientSite", "Login");

            }

            else
            {
                TempData["error"] = "Unable to create the request";
                return View("CreateConciergeRequest");
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateBusinessRequest(BusinessRequestModel model)
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






            if (ModelState.IsValid)
            {
                if (!PatientCheck(model.Email))
                {
                    int emailSentCount = 1;
                    bool isEmailSent = false;
                    string resetToken = Guid.NewGuid().ToString();
                    string subject = "HalloDoc - Create your account";
                    string platformTitle = "HalloDoc";
                    string resetLink = $"{Request.Scheme}://{Request.Host}/Login/CreatePatientAccount";
                    var body = $"<h3>Hey {model.FirstName + " " + model.LastName}</h3><br> Please click the following link to reset your password:<br> <a href='{resetLink}'>Click Here</a><br /><br />Regards,<br/>{platformTitle}<br/>";
                    string senderEmail = "tatva.dotnet.ishanbhatt@outlook.com";
                    string senderPassword = "Ishan@1503";

                    while (emailSentCount <= 3 && !isEmailSent)
                    {
                        try
                        {

                            SmtpClient client = new SmtpClient("smtp.office365.com")
                            {
                                Port = 587,
                                Credentials = new NetworkCredential(senderEmail, senderPassword),
                                EnableSsl = true,
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                                UseDefaultCredentials = false
                            };

                            MailMessage mailMessage = new MailMessage
                            {
                                From = new MailAddress(senderEmail, "HalloDoc"),
                                Subject = subject,
                                IsBodyHtml = true,
                                Body = body,
                            };
                            mailMessage.To.Add(model.Email);

                            await client.SendMailAsync(mailMessage);

                            isEmailSent = true;
                            DateTime temp = DateTime.Now;
                            _adminInterface.AddEmailLog(body, subject, model.Email, 3, null, null, null, null, null, temp, isEmailSent, emailSentCount);
                        }
                        catch (Exception ex)
                        {
                            if (emailSentCount >= 3)
                            {
                                DateTime temp = DateTime.Now;
                                _adminInterface.AddEmailLog(body, subject, model.Email, 3, null, null, null, null, null, temp, false, emailSentCount);
                            }
                            emailSentCount++;
                            ModelState.AddModelError("Email", "Invalid Email");
                            return RedirectToAction("PatientSite");
                        }
                    }
                }
                var region = _businessRequest.ValidateRegion(model);
                if (region == null)
                {
                    ModelState.AddModelError("State", "Currently we are not serving in this region");
                    return View(model);
                }
                var existingUser = _businessRequest.ValidateAspNetUser(model);
                bool userExists = true;

                _businessRequest.InsertDataBusinessRequest(model);
                TempData["success"] = "Request created successfully";
                return RedirectToAction("PatientSite", "Login");

            }

            else
            {
                TempData["error"] = "Unable to create the request";
                return View("CreateBusinessRequest");
            }
        }



        public bool PatientCheck(string email)
        {
            AspNetUser existingUser = _patientRequest.GetEmailFromAspNet(email);
            bool isValidEmail = true;
            if (existingUser == null)
            {
                isValidEmail = false;
            }
            return isValidEmail;
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