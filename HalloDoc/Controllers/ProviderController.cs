using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ClosedXML.Excel;
using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using DocumentFormat.OpenXml.InkML;
using System.Globalization;
using DocumentFormat.OpenXml.Office2010.Excel;
using HalloDocMvc.Entity.ViewModel;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Office.Interop.Excel;
using HalloDoc.LogicLayer.Patient_Interface;
using static HalloDoc.DataLayer.Models.Enums;
using System.Collections;
using HalloDoc.LogicLayer.Patient_Repository;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Drawing;
using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using Newtonsoft.Json.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using System.Configuration.Provider;
using Twilio.Base;
using Twilio.Types;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualBasic;
using Twilio.Http;
using Request = HalloDoc.DataLayer.Models.Request;
using Org.BouncyCastle.Asn1.Ocsp;
using DocumentFormat.OpenXml.Bibliography;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using Microsoft.AspNetCore.Http;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using static System.Runtime.InteropServices.JavaScript.JSType;
using iText.Kernel.Utils;
//using Twilio.Http;
//using System.Diagnostics;
//using HalloDoc.Data;
namespace HalloDoc.Controllers
{
    public class ProviderController : Controller
    {
        private readonly IAdminInterface _adminInterface;
        private readonly IHttpContextAccessor _sescontext;
        private readonly IJwtToken _jwtToken;
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPatientRequest _patientRequest;
        private readonly IProviderInterface _providerInterface;


        public ProviderController(IAdminInterface adminInterface, IHttpContextAccessor sescontext, IJwtToken jwtToken, IConfiguration configuration, IPatientRequest patientRequest, IProviderInterface providerInterface)
        {
            _adminInterface = adminInterface;
            _sescontext = sescontext;
            _jwtToken = jwtToken;
            _configuration = configuration;
            _patientRequest = patientRequest;
            _providerInterface = providerInterface;
        }

        [CustomAuthorize("Provider", "AdminDashboard")]
        public IActionResult ConcludeCare(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                Request request = _adminInterface.ValidateRequest(id);
                RequestClient rc = _adminInterface.GetRequestClientFromId(request.RequestClientId);
                string fname = rc.FirstName + " " + rc.LastName + " ";
                User user = _adminInterface.ValidateUserByRequestId(request);
                List<RequestWiseFile> rwf = _adminInterface.GetFileData(id);

                ViewUploadsModel vum = new ViewUploadsModel()
                {
                    confirmation_number = request.ConfirmationNumber,
                    requestId = id,
                    user = user,
                    requestWiseFiles = rwf,
                    an = an,
                    FullName = fname,
                };
                return View(vum);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view the uploaded files";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Provider", "AdminDashboard")]
        // function to store the newly uploaded file from View Uploads view
        public IActionResult SetImageContent(ViewUploadsModel model, int requestId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 1;
                var request = _adminInterface.GetRequestWithUser(requestId);
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                ViewUploadsModel viewModel = new ViewUploadsModel
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
                        RequestId = request.RequestId,
                        IsDeleted = new BitArray(1, false)
                    };
                    _adminInterface.AddFile(requestWiseFile);
                }

                return RedirectToAction("ConcludeCare", new { requestId = model.requestId });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to upload the file";
                return RedirectToAction("ConcludeCare", new { requestId = model.requestId });
            }
        }

        [CustomAuthorize("Provider", "AdminDashboard")]
        // function to delete individual file from View Uploads view
        public IActionResult DeleteIndividual(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                int reqId = _adminInterface.SingleDelete(id);
                return RedirectToAction("ConcludeCare", new { requestId = reqId });
            }

            catch (Exception ex)
            {
                int reqId = _adminInterface.SingleDelete(id);
                TempData["error"] = "Unable to delete this file";
                return RedirectToAction("ConcludeCare", new { requestId = reqId });
            }
        }

        /// <summary>
        /// Action called when we conclude the case
        /// </summary>
        /// <param name="model"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [CustomAuthorize("Provider", "AdminDashboard")]
        public IActionResult ConcludeCaseSubmitAction(ViewUploadsModel model, int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                if (!_providerInterface.isEncounterFinalized(id))
                {
                    TempData["error"] = "Encounter form is not finalized for this case";
                    return RedirectToAction("ConcludeCare", new { id = id });
                }

                bool isConcluded = _providerInterface.ConcludeCaseSubmitAction(model, id, p);
                return RedirectToAction("AdminDashboard", "Admin");
            }

            catch (Exception ex)
            {
                int reqId = _adminInterface.SingleDelete(id);
                TempData["error"] = "Unable to delete this file";
                return RedirectToAction("AdminDashboard", "Admin");
            }
        }

        [CustomAuthorize("Provider", "Scheduling")]
        public IActionResult MySchedule()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 19;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                SchedulingViewModel svm = new SchedulingViewModel
                {
                    adminNavbarModel = an,
                    allRegions = _providerInterface.GetProviderRegionFromId(p.PhysicianId),
                    physicianId = p.PhysicianId
                };
                return View(svm);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view scheduling page";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Provider", "Scheduling")]
        public IActionResult GetProviderScheduleData()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 19;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                string[] color = { "#edacd2", "#a5cfa6" };
                List<ShiftDetail> shiftDetails = _providerInterface.GetProviderScheduleData(p.PhysicianId);

                List<ShiftDTO> list = shiftDetails.Select(s => new ShiftDTO
                {
                    resourceId = s.Shift.PhysicianId,
                    Id = s.ShiftDetailId,
                    title = _adminInterface.GetPhysicianNameFromId(s.Shift.PhysicianId, s.ShiftId),
                    start = s.ShiftDate.ToString("yyyy-MM-dd") + s.StartTime.ToString("THH:mm:ss"),
                    end = s.ShiftDate.ToString("yyyy-MM-dd") + s.EndTime.ToString("THH:mm:ss"),
                    color = color[s.Status]
                }).ToList();
                return Json(list);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to fetch provider scheduling data";
                return RedirectToAction("AdminDashboard");
            }

        }

        [CustomAuthorize("Provider", "Scheduling")]
        public IActionResult GetProviderDetailsForSchedule(int RegionId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 19;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                List<SchedulingViewModel> model = _providerInterface.GetProviderInformation(p.PhysicianId);

                List<ProviderDTO> list = model.Select(p => new ProviderDTO
                {
                    Id = p.physicianId,
                    title = string.Concat(p.ProviderName, " ") ?? "",
                    imageUrl = "/Physician/" + p.physicianId + "/Profile.png",
                }).ToList();
                //}
                return Json(list);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to fetch provider scheduling data";
                return RedirectToAction("AdminDashboard");
            }

        }

        [CustomAuthorize("Provider", "Scheduling")]
        public IActionResult CreateNewShift(SchedulingViewModel model, List<int> RepeatedDays, int physician)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 19;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                model.physicianId = physician;
                if (_providerInterface.CreateNewShift(model, RepeatedDays, (int)userId))
                {
                    TempData["success"] = "Shift created successfully";
                    return RedirectToAction("MySchedule");
                }
                else
                {
                    TempData["error"] = "Sorry, shift is not created!";
                    return RedirectToAction("MySchedule");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the new shift";
                return RedirectToAction("MySchedule");
            }
        }

        [CustomAuthorize("Provider", "MyProfile")]
        public IActionResult MyProfile()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 20;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                EditProviderAccountViewModel model = _providerInterface.GetProviderProfile(p.PhysicianId, an);
                return View(model);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view provider profile";
                return RedirectToAction("AdminDashboard", "Admin");
            }
        }

        [CustomAuthorize("Provider", "MyProfile")]
        public async Task<IActionResult> RequestToEditProfile(int id, string requestProfile)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 20;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                List<Admin> ad2 = _providerInterface.GetAllAdmins();

                int emailSentCount = 1;
                bool isEmailSent = false;
                foreach (var item in ad2)
                {
                    emailSentCount = 1;
                    isEmailSent = false;
                    while (emailSentCount <= 3 && !isEmailSent)
                    {
                        string senderEmail = "tatva.dotnet.ishanbhatt@outlook.com";
                        string senderPassword = "Ishan@1503";
                        string subject = "HalloDoc - Request to edit profile";
                        string platformTitle = "HalloDoc";
                        var body = $"Hey, <br/> Please edit my profile. I want to edit following details: <br/> {requestProfile} <br /><br />Regards,<br/>{platformTitle}<br/>";

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
                                Body = body
                            };

                            mailMessage.To.Add(item.Email);
                            await client.SendMailAsync(mailMessage);
                            isEmailSent = true;
                            DateTime temp = DateTime.Now;
                            _adminInterface.AddEmailLog(body, subject, item.Email, 2, null, null, null, item.AdminId, null, temp, isEmailSent, emailSentCount);
                        }

                        catch (Exception ex)
                        {
                            if (emailSentCount >= 3)
                            {
                                DateTime temp = DateTime.Now;
                                _adminInterface.AddEmailLog(body, subject, item.Email, 2, null, null, null, item.AdminId, null, temp, false, emailSentCount);
                            }
                            emailSentCount++;
                        }
                    }

                    int smsCount = 1;
                    bool isSMSSent = false;
                    while (smsCount <= 3 && !isSMSSent)
                    {
                        string messageSMS = $@"Hey, Please edit my profile. I want to edit following details: {requestProfile}";

                        var accountSid = _configuration["Twilio:accountSid"];
                        var authToken = _configuration["Twilio:authToken"];
                        var twilionumber = _configuration["Twilio:twilioNumber"];
                        string num = "+917990117699";
                        try
                        {
                            TwilioClient.Init(accountSid, authToken);
                            //var messageBody =
                            var message2 = MessageResource.Create(
                                from: new Twilio.Types.PhoneNumber(twilionumber),
                                body: messageSMS,
                                to: new Twilio.Types.PhoneNumber(num)
                            );
                            isSMSSent = true;
                            DateTime temp = DateTime.Now;
                            _adminInterface.AddSmsLogFromSendLink(messageSMS, num, null, temp, smsCount, isSMSSent, 1);
                            break;
                        }

                        catch (Exception ex)
                        {
                            if (smsCount >= 3)
                            {
                                DateTime temp = DateTime.Now;
                                _adminInterface.AddSmsLogFromSendLink(messageSMS, num, null, temp, smsCount, false, 1);
                            }
                            smsCount++;
                        }
                    }
                }
                TempData["success"] = "Request to edit profile sent successfully";
                return RedirectToAction("MyProfile");

            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view provider profile";
                return RedirectToAction("AdminDashboard", "Admin");
            }
        }

        [CustomAuthorize("Provider", "ProviderInvoicing")]
        public IActionResult MyInvoicing()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 21;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                InvoicingViewModel ivm = new InvoicingViewModel
                {
                    adminNavbarModel = an,
                };
                return View(ivm);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view invoicing information";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Provider", "ProviderInvoicing")]
        public IActionResult GetTimesheetFromDate(string date)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 21;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                string[] bothDates = date.Split('-');
                string format = "M/d/yyyy";
                DateTime startDate = DateTime.ParseExact(bothDates[0], format, CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(bothDates[1], format, CultureInfo.InvariantCulture);
                InvoicingViewModel model = _providerInterface.GetTimesheetOnInvoicing(startDate, endDate, an, (int)userId);
                return PartialView("ProviderTimeSheetPartialView", model);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view invoicing information";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Provider", "ProviderInvoicing")]
        public bool CheckFinalized(string date)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 21;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                string[] bothDates = date.Split('-');
                string format = "M/d/yyyy";
                DateTime startDate = DateTime.ParseExact(bothDates[0], format, CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(bothDates[1], format, CultureInfo.InvariantCulture);
                return _providerInterface.CheckFinalized(startDate, endDate, (int)userId);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view invoicing information";
                return false;
            }
        }

        [CustomAuthorize("Admin Provider", "ProviderInvoicing")]
        public IActionResult BiWeeklyTimesheet(AdminInvoicingViewModel? model, string? dateRange = null, int? pid = null)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                int x = (int)(pid != null ? pid : 0);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    x = p.PhysicianId;
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                if (pid != null)
                {
                    an.Tab = 7;
                }
                else
                {
                    an.Tab = 21;
                }
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;


                string[] bothDates = dateRange.Split('-');
                string format = "M/d/yyyy";
                DateTime startDate = DateTime.ParseExact(bothDates[0], format, CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(bothDates[1], format, CultureInfo.InvariantCulture);

                InvoicingViewModel ivm = _providerInterface.GetBiWeeklyTimesheet(startDate, endDate, an, x);
                return View(ivm);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view invoicing information";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin Provider", "ProviderInvoicing")]
        public IActionResult SubmitTimeSheet(InvoicingViewModel model, DateTime startDate, DateTime endDate, int phyId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                Physician phy = _adminInterface.FetchPhysician(phyId);
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 21;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;


                bool isSubmitted = _providerInterface.SubmitTimesheet(model, startDate, endDate, (int)phy.AspNetUserId);
                if (isSubmitted)
                {
                    TempData["success"] = "Timesheet details added successfully";
                }
                else
                {
                    TempData["error"] = "Unable to add timesheet details";
                }
                if (an.roleName == "Provider")
                {
                    return RedirectToAction("MyInvoicing");
                }
                else
                {
                    return RedirectToAction("Invoicing", "Admin");
                }
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to add timesheet details" + ex.Message;
                return RedirectToAction("MyInvoicing");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin Provider", "ProviderInvoicing")]
        public IActionResult SubmitReimbursement(int phyId, int ind, DateTime startDate, DateTime endDate, string item, int amount, IFormFile upload)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                }
                an.Tab = 21;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                bool isSubmitted = false;
                if (upload != null)
                {
                    isSubmitted = _providerInterface.AddReimbursementData(ind, startDate, endDate, phyId, item, amount, upload);
                }
                else
                {
                    isSubmitted = _providerInterface.AddReimbursementData(ind, startDate, endDate, phyId, item, amount, null);
                }
                if (isSubmitted)
                {
                    TempData["success"] = "Reimbursement details added successfully";
                }
                else
                {
                    TempData["error"] = "Unable to add timesheet details";
                }
                if (an.roleName == "Provider")
                {
                    return RedirectToAction("MyInvoicing");
                }
                else
                {
                    return RedirectToAction("Invoicing", "Admin");
                }
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to add timesheet details" + ex.Message;
                return RedirectToAction("MyInvoicing");
            }
        }

        [CustomAuthorize("Provider", "ProviderInvoicing")]
        public IActionResult FinalizeTimesheet(int tid)
        {
            try
            {
                bool isFinalized = _providerInterface.FinalizeTimesheet(tid);
                if (isFinalized)
                {
                    TempData["success"] = "Timesheet finalized successfully";
                    return RedirectToAction("MyInvoicing");
                }
                else
                {
                    TempData["error"] = "Unable to finalze the timesheet";
                    return RedirectToAction("MyInvoicing");
                }
            }

            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("MyInvoicing");
            }
        }

        public IActionResult DeleteFile(int id)
        {
            _providerInterface.DeleteFile(id);
            return RedirectToAction("Invoicing");
        }

    }
}
