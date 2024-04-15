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

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult ConcludeCare(int requestid)
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

                Request request = _adminInterface.ValidateRequest(requestid);
                RequestClient rc = _adminInterface.GetRequestClientFromId(request.RequestClientId);
                string fname = rc.FirstName + " " + rc.LastName + " ";
                User user = _adminInterface.ValidateUserByRequestId(request);
                List<RequestWiseFile> rwf = _adminInterface.GetFileData(requestid);

                ViewUploadsModel vum = new ViewUploadsModel()
                {
                    confirmation_number = request.ConfirmationNumber,
                    requestId = requestid,
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
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
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

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
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

        [HttpPost]
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
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

        [CustomAuthorize("Admin Provider", "Scheduling")]
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


    }
}
