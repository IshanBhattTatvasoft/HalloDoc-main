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
using iText.Kernel.Geom;
using Microsoft.IdentityModel.Tokens;
//using Twilio.Http;
//using System.Diagnostics;
//using HalloDoc.Data;

namespace HalloDoc.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminInterface _adminInterface;
        private readonly IHttpContextAccessor _sescontext;
        private readonly IJwtToken _jwtToken;
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPatientRequest _patientRequest;


        public AdminController(IAdminInterface adminInterface, IHttpContextAccessor sescontext, IJwtToken jwtToken, IConfiguration configuration, IPatientRequest patientRequest)
        {
            _adminInterface = adminInterface;
            _sescontext = sescontext;
            _jwtToken = jwtToken;
            _configuration = configuration;
            _patientRequest = patientRequest;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //validate admin login
        public IActionResult PlatformLoginPage(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                AspNetUser user = new AuthManager().Login(model.UserName, model.PasswordHash);
                if (user != null)
                {
                    var token = _jwtToken.GenerateJwtToken(user);
                    if (model.PasswordHash == user.PasswordHash)
                    {
                        Admin ad = _adminInterface.ValidateUser(user.Email);
                        HttpContext.Session.SetInt32("id", ad.AdminId);
                        HttpContext.Session.SetString("name", ad.FirstName);
                        Response.Cookies.Append("token", token.ToString());
                        HttpContext.Session.SetString("IsLoggedIn", "true");

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

            return View(model);
        }

        // destroy session on logout
        public IActionResult Logout()
        {
            _sescontext.HttpContext.Session.Clear();
            Response.Cookies.Delete("token");
            return RedirectToAction("PatientLoginPage", "Login");
        }

        public IActionResult PlatformForgotPassword()
        {
            return View();
        }

        // admin dashboard function
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult AdminDashboard(string? status)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            Physician p = _adminInterface.GetPhysicianFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            if (ad != null)
            {
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            }
            else
            {
                an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
            }
            an.Tab = 1;
            string token = Request.Cookies["token"];
            string roleIdVal = _jwtToken.GetRoleId(token);
            List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
            ViewBag.Menu = menus;
            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("New", (int)userId, null, null, -1, 1, 10);
            return View(adminDashboardViewModel);
        }



        //[HttpPost]
        // function for new state of admin dashboard
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult New(string? status, string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("New", (int)userId, search, requestor, (int)region, page, pageSize);
                return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view requests of new state";
                return RedirectToAction("PatientLoginPage", "Login");
            }
        }

        //[HttpPost]
        // function for pending state of admin dashboard
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult Pending(string? status, string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Pending", (int)userId, search, requestor, (int)region, page, pageSize);
                return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Unable to view requests of pending state";
                return RedirectToAction("PatientLoginPage", "Login");
            }
        }

        //[HttpPost]
        // function for active state of admin dashboard
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult Active(string? status, string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Active", (int)userId, search, requestor, (int)region, page, pageSize);
                return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Unable to view requests of active state";
                return RedirectToAction("PatientLoginPage", "Login");
            }
        }

        //[HttpPost]
        // function for conclude state of admin dashboard
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult Conclude(string? status, string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Conclude", (int)userId, search, requestor, (int)region, page, pageSize);
                return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Unable to view requests of conclude state";
                return RedirectToAction("PatientLoginPage", "Login");
            }
        }

        //[HttpPost]
        // function for to-close state of admin dashboard
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult Toclose(string? status, string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("ToClose", (int)userId, search, requestor, (int)region, page, pageSize);
                return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Unable to view requests of to-close state";
                return RedirectToAction("PatientLoginPage", "Login");
            }
        }

        //[HttpPost]
        // function for unpaid state of admin dashboard
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult Unpaid(string? status, string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Unpaid", (int)userId, search, requestor, (int)region, page, pageSize);
                return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Unable to view requests of unpaid state";
                return RedirectToAction("PatientLoginPage", "Login");
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public bool UpdateProviderLocation(string lat, string lon)
        {
            bool isUpdated = false;
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

                isUpdated = _adminInterface.UpdateProviderLocation(lat, lon, p.PhysicianId);
                return isUpdated;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Unable to view requests of unpaid state";
                return isUpdated;
            }
        }

        [CustomAuthorize("Admin", "AdminDashboard")]
        // function to get data for excel sheet
        public List<Request> GetTableData()
        {
            List<Request> r = new List<Request> { new Request() };
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                List<Request> data = new List<Request>();

                data = _adminInterface.GetRequestDataInList();
                return data;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Unable to fetch data to export it";
                return r;
            }
        }

        [CustomAuthorize("Admin", "AdminDashboard")]
        // function to download the data of all requests in excel sheet
        public IActionResult DownloadAll()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            string token = Request.Cookies["token"];
            string roleIdVal = _jwtToken.GetRoleId(token);
            List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
            ViewBag.Menu = menus;
            try
            {
                List<Request> data = GetTableData();
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Data");


                worksheet.Cell(1, 1).Value = "Name";
                worksheet.Cell(1, 2).Value = "Date Of Birth";
                worksheet.Cell(1, 3).Value = "Requestor";
                worksheet.Cell(1, 4).Value = "Physician Name";
                worksheet.Cell(1, 5).Value = "Date of Service";
                worksheet.Cell(1, 6).Value = "Requested Date";
                worksheet.Cell(1, 7).Value = "Phone Number";
                worksheet.Cell(1, 8).Value = "Address";
                worksheet.Cell(1, 9).Value = "Notes";

                int row = 2;
                foreach (var item in data)
                {
                    var statusClass = "";
                    var dos = "";
                    var notes = "";
                    if (item.RequestTypeId == 1)
                    {
                        statusClass = "patient";
                    }
                    else if (item.RequestTypeId == 4)
                    {
                        statusClass = "business";
                    }
                    else if (item.RequestTypeId == 2)
                    {
                        statusClass = "family";
                    }
                    else
                    {
                        statusClass = "concierge";
                    }
                    foreach (var stat in item.RequestStatusLogs)
                    {
                        if (stat.Status == 2)
                        {
                            dos = stat.CreatedDate.ToString("MMMM dd,yyyy");
                            notes = stat.Notes ?? "";
                        }
                    }
                    worksheet.Cell(row, 1).Value = item.RequestClient.FirstName + item.RequestClient.LastName;
                    worksheet.Cell(row, 2).Value = DateTime.Parse($"{item.RequestClient.IntYear}-{item.RequestClient.StrMonth}-{item.RequestClient.IntDate}").ToString("MMMM dd,yyyy");
                    worksheet.Cell(row, 3).Value = statusClass.Substring(0, 1).ToUpper() + statusClass.Substring(1).ToLower() + item.FirstName + item.LastName;
                    worksheet.Cell(row, 4).Value = ("Dr." + item?.Physician == null ? "" : item?.Physician?.FirstName);
                    worksheet.Cell(row, 5).Value = item.CreatedDate.ToString("MMMM dd,yyyy");
                    worksheet.Cell(row, 6).Value = dos;
                    worksheet.Cell(row, 7).Value = item.RequestClient.PhoneNumber + "(Patient)" + (item.RequestTypeId != 4 ? item.PhoneNumber + statusClass.Substring(0, 1).ToUpper() + statusClass.Substring(1).ToLower() : "");
                    worksheet.Cell(row, 8).Value = (item.RequestClient.Address == null ? item.RequestClient.Address + item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode : item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode);
                    worksheet.Cell(row, 9).Value = item.RequestClient.Notes;
                    row++;
                }
                worksheet.Columns().AdjustToContents();

                var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "data.xlsx");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "AdminDashboard")]
        // function to dowload filtered data or data of particular state in excel sheet
        public IActionResult DownloadSpecificExcel(AdminDashboardTableView model)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            string token = Request.Cookies["token"];
            string roleIdVal = _jwtToken.GetRoleId(token);
            List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
            ViewBag.Menu = menus;
            try
            {
                List<Request> data = model.requests;
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Data");

                if (model.status == "N")
                {
                    worksheet.Cell(1, 1).Value = "Name";
                    worksheet.Cell(1, 2).Value = "Date Of Birth";
                    worksheet.Cell(1, 3).Value = "Requestor";
                    worksheet.Cell(1, 4).Value = "Requested Date";
                    worksheet.Cell(1, 5).Value = "Phone Number";
                    worksheet.Cell(1, 6).Value = "Address";
                    worksheet.Cell(1, 7).Value = "Notes";

                    int row = 2;
                    foreach (var item in data)
                    {
                        var statusClass = "";
                        var dos = "";
                        var notes = "";
                        if (item.RequestTypeId == 1)
                        {
                            statusClass = "patient";
                        }
                        else if (item.RequestTypeId == 4)
                        {
                            statusClass = "business";
                        }
                        else if (item.RequestTypeId == 2)
                        {
                            statusClass = "family";
                        }
                        else
                        {
                            statusClass = "concierge";
                        }
                        foreach (var stat in item.RequestStatusLogs)
                        {
                            if (stat.Status == 2)
                            {
                                dos = stat.CreatedDate.ToString("MMMM dd,yyyy");
                                notes = stat.Notes ?? "";
                            }
                        }
                        worksheet.Cell(row, 1).Value = item.RequestClient.FirstName + item.RequestClient.LastName;
                        worksheet.Cell(row, 2).Value = DateTime.Parse($"{item.RequestClient.IntYear}-{item.RequestClient.StrMonth}-{item.RequestClient.IntDate}").ToString("MMMM dd,yyyy");
                        worksheet.Cell(row, 3).Value = statusClass.Substring(0, 1).ToUpper() + statusClass.Substring(1).ToLower() + item.FirstName + item.LastName;
                        worksheet.Cell(row, 4).Value = item.CreatedDate.ToString("MMMM dd,yyyy");
                        worksheet.Cell(row, 5).Value = item.RequestClient.PhoneNumber + "(Patient)" + (item.RequestTypeId != 4 ? item.PhoneNumber + statusClass.Substring(0, 1).ToUpper() + statusClass.Substring(1).ToLower() : "");
                        worksheet.Cell(row, 6).Value = (item.RequestClient.Address == null ? item.RequestClient.Address + item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode : item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode);
                        worksheet.Cell(row, 7).Value = item.RequestClient.Notes;
                        row++;
                    }
                }

                else if (model.status == "P" || model.status == "A")
                {
                    worksheet.Cell(1, 1).Value = "Name";
                    worksheet.Cell(1, 2).Value = "Date Of Birth";
                    worksheet.Cell(1, 3).Value = "Requestor";
                    worksheet.Cell(1, 4).Value = "Physician Name";
                    worksheet.Cell(1, 5).Value = "Date of Service";
                    worksheet.Cell(1, 6).Value = "Phone Number";
                    worksheet.Cell(1, 7).Value = "Address";
                    worksheet.Cell(1, 8).Value = "Notes";

                    int row = 2;
                    foreach (var item in data)
                    {
                        var statusClass = "";
                        var dos = "";
                        var notes = "";
                        if (item.RequestTypeId == 1)
                        {
                            statusClass = "patient";
                        }
                        else if (item.RequestTypeId == 4)
                        {
                            statusClass = "business";
                        }
                        else if (item.RequestTypeId == 2)
                        {
                            statusClass = "family";
                        }
                        else
                        {
                            statusClass = "concierge";
                        }
                        foreach (var stat in item.RequestStatusLogs)
                        {
                            if (stat.Status == 2)
                            {
                                dos = stat.CreatedDate.ToString("MMMM dd,yyyy");
                                notes = stat.Notes ?? "";
                            }
                        }
                        worksheet.Cell(row, 1).Value = item.RequestClient.FirstName + item.RequestClient.LastName;
                        worksheet.Cell(row, 2).Value = DateTime.Parse($"{item.RequestClient.IntYear}-{item.RequestClient.StrMonth}-{item.RequestClient.IntDate}").ToString("MMMM dd,yyyy");
                        worksheet.Cell(row, 3).Value = statusClass.Substring(0, 1).ToUpper() + statusClass.Substring(1).ToLower() + item.FirstName + item.LastName;
                        worksheet.Cell(row, 4).Value = ("Dr." + item?.Physician == null ? "" : item?.Physician?.FirstName);
                        worksheet.Cell(row, 5).Value = dos;
                        worksheet.Cell(row, 6).Value = item.RequestClient.PhoneNumber + "(Patient)" + (item.RequestTypeId != 4 ? item.PhoneNumber + statusClass.Substring(0, 1).ToUpper() + statusClass.Substring(1).ToLower() : "");
                        worksheet.Cell(row, 7).Value = (item.RequestClient.Address == null ? item.RequestClient.Address + item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode : item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode);
                        worksheet.Cell(row, 8).Value = item.RequestClient.Notes;
                        row++;
                    }
                }

                else if (model.status == "C")
                {
                    worksheet.Cell(1, 1).Value = "Name";
                    worksheet.Cell(1, 2).Value = "Date Of Birth";
                    worksheet.Cell(1, 3).Value = "Physician Name";
                    worksheet.Cell(1, 4).Value = "Date of Service";
                    worksheet.Cell(1, 5).Value = "Phone Number";
                    worksheet.Cell(1, 6).Value = "Address";
                    worksheet.Cell(1, 7).Value = "Notes";

                    int row = 2;
                    foreach (var item in data)
                    {
                        var statusClass = "";
                        var dos = "";
                        var notes = "";
                        if (item.RequestTypeId == 1)
                        {
                            statusClass = "patient";
                        }
                        else if (item.RequestTypeId == 4)
                        {
                            statusClass = "business";
                        }
                        else if (item.RequestTypeId == 2)
                        {
                            statusClass = "family";
                        }
                        else
                        {
                            statusClass = "concierge";
                        }
                        foreach (var stat in item.RequestStatusLogs)
                        {
                            if (stat.Status == 2)
                            {
                                dos = stat.CreatedDate.ToString("MMMM dd,yyyy");
                                notes = stat.Notes ?? "";
                            }
                        }
                        worksheet.Cell(row, 1).Value = item.RequestClient.FirstName + item.RequestClient.LastName;
                        worksheet.Cell(row, 2).Value = DateTime.Parse($"{item.RequestClient.IntYear}-{item.RequestClient.StrMonth}-{item.RequestClient.IntDate}").ToString("MMMM dd,yyyy");
                        worksheet.Cell(row, 3).Value = ("Dr." + item?.Physician == null ? "" : item?.Physician?.FirstName);
                        worksheet.Cell(row, 4).Value = dos;
                        worksheet.Cell(row, 5).Value = item.RequestClient.PhoneNumber + "(Patient)" + (item.RequestTypeId != 4 ? item.PhoneNumber + statusClass.Substring(0, 1).ToUpper() + statusClass.Substring(1).ToLower() : "");
                        worksheet.Cell(row, 6).Value = (item.RequestClient.Address == null ? item.RequestClient.Address + item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode : item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode);
                        worksheet.Cell(row, 7).Value = item.RequestClient.Notes;
                        row++;
                    }
                }

                else if (model.status == "T")
                {
                    worksheet.Cell(1, 1).Value = "Name";
                    worksheet.Cell(1, 2).Value = "Date Of Birth";
                    worksheet.Cell(1, 3).Value = "Physician Name";
                    worksheet.Cell(1, 4).Value = "Date of Service";
                    worksheet.Cell(1, 5).Value = "Address";
                    worksheet.Cell(1, 6).Value = "Notes";

                    int row = 2;
                    foreach (var item in data)
                    {
                        var statusClass = "";
                        var dos = "";
                        var notes = "";
                        if (item.RequestTypeId == 1)
                        {
                            statusClass = "patient";
                        }
                        else if (item.RequestTypeId == 4)
                        {
                            statusClass = "business";
                        }
                        else if (item.RequestTypeId == 2)
                        {
                            statusClass = "family";
                        }
                        else
                        {
                            statusClass = "concierge";
                        }
                        foreach (var stat in item.RequestStatusLogs)
                        {
                            if (stat.Status == 2)
                            {
                                dos = stat.CreatedDate.ToString("MMMM dd,yyyy");
                                notes = stat.Notes ?? "";
                            }
                        }
                        worksheet.Cell(row, 1).Value = item.RequestClient.FirstName + item.RequestClient.LastName;
                        worksheet.Cell(row, 2).Value = DateTime.Parse($"{item.RequestClient.IntYear}-{item.RequestClient.StrMonth}-{item.RequestClient.IntDate}").ToString("MMMM dd,yyyy");
                        worksheet.Cell(row, 3).Value = ("Dr." + item?.Physician == null ? "" : item?.Physician?.FirstName);
                        worksheet.Cell(row, 4).Value = dos;
                        worksheet.Cell(row, 5).Value = (item.RequestClient.Address == null ? item.RequestClient.Address + item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode : item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode);
                        worksheet.Cell(row, 6).Value = item.RequestClient.Notes;
                        row++;
                    }
                }

                else if (model.status == "U")
                {
                    worksheet.Cell(1, 1).Value = "Name";
                    worksheet.Cell(1, 2).Value = "Physician Name";
                    worksheet.Cell(1, 3).Value = "Date of Service";
                    worksheet.Cell(1, 4).Value = "Phone Number";
                    worksheet.Cell(1, 5).Value = "Address";
                    worksheet.Cell(1, 6).Value = "Notes";

                    int row = 2;
                    foreach (var item in data)
                    {
                        var statusClass = "";
                        var dos = "";
                        var notes = "";
                        if (item.RequestTypeId == 1)
                        {
                            statusClass = "patient";
                        }
                        else if (item.RequestTypeId == 4)
                        {
                            statusClass = "business";
                        }
                        else if (item.RequestTypeId == 2)
                        {
                            statusClass = "family";
                        }
                        else
                        {
                            statusClass = "concierge";
                        }
                        foreach (var stat in item.RequestStatusLogs)
                        {
                            if (stat.Status == 2)
                            {
                                dos = stat.CreatedDate.ToString("MMMM dd,yyyy");
                                notes = stat.Notes ?? "";
                            }
                        }
                        worksheet.Cell(row, 1).Value = item.RequestClient.FirstName + item.RequestClient.LastName;
                        worksheet.Cell(row, 2).Value = DateTime.Parse($"{item.RequestClient.IntYear}-{item.RequestClient.StrMonth}-{item.RequestClient.IntDate}").ToString("MMMM dd,yyyy");
                        worksheet.Cell(row, 3).Value = dos;
                        worksheet.Cell(row, 4).Value = item.RequestClient.PhoneNumber + "(Patient)" + (item.RequestTypeId != 4 ? item.PhoneNumber + statusClass.Substring(0, 1).ToUpper() + statusClass.Substring(1).ToLower() : "");
                        worksheet.Cell(row, 5).Value = (item.RequestClient.Address == null ? item.RequestClient.Address + item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode : item.RequestClient.Street + item.RequestClient.City + item.RequestClient.State + item.RequestClient.ZipCode);
                        worksheet.Cell(row, 6).Value = item.RequestClient.Notes;
                        row++;
                    }
                }
                worksheet.Columns().AdjustToContents();

                var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                string s = "";
                if (model.status == "N") s = "New";
                else if (model.status == "P") s = "Pending";
                else if (model.status == "A") s = "Active";
                else if (model.status == "C") s = "Conclude";
                else if (model.status == "T") s = "ToClose";
                else s = "Unpaid";
                string fileName = $"{s}.xlsx";
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        [CustomAuthorize("Admin", "AdminDashboard")]
        public async Task<IActionResult> RequestDtySupport(string reason)
        {
            try
            {

                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                List<KeyValuePair<int, string>> unscheduledPhy = _adminInterface.GetEmailForDtySupport();

                int emailSentCount = 1;
                bool isEmailSent = false;
                foreach (var item in unscheduledPhy)
                {
                    emailSentCount = 1;
                    isEmailSent = false;
                    while (emailSentCount <= 3 && !isEmailSent)
                    {
                        string senderEmail = "tatva.dotnet.ishanbhatt@outlook.com";
                        string senderPassword = "Ishan@1503";
                        string subject = "HalloDoc - Request for Support";
                        string platformTitle = "HalloDoc";
                        var body = $"Hey, <br/> If you are free, we need support due to the following reason <br/> {reason}<br/> Please help us if you can<br /><br />Regards,<br/>{platformTitle}<br/>";

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

                            mailMessage.To.Add(item.Value);
                            await client.SendMailAsync(mailMessage);
                            isEmailSent = true;
                            DateTime temp = DateTime.Now;
                            _adminInterface.AddEmailLog(body, subject, item.Value, 1, null, null, null, null, item.Key, temp, isEmailSent, emailSentCount);
                        }

                        catch (Exception ex)
                        {
                            if (emailSentCount >= 3)
                            {
                                DateTime temp = DateTime.Now;
                                _adminInterface.AddEmailLog(body, subject, item.Value, 1, null, null, null, null, item.Key, temp, false, emailSentCount);
                            }
                            emailSentCount++;
                        }
                    }
                }
                TempData["success"] = "Case data updated successfully";
                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {

                TempData["error"] = "Unable to edit the case information";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        // function to display data in View Case view
        public IActionResult ViewCase(int id)
        {
            try
            {
                Request r = _adminInterface.GetReqFromReqId(id);
                if (r == null)
                {
                    TempData["error"] = "No such request exists";
                    return RedirectToAction("PageNotFoundError");
                }
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
                    Physician ph = _adminInterface.GetPhysicianFromId((int)userId);
                    Request re = _adminInterface.GetReqFromReqId(id);
                    if (re == null)
                    {
                        TempData["error"] = "Request does not exist";
                        return RedirectToAction("AdminDashboard");
                    }
                    if (re.PhysicianId != ph.PhysicianId)
                    {
                        TempData["error"] = "Details of unassigned case cannot be accessed";
                        return RedirectToAction("AdminDashboard");
                    }
                }
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                Request request = _adminInterface.ValidateRequest(id);

                RequestClient user = _adminInterface.ValidateRequestClient(request.RequestClientId);

                int intYear = (int)user.IntYear;
                int intDate = (int)user.IntDate;
                string month = user.StrMonth;
                int mon = 0;
                if (month.Length > 1)
                {

                    if (month == "January" || month == "1")
                    {
                        mon = 1;
                    }
                    else if (month == "February" || month == "2")
                    {
                        mon = 2;
                    }
                    else if (month == "March" || month == "3")
                    {
                        mon = 3;
                    }
                    else if (month == "April" || month == "4")
                    {
                        mon = 4;
                    }
                    else if (month == "May" || month == "5")
                    {
                        mon = 5;
                    }
                    else if (month == "June" || month == "6")
                    {
                        mon = 6;
                    }
                    else if (month == "July" || month == "7")
                    {
                        mon = 7;
                    }
                    else if (month == "August" || month == "8")
                    {
                        mon = 8;
                    }
                    else if (month == "September" || month == "9")
                    {
                        mon = 9;
                    }
                    else if (month == "October" || month == "10")
                    {
                        mon = 10;
                    }
                    else if (month == "November" || month == "11")
                    {
                        mon = 11;
                    }
                    else if (month == "December" || month == "12")
                    {
                        mon = 12;
                    }
                }
                int mon1 = 0;
                if (month.Length == 1)
                {
                    mon1 = int.Parse(month);
                }
                DateTime date = new DateTime();
                if (month.Length == 1)
                {
                    date = new DateTime(intYear, mon1, intDate);
                }
                if (month.Length > 1)
                {
                    date = new DateTime(intYear, mon, intDate);
                }

                ViewCaseModel viewCase = new ViewCaseModel
                {
                    RequestId = id,
                    PatientNotes = user.Notes,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    DOB = date,
                    ConfirmationNo = request.ConfirmationNumber,
                    reqTypeId = request.RequestTypeId,
                    regions = _adminInterface.GetAllRegion(),
                    Status = request.Status,
                    caseTags = _adminInterface.GetAllCaseTags(),
                    an = an,
                    regionName = user.State,
                    Address = user.Street + " " + user.City + " " + user.State + " " + user.ZipCode,
                };

                return View(viewCase);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view the case";
                return RedirectToAction("AdminDashboard");
            }
        }


        [HttpPost]
        [CustomAuthorize("Admin", "AdminDashboard")]
        // action to store edited information of view case in database
        public IActionResult EditViewCase(ViewCaseModel userProfile)
        {
            try
            {

                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                int requestId = (int)userProfile.RequestId;
                if (requestId != null)
                {
                    Request rid = _adminInterface.ValidateRequest(requestId);

                    RequestClient userToUpdate = _adminInterface.ValidateRequestClient(rid.RequestClientId);
                    if (userToUpdate != null)
                    {
                        _adminInterface.EditViewCaseAction(userProfile, userToUpdate);
                    }
                }
                TempData["success"] = "Case data updated successfully";
                return RedirectToAction("ViewCase", new { id = requestId });
            }

            catch (Exception ex)
            {
                int requestId = (int)userProfile.RequestId;
                TempData["error"] = "Unable to edit the case information";
                return RedirectToAction("ViewCase", new { id = requestId });
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        // action to show data in View Notes view
        public IActionResult ViewNotes(int id)
        {
            try
            {
                Request requ = _adminInterface.GetReqFromReqId(id);
                if (requ == null)
                {
                    TempData["error"] = "No such request exists";
                    return RedirectToAction("PageNotFoundError");
                }

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
                    Physician ph = _adminInterface.GetPhysicianFromId((int)userId);
                    Request re = _adminInterface.GetReqFromReqId(id);
                    if (re == null)
                    {
                        TempData["error"] = "Request does not exist";
                        return RedirectToAction("AdminDashboard");
                    }
                    if (re.PhysicianId != ph.PhysicianId)
                    {
                        TempData["error"] = "Notes of unassigned case cannot be accessed";
                        return RedirectToAction("AdminDashboard");
                    }
                }
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                Request r = _adminInterface.ValidateRequest(id);

                RequestNote rn = _adminInterface.FetchRequestNote(id);


                List<RequestStatusLog> rs = _adminInterface.GetAllRslData(id);
                string adNotes = " ";
                string phNotes = " ";
                string tNotes = " ";
                if (rn != null)
                {
                    adNotes = rn.AdminNotes;
                    phNotes = rn.PhysicianNotes;
                }
                foreach (var item in rs)
                {
                    if (item != null && item.Status == 2)
                    {
                        tNotes = tNotes + " " + item.Notes + ", ";
                    }
                }

                RequestStatusLog rsl = _adminInterface.FetchRequestStatusLogs(id);
                string name = "";
                if (rsl != null && rsl.PhysicianId != null)
                {
                    int pid = (int)rsl.PhysicianId;
                    Physician py = _adminInterface.FetchPhysician(pid);
                    name = py.FirstName;
                }
                string cancelledByAdmin = _adminInterface.GetCancelledByAdminNotes(id);
                string cancelledByPatient = _adminInterface.GetCancelledByPatientNotes(id);
                var viewModel = new ViewNotes
                {
                    AdminNotes = adNotes,
                    PhysicianNotes = phNotes,
                    cancelledByAdminNotes = cancelledByAdmin,
                    cancelledByPatientNotes = cancelledByPatient,
                    PhyName = name,
                    Notes = tNotes,
                    CreatedDate = rsl == null ? DateTime.Now : rsl.CreatedDate,
                    RequestId = id,
                    an = an,
                };
                return View(viewModel);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view notes";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        // function to store edited information of View Notes view in database
        public IActionResult EditViewNotes(ViewNotes model)
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
                model.an = an;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                RequestNote rn = _adminInterface.FetchRequestNote(model.RequestId);

                _adminInterface.EditViewNotesAction(model, (int)userId);
                TempData["success"] = "Notes edited successfully";
                return RedirectToAction("ViewNotes", new { id = model.RequestId });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to edit the notes";
                return RedirectToAction("ViewNotes", new { id = model.RequestId });
            }

        }

        [HttpPost]
        [CustomAuthorize("Admin", "AdminDashboard")]
        // function called when we cancel the case OR submit the cancel case modal
        public IActionResult CancelCase(AdminDashboardTableView model, int selectedCaseTagId, string additionalNotes)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                CaseTag ct = _adminInterface.FetchCaseTag(selectedCaseTagId);

                Request r = _adminInterface.ValidateRequest(model.RequestId);
                r.CaseTag = ct.Name;
                r.Status = 3;
                _adminInterface.UpdateRequest(r);

                RequestStatusLog rs = new RequestStatusLog();
                rs.RequestId = model.RequestId;
                rs.Notes = additionalNotes;
                rs.Status = 3;
                rs.CreatedDate = DateTime.Now;

                _adminInterface.AddRequestStatusLogFromCancelCase(rs);
                TempData["success"] = "Case cancelled successfully";

                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to cancel the case";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "AdminDashboard")]
        // function called when we want to fetch all the physicians belonging to a certain region as given by RegionId
        public List<Physician> GetPhysicianByRegion(int RegionId)
        {
            List<Physician> ph = new List<Physician> { new Physician() };
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                List<Physician> p = _adminInterface.FetchPhysicianByRegion(RegionId);
                return p;
            }
            catch (Exception ex)
            {
                // Log the exception here
                Console.WriteLine("Error in GetPhysicianByRegion: " + ex.Message);

                TempData["error"] = "Unable to fetch physicians";
                return ph;
            }
        }

        [CustomAuthorize("Admin", "AdminDashboard")]
        public int sendAgreement2(int reqId)
        {
            int x = 0;

            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                Request r = _adminInterface.GetReqFromReqType(reqId);
                return r.RequestTypeId;
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to send the agreement";
                return x;
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "AdminDashboard")]
        // function called when we assign the case OR submit the Assign Case modal
        public IActionResult AssignCaseSubmitAction(AdminDashboardTableView model, string assignCaseDescription, int selectedPhysicianId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                Request r = _adminInterface.ValidateRequest(model.RequestId);
                r.Status = 1;
                r.PhysicianId = selectedPhysicianId;

                RequestStatusLog rsl = new RequestStatusLog();
                rsl.RequestId = model.RequestId;
                rsl.Notes = assignCaseDescription;
                rsl.Status = 1;
                rsl.CreatedDate = DateTime.Now;
                rsl.TransToPhysicianId = selectedPhysicianId;
                rsl.PhysicianId = selectedPhysicianId;

                _adminInterface.AddRequestStatusLogFromCancelCase(rsl);
                _adminInterface.UpdateRequest(r);
                TempData["success"] = "Successfully requested to assign the case";
                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to assign the case";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult AcceptCase(AdminDashboardTableView model)
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

                if (_adminInterface.AcceptCase(model.RequestId))
                {
                    TempData["success"] = "Case accepted successfully";
                    return RedirectToAction("AdminDashboard");
                }

                else
                {
                    TempData["error"] = "Case is not accepted";
                    return RedirectToAction("AdminDashboard");
                }
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to accept the case";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult ProviderTransferRequest(AdminDashboardTableView model)
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

                if (_adminInterface.ProviderTransferRequest(model.providerTransferDescription, model.RequestId))
                {
                    TempData["success"] = "Successfully requested to transfer the case";
                    return RedirectToAction("AdminDashboard");
                }

                else
                {
                    TempData["error"] = "Transfer request unsuccessful";
                    return RedirectToAction("AdminDashboard");
                }
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to accept the case";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult SaveCallType(int requestid, int Calltype)
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
                int x = _adminInterface.SelectCallTypeOfRequest(requestid, Calltype);
                if (x == 0)
                {
                    TempData["error"] = "Unable to select call type of request";
                }
                else if (x == 1)
                {
                    TempData["success"] = "Call type of request assigned successfully";
                }
                else
                {
                    TempData["success"] = "Call type of request assigned successfully";
                }
                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to select call type of request";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult ActiveToConclude(int id)
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
                bool isConcluded = _adminInterface.ActiveToConclude(id);
                if (isConcluded)
                {
                    TempData["success"] = "Request transferred to conclude state";
                }
                else
                {
                    TempData["error"] = "Unable to transfer the request to conclude state";
                }
                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to transfer the request to conclude state";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "AdminDashboard")]
        // function called when we transfer the case OR when we submit the transfer case modal
        public IActionResult TransferCaseSubmitAction(AdminDashboardTableView model, string assignCaseDescription, int selectedPhysicianId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                Request r = _adminInterface.ValidateRequest(model.RequestId);
                r.Status = 2; //when a case is assigned, status is set to 1 currently
                              // but when the assigned case gets accepted, then its status can be 2 and will be shown in Pending state.
                r.PhysicianId = selectedPhysicianId;

                RequestStatusLog rsl = new RequestStatusLog();
                rsl.RequestId = model.RequestId;
                rsl.Notes = assignCaseDescription;
                rsl.Status = 2;
                rsl.CreatedDate = DateTime.Now;
                rsl.TransToPhysicianId = selectedPhysicianId;

                _adminInterface.AddRequestStatusLogFromCancelCase(rsl);
                _adminInterface.UpdateRequest(r);
                TempData["success"] = "Case transferred!!";
                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to transfer the case";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "AdminDashboard")]
        // function when we clear a case OR when we submit the clear case modal
        public IActionResult ClearCaseSubmitAction(AdminDashboardTableView model)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                Request r = _adminInterface.GetReqFromModel(model);
                if (r != null)
                {
                    r.Status = 10;
                    TempData["success"] = "Case cleared successfully";
                    _adminInterface.UpdateRequest(r);
                }
                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to clear the case";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "AdminDashboard")]
        public IActionResult BlockCase(AdminDashboardTableView model, string reasonForBlockRequest)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                Request r = _adminInterface.ValidateRequest(model.RequestId);
                r.Status = 11;
                _adminInterface.UpdateRequest(r);

                RequestClient rc = _adminInterface.GetRequestClientFromId(r.RequestClientId);

                RequestStatusLog rs = new RequestStatusLog();
                rs.Status = 11;
                rs.CreatedDate = DateTime.Now;
                rs.Notes = reasonForBlockRequest;
                rs.RequestId = model.RequestId;
                _adminInterface.AddRequestStatusLogFromCancelCase(rs);

                BlockRequest br = new BlockRequest();
                br.RequestId = model.RequestId;
                br.Email = r.Email;
                br.PhoneNumber = rc.PhoneNumber;
                br.IsActive = new BitArray(1, true);
                br.Reason = reasonForBlockRequest;
                br.CreatedDate = DateTime.Now;
                _adminInterface.AddBlockRequestData(model.RequestId, rc.PhoneNumber, rc.Email, reasonForBlockRequest);
                TempData["success"] = "Case blocked successfully";
                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to block the case";
                return RedirectToAction("AdminDashboard");
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

        [HttpPost]
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        // function called when admin creates the request for a patient
        public async Task<IActionResult> CreateRequest(AdminCreateRequestModel model)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                int x = 0;
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                    x = ad.AspNetUserId;
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                    x = (int)p.AspNetUserId;
                }
                an.Tab = 1;
                model.adminNavbarModel = an;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;


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
                                    Subject = "Create account for patient " + model.FirstName,
                                    IsBodyHtml = true,
                                    Body = $"<h3>Hey {model.FirstName + " " + model.LastName}</h3><br> Please click the following link to reset your password:<br> <a href='{resetLink}'>Click Here</a>"
                                };
                                mailMessage.To.Add(model.Email);

                                await client.SendMailAsync(mailMessage);

                                isEmailSent = true;
                                DateTime temp = DateTime.Now;
                                if (ad != null)
                                {
                                    _adminInterface.AddEmailLog(body, subject, model.Email, 3, null, null, null, x, null, temp, isEmailSent, emailSentCount);
                                }
                                else
                                {
                                    _adminInterface.AddEmailLog(body, subject, model.Email, 3, null, null, null, null, x, temp, isEmailSent, emailSentCount);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (emailSentCount >= 3)
                                {
                                    DateTime temp = DateTime.Now;
                                    if (ad != null)
                                    {
                                        _adminInterface.AddEmailLog(body, subject, model.Email, 3, null, null, null, x, null, temp, false, emailSentCount);
                                    }
                                    else
                                    {
                                        _adminInterface.AddEmailLog(body, subject, model.Email, 3, null, null, null, null, x, temp, false, emailSentCount);
                                    }
                                }
                                emailSentCount++;
                                ModelState.AddModelError("Email", "Invalid Email");
                                return RedirectToAction("PatientSite");
                            }
                        }
                    }
                    var region = _adminInterface.ValidateRegion(model);
                    if (region == null)
                    {
                        ModelState.AddModelError("State", "Currently we are not serving in this region");
                        return View(model);
                    }

                    var blockedUser = _adminInterface.ValidateBlockRequest(model);
                    if (blockedUser != null)
                    {
                        ModelState.AddModelError("Email", "This patient is blocked.");
                        return View(model);
                    }

                    var existingUser = _adminInterface.ValidateAspNetUser(model);
                    _adminInterface.InsertDataOfRequest(model, x);
                    TempData["success"] = "Request created successfully";
                    return RedirectToAction("AdminDashboard");
                }

                else
                {
                    TempData["error"] = "Unable to create the request";
                    return RedirectToAction("AdminDashboard");
                }
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the request";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        // function to check whether the entered state name belongs to the areas the service is available
        public IActionResult VerifyLocation(string state)
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

                if (state == null)
                {
                    return Json(new { isVerified = 3 });
                }
                bool isVerified = _adminInterface.VerifyLocation(state);
                if (isVerified)
                {
                    return Json(new { isVerified = 1 });
                }
                else
                {
                    return Json(new { isVerified = 2 });
                }
            }

            catch
            {
                TempData["error"] = "Unable to verify the license";
                return RedirectToAction("CreateRequest");
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        // function to return View of Create Request
        public IActionResult CreateRequest()
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
            AdminCreateRequestModel model = new AdminCreateRequestModel
            {
                adminNavbarModel = an,
                FirstName = "",
                LastName = "",
                Email = "",
                PhoneNumber = "",
                DOB = new DateOnly(),
                Street = "",
                City = "",
                State = ""
            };
            string token = Request.Cookies["token"];
            string roleIdVal = _jwtToken.GetRoleId(token);
            List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
            ViewBag.Menu = menus;
            return View(model);
        }

        public IActionResult PlatformLoginPage()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        // function to return View of View Uploads page
        public IActionResult ViewUploads(int id)
        {
            try
            {
                Request r = _adminInterface.GetReqFromReqId(id);
                if (r == null)
                {
                    TempData["error"] = "No such request exists";
                    return RedirectToAction("PageNotFoundError");
                }

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
                    Physician ph = _adminInterface.GetPhysicianFromId((int)userId);
                    Request re = _adminInterface.GetReqFromReqId(id);
                    if (re == null)
                    {
                        TempData["error"] = "Request does not exist";
                        return RedirectToAction("AdminDashboard");
                    }
                    if (re.PhysicianId != ph.PhysicianId)
                    {
                        TempData["error"] = "Documents of unassigned case cannot be accessed";
                        return RedirectToAction("AdminDashboard");
                    }
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
                    var filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", model.ImageContent.FileName);
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

                return RedirectToAction("ViewUploads", new { id = model.requestId });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to upload the file";
                return RedirectToAction("ViewUploads", new { id = model.requestId });
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
                return RedirectToAction("ViewUploads", new { id = reqId });
            }

            catch (Exception ex)
            {
                int reqId = _adminInterface.SingleDelete(id);
                TempData["error"] = "Unable to delete this file";
                return RedirectToAction("ViewUploads", new { id = reqId });
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        // function to delete multiple files from View Uploads view
        public IActionResult DeleteMultiple(int requestid, string fileId)
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
                _adminInterface.MultipleDelete(requestid, fileId);
                TempData["success"] = "File(s) deleted successfully";
                return RedirectToAction("ViewUploads", new { id = requestid });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to delete these files";
                return RedirectToAction("ViewUploads", new { id = requestid });
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        // (IMPORTANT) function to send selected files in mail from View Uploads view
        public IActionResult SendSelectedFiles(int requestid, string fileName)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            Physician p = _adminInterface.GetPhysicianFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            int x = 0;
            if (ad != null)
            {
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.roleName = "Admin";
                x = ad.AspNetUserId;
            }
            else
            {
                an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                an.roleName = "Provider";
                x = (int)p.AspNetUserId;
            }
            an.Tab = 1;
            string token = Request.Cookies["token"];
            string roleIdVal = _jwtToken.GetRoleId(token);
            List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
            ViewBag.Menu = menus;
            try
            {
                int emailSentCount = 1;
                Request r = _adminInterface.ValidateRequest(requestid);
                RequestClient rc = _adminInterface.ValidateRequestClient(r.RequestClientId);
                string name = rc.FirstName + " " + rc.LastName;
                bool isEmailSent = false;
                string resetToken = Guid.NewGuid().ToString();
                string subject = "HalloDoc - Create your account";
                string platformTitle = "HalloDoc";
                string resetLink = $"{Request.Scheme}://{Request.Host}/Login/CreatePatientAccount";
                var body = $"<h2>Documents</h2><p>Here are the documents uploaded for the request of Patient: {name}</p><br /><br />Regards,<br/>{platformTitle}<br/>";
                string senderEmail = "tatva.dotnet.ishanbhatt@outlook.com";
                string senderPassword = "Ishan@1503";

                string[] files = fileName.Split(',').Select(x => x.Trim()).ToArray();
                Request req = _adminInterface.GetRequestWithUser(requestid);

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
                        var user = _adminInterface.ValidAspNetUser(rc.Email);
                        foreach (var file in files)
                        {
                            var filePath = System.IO.Path.Combine("wwwroot/uploads", file);
                            var attachment = new Attachment(filePath);
                            mailMessage.Attachments.Add(attachment);
                        }
                        if (user != null)
                        {
                            mailMessage.To.Add(rc.Email);
                            client.SendMailAsync(mailMessage);
                            isEmailSent = true;
                            DateTime temp = DateTime.Now;
                            if (ad != null)
                            {
                                _adminInterface.AddEmailLog(body, subject, rc.Email, 3, fileName, req.ConfirmationNumber, requestid, x, null, temp, isEmailSent, emailSentCount);
                            }
                            else
                            {
                                _adminInterface.AddEmailLog(body, subject, rc.Email, 3, fileName, req.ConfirmationNumber, requestid, null, x, temp, isEmailSent, emailSentCount);
                            }
                            TempData["success"] = "Mail sent successfully";
                            return RedirectToAction("ViewUploads", new { requestID = requestid });
                        }
                        else
                        {
                            ModelState.AddModelError("Email", "Invalid Email");
                            TempData["error"] = "Unable to send the mail";
                            return RedirectToAction("ViewUploads", new { requestID = requestid });
                        }
                    }

                    catch (Exception ex)
                    {
                        if (emailSentCount >= 3)
                        {
                            DateTime temp = DateTime.Now;
                            if (ad != null)
                            {
                                _adminInterface.AddEmailLog(body, subject, rc.Email, 3, fileName, req.ConfirmationNumber, requestid, x, null, temp, false, emailSentCount);
                            }
                            else
                            {
                                _adminInterface.AddEmailLog(body, subject, rc.Email, 3, fileName, req.ConfirmationNumber, requestid, null, x, temp, false, emailSentCount);
                            }
                        }
                        emailSentCount++;
                        return RedirectToAction("AdminDashboard");
                    }
                }
                TempData["success"] = "Mail sent successfully";
                return RedirectToAction("ViewUploads", new { id = requestid });
            }
            catch (Exception ex)
            {
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "AdminDashboard")]
        public async Task<IActionResult> SendLink(AdminDashboardTableView model)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            Physician p = _adminInterface.GetPhysicianFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            int x = 0;
            if (ad != null)
            {
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.roleName = "Admin";
                x = ad.AspNetUserId;
            }
            else
            {
                an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                an.roleName = "Provider";
                x = (int)p.AspNetUserId;
            }
            an.Tab = 1;
            string token = Request.Cookies["token"];
            string roleIdVal = _jwtToken.GetRoleId(token);
            List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
            ViewBag.Menu = menus;

            int emailCount = 1;
            int smsCount = 1;
            bool isEmailSent = false;
            bool isSMSSent = false;
            try
            {
                while (smsCount <= 3 && !isSMSSent)
                {
                    string messageSMS = $@"Please Request/Register your case. Hii {model.FirstName},hope you are fine, to register your case, please fill the Request Form.Your Request Form,please fill all the required details, for better assistance:https://localhost:44379/Login/SubmitRequest";

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
                            _adminInterface.AddSmsLogFromSendLink(messageSMS, num, null, temp, smsCount, isSMSSent, 1);
                        }
                        smsCount++;
                    }
                }

                string subject = "HalloDoc - Submit a request";
                string platformTitle = "HalloDoc";
                string resetToken = Guid.NewGuid().ToString();
                string resetLink = $"{Request.Scheme}://{Request.Host}/Login/SubmitRequestScreen?token={resetToken}";
                var body = $"<h3>Hey {model.FirstName + " " + model.LastName}</h3><br> Please click the following link to reset your password:<br> <a href='{resetLink}'>Click Here</a><br /><br />Regards,<br/>{platformTitle}<br/>";
                string senderEmail = "tatva.dotnet.ishanbhatt@outlook.com";
                string senderPassword = "Ishan@1503";

                while (emailCount <= 3 && !isEmailSent)
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
                            Subject = "Create Request For Patient",
                            IsBodyHtml = true,
                            Body = $"Hey {model.FirstName + " " + model.LastName} !! Please click the following link to submit a request: <a href='{resetLink}'>Click Here</a>"
                        };
                        RequestClient rc = _adminInterface.ValidatePatientEmail(model.email);
                        if (rc != null)
                        {
                            mailMessage.To.Add(model.email);
                            await client.SendMailAsync(mailMessage);
                            isEmailSent = true;
                            DateTime temp = DateTime.Now;
                            if (ad != null)
                            {
                                _adminInterface.AddEmailLog(body, subject, model.email, 3, null, null, null, x, null, temp, isEmailSent, emailCount);
                            }
                            else
                            {
                                _adminInterface.AddEmailLog(body, subject, model.email, 3, null, null, null, null, x, temp, isEmailSent, emailCount);
                            }
                            TempData["success"] = "Message Sent Successfully";
                            return RedirectToAction("AdminDashboard");
                        }
                        else
                        {
                            TempData["error"] = "Invalid email";
                            return RedirectToAction("AdminDashboard");
                        }
                    }

                    catch (Exception ex)
                    {
                        if (emailCount >= 3)
                        {
                            DateTime temp = DateTime.Now;
                            if (ad != null)
                            {
                                _adminInterface.AddEmailLog(body, subject, model.email, 3, null, null, null, x, null, temp, false, emailCount);
                            }
                            else
                            {
                                _adminInterface.AddEmailLog(body, subject, model.email, 3, null, null, null, null, x, temp, false, emailCount);
                            }
                        }
                        emailCount++;
                        TempData["error"] = "Unable to send the email";
                        return RedirectToAction("PatientSite");
                    }
                }
                return RedirectToAction("AdminDashboard");
            }
            catch (Exception ex)
            {
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin Provider", "Orders")]
        // function to return Send Orders view
        public IActionResult Orders(int id)
        {
            try
            {
                Request r = _adminInterface.GetReqFromReqId(id);
                if (r == null)
                {
                    TempData["error"] = "No such request exists";
                    return RedirectToAction("AdminDashboard");
                }

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
                List<HealthProfessionalType> hPT = _adminInterface.GetHealthProfessionalType();
                List<HealthProfessional> hP = _adminInterface.GetHealthProfessional();
                SendOrder so = new SendOrder
                {
                    hpType = hPT,
                    hp = hP,
                    requestId = id,
                    an = an
                };
                return View(so);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view the orders";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin Provider", "Orders")]
        // function to get data of HealthProfessional table in Send Orders view
        public List<HealthProfessional> GetBusinessData(int professionId, SendOrder model)

        {
            List<HealthProfessional> hp = new List<HealthProfessional> { new HealthProfessional() };
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
                List<HealthProfessional> healthProfessionals = _adminInterface.GetBusinessDataFromProfession(professionId);
                return healthProfessionals;
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to fetch business data";
                return hp;
            }
        }

        [CustomAuthorize("Admin Provider", "Orders")]
        // function to get other data based on selected BusinessName in Send Orders view
        public HealthProfessional GetOtherData(int businessId)
        {
            HealthProfessional hp2 = new HealthProfessional();
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
                HealthProfessional hp = _adminInterface.GetOtherDataFromBId(businessId);
                return hp;
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to fetch the data of professional";
                return hp2;
            }
        }
        //[HttpPost]
        [CustomAuthorize("Admin", "Orders")]
        public IActionResult GetAgreementData(int reqId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                RequestClient rc = _adminInterface.GetPatientData(reqId);
                return Json(new { response = rc });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to get the agreement data";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin Provider", "Orders")]
        // function to send order to specified vendor
        public async Task<IActionResult> SendOrder(SendOrder model, int vendorId, int noOfRefill)
        {
            int retryCount = 1;
            bool isSent = false;
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                int x = 0;
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                    x = ad.AspNetUserId;
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                    x = (int)p.AspNetUserId;
                }
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                while (retryCount <= 3 && !isSent)
                {
                    string senderEmail = "tatva.dotnet.ishanbhatt@outlook.com";
                    string senderPassword = "Ishan@1503";
                    var platformTitle = "HalloDoc";
                    var subject = "Order details";
                    var body = $"Hello, <br />Here are the details you may need to fulfil our order,<br /><br />Order Details: {model.prescription}<br /> Number of refills: {model.numOfRefill}</a><br /><br />Regards,<br/>{platformTitle}<br/>";

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
                            Subject = "Create New Request",
                            IsBodyHtml = true,
                            Body = body
                        };

                        mailMessage.To.Add(model.email);

                        await client.SendMailAsync(mailMessage);


                        isSent = true;
                        DateTime temp = DateTime.Now;
                        if (ad != null)
                        {
                            _adminInterface.AddEmailLog(body, subject, model.email, null, null, null, null, x, null, temp, isSent, retryCount);
                        }
                        else
                        {
                            _adminInterface.AddEmailLog(body, subject, model.email, null, null, null, null, null, x, temp, isSent, retryCount);
                        }
                        break;
                    }

                    catch (Exception e)
                    {
                        if (retryCount >= 3)
                        {
                            DateTime temp = DateTime.Now;
                            if (ad != null)
                            {
                                _adminInterface.AddEmailLog(body, subject, model.email, null, null, null, null, x, null, temp, false, retryCount);
                            }
                            else
                            {
                                _adminInterface.AddEmailLog(body, subject, model.email, null, null, null, null, null, x, temp, false, retryCount);
                            }
                        }
                        retryCount++;
                    }
                }

                int smsSentTries = 1;
                bool isSmsSent = false;

                while (smsSentTries <= 3 && !isSmsSent)
                {
                    string messageSMS = "Hello, Here are the details you may need to fulfil our order. Order details: " + model.prescription + " and Number of refills required: " + noOfRefill;
                    string recipient = "+917990117699";
                    var accountSid = _configuration["Twilio:accountSid"];
                    var authToken = _configuration["Twilio:authToken"];
                    var twilionumber = _configuration["Twilio:twilioNumber"];

                    try
                    {
                        TwilioClient.Init(accountSid, authToken);
                        //var messageBody =
                        var message2 = MessageResource.Create(
                            from: new Twilio.Types.PhoneNumber(twilionumber),
                            body: messageSMS,
                            to: new Twilio.Types.PhoneNumber(recipient)
                        );

                        isSmsSent = true;
                        DateTime temp = DateTime.Now;
                        _adminInterface.AddSmsLogFromSendOrder(messageSMS, recipient, null, temp, smsSentTries, isSmsSent, 2);
                        break;
                    }

                    catch (Exception ex)
                    {
                        if (smsSentTries >= 3)
                        {
                            DateTime temp = DateTime.Now;
                            _adminInterface.AddSmsLogFromSendOrder(messageSMS, recipient, null, temp, smsSentTries, isSmsSent, 2);
                        }
                        smsSentTries++;
                    }
                }



                OrderDetail orderDetail = new OrderDetail();
                orderDetail.VendorId = vendorId;
                orderDetail.RequestId = model.requestId;
                orderDetail.FaxNumber = model.faxNumber;
                orderDetail.Email = model.email;
                orderDetail.BusinessContact = model.businessContact;
                orderDetail.Prescription = model.prescription;
                orderDetail.NoOfRefill = noOfRefill;
                orderDetail.CreatedDate = DateTime.Now;
                _adminInterface.AddOrderDetails(orderDetail);

                TempData["success"] = "Order sent successfully";
                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to send the orders";
                return RedirectToAction("AdminDashboard");
            }
        }


        public async Task<IActionResult> SendMailForSetUpAccount(LoginViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
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
                string resetLink = $"{Request.Scheme}://{Request.Host}/Admin/CreatePassword?token={resetToken}";

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
                var user = _adminInterface.ValidateAspNetUser(model);
                if (user != null)
                {
                    mailMessage.To.Add(model.UserName);
                    _sescontext.HttpContext.Session.SetString("Token", resetToken);
                    _sescontext.HttpContext.Session.SetString("UserEmail", model.UserName);
                    await client.SendMailAsync(mailMessage);
                    TempData["success"] = "Mail sent successfully. Please check it";
                    return RedirectToAction("PlatfromLoginPage");
                }
                else
                {
                    ModelState.AddModelError("Email", "Invalid Email");
                    return RedirectToAction("PlatformForgotPassword");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("PlatformForgotPassword");
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        // function to send mail of agreement to particular AspNetUser based on RequestClient's Email
        public async Task<IActionResult> SendMailOfAgreement(AdminDashboardTableView model)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            Physician p = _adminInterface.GetPhysicianFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            int x = 0;
            if (ad != null)
            {
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.roleName = "Admin";
                x = ad.AspNetUserId;
            }
            else
            {
                an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                an.roleName = "Provider";
                x = (int)p.AspNetUserId;
            }
            an.Tab = 1;
            string token = Request.Cookies["token"];
            string roleIdVal = _jwtToken.GetRoleId(token);
            List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
            ViewBag.Menu = menus;

            Request r = _adminInterface.GetRequestWithUser(model.RequestId);
            string email = _adminInterface.GetMailToSentAgreement(model.RequestId);
            RequestClient rc = _adminInterface.GetPatientData(model.RequestId);
            string resetToken = Guid.NewGuid().ToString();
            string url = $"{Request.Scheme}://{Request.Host}/Admin/ReviewAgreement?id={r.RequestId}";
            int emailSentCount = 1;
            bool isEmailSent = false;
            string senderEmail = "tatva.dotnet.ishanbhatt@outlook.com";
            string senderPassword = "Ishan@1503";
            string subject = "HalloDoc - Review Agreement";
            string platformTitle = "HalloDoc";
            var body = $"Please click the following link to reset your password: <br><br><a href='{url}'>Click Here</a><br /><br />Regards,<br/>{platformTitle}<br/>";

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
                        Subject = "Review the agreement",
                        IsBodyHtml = true,
                        Body = body,
                    };


                    mailMessage.To.Add(model.sendAgreeEmail);
                    _sescontext.HttpContext.Session.SetString("UserEmail", model.sendAgreeEmail);
                    await client.SendMailAsync(mailMessage);
                    isEmailSent = true;
                    DateTime temp = DateTime.Now;
                    if (ad != null)
                    {
                        _adminInterface.AddEmailLog(body, subject, model.sendAgreeEmail, 3, null, r.ConfirmationNumber, model.RequestId, x, null, temp, isEmailSent, emailSentCount);
                    }
                    else
                    {
                        _adminInterface.AddEmailLog(body, subject, model.sendAgreeEmail, 3, null, r.ConfirmationNumber, model.RequestId, null, x, temp, isEmailSent, emailSentCount);
                    }
                    TempData["success"] = "Mail sent successfully. Please check it";



                }
                catch (Exception ex)
                {
                    if (emailSentCount >= 3)
                    {
                        DateTime temp = DateTime.Now;
                        if (ad != null)
                        {
                            _adminInterface.AddEmailLog(body, subject, model.sendAgreeEmail, 3, null, r.ConfirmationNumber, model.RequestId, x, null, temp, false, emailSentCount);
                        }
                        else
                        {
                            _adminInterface.AddEmailLog(body, subject, model.sendAgreeEmail, 3, null, r.ConfirmationNumber, model.RequestId, null, x, temp, false, emailSentCount);
                        }
                    }
                    emailSentCount++;
                    TempData["error"] = "Failed to send the agreement to the provided mail";
                }
            }
            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        public IActionResult PlatformCreatePassword(string token)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            var useremail = _sescontext.HttpContext.Session.GetString("Token");
            PasswordReset pr = _adminInterface.ValidateToken(token);

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
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            var useremail = _sescontext.HttpContext.Session.GetString("UserEmail");
            AspNetUser user = _adminInterface.ValidateUserForResetPassword(model, useremail);
            if (user != null && model.Password == model.ConfirmPassword)
            {
                //user.PasswordHash = model.Password;
                //_context.SaveChanges();
                _adminInterface.SetPasswordForResetPassword(user, model);
                return RedirectToAction("PlatformLoginPage");
            }
            else
            {
                ModelState.AddModelError("Password", "Password Missmatched");
                return RedirectToAction("PlatformForgotPassword");
            }

        }

        // function to return Review Agreement view
        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult ReviewAgreement(int id)
        {
            try
            {



                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Request r = _adminInterface.GetReqFromReqId(id);
                if (r.Status != 2)
                {
                    TempData["error"] = "unable to review the agreement";
                    return RedirectToAction("PatientLoginPage", "Login");
                }
                RequestClient rc = _adminInterface.GetPatientData(id);
                return View(rc);
            }

            catch (Exception ex)
            {
                TempData["error"] = "unable to review the agreement";
                return RedirectToAction("PatientLoginPage", "Login");
            }
        }

        [HttpPost]
        // function called when a patient accepts the agreement
        public IActionResult AcceptAgreement(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;

                Request r = _adminInterface.GetReqFromReqClient(id);
                r.Status = 4;
                _adminInterface.UpdateRequest(r);

                RequestStatusLog rsl = new RequestStatusLog();
                rsl.RequestId = r.RequestId;
                rsl.Status = 4;
                rsl.CreatedDate = DateTime.Now;
                rsl.RequestId = r.RequestId;
                _adminInterface.AddRequestStatusLogFromAgreement(rsl);

                TempData["success"] = "Agreement accepted successfully";
                return RedirectToAction("PatientLoginPage", "Login");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to accept the agreement";
                return RedirectToAction("PatientLoginPage", "Login");
            }
        }

        [HttpPost]
        // function called when a patient cancels the agreement
        public IActionResult CancelAgreement(int id, string desc)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;

                Request r = _adminInterface.GetReqFromReqClient(id);
                r.Status = 7;
                _adminInterface.UpdateRequest(r);

                RequestStatusLog rsl = new RequestStatusLog();
                rsl.Status = 7;
                rsl.Notes = desc;
                rsl.CreatedDate = DateTime.Now;
                rsl.RequestId = r.RequestId;
                _adminInterface.AddRequestStatusLogFromCancelCase(rsl);

                TempData["success"] = "Agreement cancelled successfully";
                return RedirectToAction("PatientLoginPage", "Login");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to cancel the agreement";
                return RedirectToAction("PatientLoginPage", "Login");
            }
        }

        public IActionResult PlatformCreatePassword()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            return View();
        }

        [CustomAuthorize("Admin Provider", "EncounterForm")]
        // function to return Encounter Form view
        public IActionResult EncounterForm(int id)
        {
            try
            {
                Request requ = _adminInterface.GetReqFromReqId(id);
                if (requ == null)
                {
                    TempData["error"] = "No such request exists";
                    return RedirectToAction("PageNotFoundError");
                }

                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                Physician p = _adminInterface.GetPhysicianFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.roleName = "Admin";
                    ViewBag.x = 1;
                }
                else
                {
                    an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                    an.roleName = "Provider";
                    Physician ph = _adminInterface.GetPhysicianFromId((int)userId);
                    Request re = _adminInterface.GetReqFromReqId(id);
                    if (re == null)
                    {
                        TempData["error"] = "Request does not exist";
                        return RedirectToAction("AdminDashboard");
                    }
                    if (re.PhysicianId != ph.PhysicianId)
                    {
                        TempData["error"] = "Encounter form of unassigned case cannot be accessed";
                        return RedirectToAction("AdminDashboard");
                    }
                    ViewBag.x = 2;
                }
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                EncounterForm ef = _adminInterface.GetEncounterFormData(id);
                Request r = _adminInterface.ValidateRequest(id);
                RequestClient rc = _adminInterface.ValidateRequestClient(r.RequestClientId);

                if (ef != null)
                {
                    EncounterFormModel efm = new EncounterFormModel();
                    efm.reqId = id;
                    efm.FirstName = rc.FirstName;
                    efm.LastName = rc.LastName;
                    efm.Email = rc.Email;
                    efm.Location = string.Concat(rc.Street, ", ", rc.City, ", ", rc.State, ", ", rc.ZipCode);
                    efm.PhoneNumber = rc.PhoneNumber;
                    efm.DOB = new DateTime((int)rc.IntYear, int.Parse(rc.StrMonth), (int)rc.IntDate);
                    efm.Date = ef.Date;
                    efm.Medications = ef.Medications;
                    efm.Allergies = ef.Allergies;
                    efm.Temp = (decimal)ef.Temp;
                    efm.HR = (decimal)ef.Hr;
                    efm.RR = (decimal)ef.Rr;
                    efm.BPS = (int)ef.BpS;
                    efm.BPD = (int)ef.BpD;
                    efm.O2 = (decimal)ef.O2;
                    efm.Pain = ef.Pain;
                    efm.Heent = ef.Heent;
                    efm.CV = ef.Cv;
                    efm.Chest = ef.Chest;
                    efm.ABD = ef.Abd;
                    efm.Extr = ef.Extr;
                    efm.Skin = ef.Skin;
                    efm.Neuro = ef.Neuro;
                    efm.Other = ef.Other;
                    efm.Diagnosis = ef.Diagnosis;
                    efm.TreatmentPlan = ef.TreatmentPlan;
                    efm.MedicationsDispensed = ef.MedicationDispensed;
                    efm.Procedures = ef.Procedures;
                    efm.FollowUp = ef.FollowUp;
                    efm.an = an;

                    return View(efm);
                }

                else
                {
                    EncounterFormModel efm1 = new EncounterFormModel
                    {
                        reqId = id,
                        FirstName = rc.FirstName,
                        LastName = rc.LastName,
                        Email = rc.Email,
                        Location = string.Concat(rc.Street, ", ", rc.City, ", ", rc.State, ", ", rc.ZipCode),
                        PhoneNumber = rc.PhoneNumber,
                        DOB = new DateTime((int)rc.IntYear, int.Parse(rc.StrMonth), (int)rc.IntDate),
                        an = an,
                    };
                    return View(efm1);
                }
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view encounter form";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin Provider", "EncounterForm")]
        // function called when we submit the encounter form
        public IActionResult EncounterFormSubmit(EncounterFormModel model, int id, int bitCheck)
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
                bool isFinalized = false;
                int requestId = (int)model.reqId;
                if (requestId != null)
                {
                    Request r = _adminInterface.ValidateRequest(requestId);
                    RequestClient rc = _adminInterface.ValidateRequestClient(r.RequestClientId);
                    if (rc != null)
                    {
                        isFinalized = _adminInterface.FinalizeEncounterForm(model, rc, id, bitCheck);

                    }
                }

                if (!isFinalized)
                {
                    TempData["success"] = "Form saved successfully";
                }
                else
                {
                    TempData["success"] = "Form finalized successfully";
                }
                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to submit the encounter form data";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin Provider", "EncounterForm")]
        public IActionResult FinalizeEncounterForm(EncounterFormModel model, int id, int bitCheck)
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
                Request r = _adminInterface.ValidateRequest(id);
                RequestClient rc = _adminInterface.ValidateRequestClient(r.RequestClientId);
                if (_adminInterface.FinalizeEncounterForm(model, rc, id, bitCheck))
                {
                    TempData["success"] = "Form finalized successfully";
                    return RedirectToAction("AdminDashboard");
                }
                else
                {
                    TempData["error"] = "Unable to finalize the encounter form data";
                    return RedirectToAction("AdminDashboard");
                }
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to finalize the encounter form data";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public IActionResult DownloadEncounterForm(int requestid)
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

                EncounterFormModel efm = GetEncounterFormModel(requestid);
                HalloDoc.DataLayer.Models.Request r = _adminInterface.GetReqFromReqId(requestid);

                return new ViewAsPdf("../Shared/_Encounter", efm)
                {
                    FileName = $"EncounterReport-{r.ConfirmationNumber}.pdf",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    PageMargins = { Left = 20, Right = 20 }
                };
            }

            catch (Exception ex)
            {
                TempData["error"] = "Cannot download the file";
                return View("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin Provider", "AdminDashboard")]
        public EncounterFormModel GetEncounterFormModel(int reqId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            Physician p = _adminInterface.GetPhysicianFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            if (ad != null)
            {
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.roleName = "Admin";
                ViewBag.x = 1;
            }
            else
            {
                an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                an.roleName = "Provider";
                ViewBag.x = 2;
            }
            an.Tab = 1;
            string token = Request.Cookies["token"];
            string roleIdVal = _jwtToken.GetRoleId(token);
            List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
            ViewBag.Menu = menus;

            EncounterForm ef = _adminInterface.GetEncounterFormData(reqId);
            Request r = _adminInterface.ValidateRequest(reqId);
            RequestClient rc = _adminInterface.ValidateRequestClient(r.RequestClientId);

            if (ef != null)
            {


                EncounterFormModel efm = new EncounterFormModel
                {
                    reqId = reqId,
                    FirstName = rc.FirstName,
                    LastName = rc.LastName,
                    Email = rc.Email,
                    Location = string.Concat(rc.Street, ", ", rc.City, ", ", rc.State, ", ", rc.ZipCode),
                    PhoneNumber = rc.PhoneNumber,
                    DOB = new DateTime((int)rc.IntYear, int.Parse(rc.StrMonth), (int)rc.IntDate),
                    Date = (DateTime)ef.Date,
                    Medications = ef.Medications,
                    Allergies = ef.Allergies,
                    Temp = (decimal)ef.Temp,
                    HR = (decimal)ef.Hr,
                    RR = (decimal)ef.Rr,
                    BPS = (int)ef.BpS,
                    BPD = (int)ef.BpD,
                    O2 = (decimal)ef.O2,
                    Pain = ef.Pain,
                    Heent = ef.Heent,
                    CV = ef.Cv,
                    Chest = ef.Chest,
                    ABD = ef.Abd,
                    Extr = ef.Extr,
                    Skin = ef.Skin,
                    Neuro = ef.Neuro,
                    Other = ef.Other,
                    Diagnosis = ef.Diagnosis,
                    TreatmentPlan = ef.TreatmentPlan,
                    MedicationsDispensed = ef.MedicationDispensed,
                    Procedures = ef.Procedures,
                    FollowUp = ef.FollowUp,
                    an = an
                };
                return efm;
            }

            else
            {
                EncounterFormModel efm1 = new EncounterFormModel
                {
                    reqId = reqId,
                    FirstName = rc.FirstName,
                    LastName = rc.LastName,
                    Email = rc.Email,
                    Location = string.Concat(rc.Street, ", ", rc.City, ", ", rc.State, ", ", rc.ZipCode),
                    PhoneNumber = rc.PhoneNumber,
                    DOB = new DateTime((int)rc.IntYear, int.Parse(rc.StrMonth), (int)rc.IntDate),
                    an = an,
                };
                return efm1;
            }
        }

        [CustomAuthorize("Admin", "AdminDashboard")]
        // function to return view of close case
        public IActionResult CloseCase(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                Request request = _adminInterface.ValidateRequest(id);

                User user = _adminInterface.ValidateUserByRequestId(request);

                List<RequestWiseFile> rwf = _adminInterface.GetFileData(id);

                RequestClient rc = _adminInterface.GetRequestClientFromId(request.RequestClientId);

                CloseCaseModel cc = new CloseCaseModel();
                cc.firstName = rc.FirstName;
                cc.lastName = rc.LastName;
                cc.fullName = rc.FirstName + " " + rc.LastName;
                cc.conf_no = request.ConfirmationNumber;
                cc.phoneNumber = rc.PhoneNumber;
                cc.email = rc.Email;
                cc.DOB = new DateOnly((int)rc.IntYear, int.Parse(rc.StrMonth), (int)rc.IntDate);
                cc.reqId = id;
                cc.requestWiseFiles = rwf;
                cc.an = an;
                return View(cc);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to close the case";
                return RedirectToAction("AdminDashboard");
            }
        }


        // function called when we case is closed
        public IActionResult ClickOnCloseCase(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                Request r = _adminInterface.ValidateRequest(id);
                r.Status = 9;
                _adminInterface.UpdateRequest(r);

                RequestStatusLog rsl = new RequestStatusLog();
                rsl.RequestId = id;
                rsl.CreatedDate = DateTime.Now;
                rsl.Status = 9;
                _adminInterface.AddRequestStatusLogFromCancelCase(rsl);

                RequestClosed rc = new RequestClosed();
                rc.RequestStatusLogId = rsl.RequestStatusLogId;
                rc.RequestId = id;
                _adminInterface.AddRequestClosedData(rc);

                TempData["success"] = "Request Closed Successfully";
                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Case is not closed";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "AdminDashboard")]
        // function to store edited info of patient from Close Case view
        public IActionResult CloseCaseSubmitAction(CloseCaseModel model, int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 1;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                Request r = _adminInterface.ValidateRequest(id);

                RequestClient rc = _adminInterface.ValidateRequestClient(r.RequestClientId);
                rc.Email = model.email;
                rc.PhoneNumber = model.phoneNumber;
                _adminInterface.UpdateRequestClient(rc);

                TempData["success"] = "Updated data";
                return RedirectToAction("CloseCase", new { id });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to edit the information";
                return RedirectToAction("CloseCase", new { id });
            }
        }

        [CustomAuthorize("Admin", "ProviderLocation")]
        public IActionResult ProviderLocation()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 2;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                ProviderLocationViewModel pl = new ProviderLocationViewModel
                {
                    locationData = _adminInterface.GetPhysicianLocation(),
                    adminNavbarModel = an,
                };

                return View(pl);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to get locations of providers";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "MyProfile")]
        // function to return Admin Profile view
        public IActionResult MyProfile()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.Tab = 3;
                    an.roleName = "Admin";
                }
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                AspNetUser anur = _adminInterface.GetAdminDataFromId(ad.AspNetUserId);
                HalloDoc.DataLayer.Models.Region r = _adminInterface.GetRegFromId((int)ad.RegionId);

                AdminProfile ap = new AdminProfile
                {
                    Username = anur.UserName,
                    firstName = ad.FirstName,
                    lastName = ad.LastName,
                    email = ad.Email,
                    confEmail = ad.Email,
                    phone = ad.Mobile,
                    address1 = ad.Address1,
                    address2 = ad.Address2,
                    city = ad.City,
                    state = r.Name,
                    adminId = ad.AdminId,
                    zipcode = ad.Zip,
                    allRegions = _adminInterface.GetAllRegion(),
                    an = an,
                };
                ap.regions = _adminInterface.GetAdminRegionFromId(ad.AdminId);
                ap.regionOfAdmin = _adminInterface.GetAvailableRegionOfAdmin(ad.AdminId);
                return View(ap);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view admin profile";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "MyProfile")]
        // function called when admin resets the password from My Profile page
        public IActionResult ProfilePasswordReset(AdminProfile model, int aid)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.Tab = 11;
                    an.roleName = "Admin";
                }
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                AspNetUser anur = _adminInterface.GetAspNetFromAdminId(aid);
                _adminInterface.AdminResetPassword(anur, model.Password);
                TempData["success"] = "Password Updated Successfully";
                return RedirectToAction("MyProfile");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to reset the password";
                return RedirectToAction("MyProfile");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "MyProfile")]
        // function called to submit the changes made in Administrator Info section of Admin Profile
        public IActionResult ProfileAdministratorInfo(AdminProfile model, int aid, string selectedRegion)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.Tab = 11;
                    an.roleName = "Admin";
                }
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                string[] regionArr = selectedRegion.Split(',');
                char[] rId = selectedRegion.ToCharArray();

                if(!_adminInterface.CheckEmailFromAdminId(aid, model.email))
                {
                    TempData["error"] = "Email already exists in other account";
                    return RedirectToAction("AdminProfileFromUserAccess", new { id = aid});
                }

                _adminInterface.UpdateAdminDataFromId(model, aid, selectedRegion);

                TempData["success"] = "Administrator info updated successfully";
                if (model.an == null)
                {
                    return RedirectToAction("AdminProfileFromUserAccess", new {id = aid});
                }
                else
                {
                    return RedirectToAction("AdminProfileFromUserAccess", new {id = aid});
                }
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to edit the administrator information";
                return RedirectToAction("AdminProfileFromUserAccess", new { id = aid });
            }
        }

        [CustomAuthorize("Admin", "MyProfile")]
        // function called to submit the changes made in Mailing Info of Admin Profile
        public IActionResult ProfileMailingInfo(AdminProfile model, int regId, int aid)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad != null)
                {
                    an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                    an.Tab = 11;
                    an.roleName = "Admin";
                }
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                _adminInterface.UpdateMailingInfo(model, regId, aid);
                TempData["success"] = "Mailing info updated successfully";
                return RedirectToAction("MyProfile");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to edit the mailing info";
                return RedirectToAction("MyProfile");
            }
        }

        [CustomAuthorize("Admin", "UserAccess")]
        public IActionResult AdminProfileFromUserAccess(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad1 = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                if (ad1 != null)
                {
                    an.Admin_Name = string.Concat(ad1.FirstName, " ", ad1.LastName);
                    an.Tab = 11;
                    an.roleName = "Admin";
                }
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                AspNetUser anur = _adminInterface.GetAdminDataFromId(ad1.AspNetUserId);
                HalloDoc.DataLayer.Models.Region r = _adminInterface.GetRegFromId((int)ad1.RegionId);

                Admin ad = _adminInterface.GetAdminFromAdminId(id);

                AdminProfile ap = new AdminProfile
                {
                    Username = anur.UserName,
                    firstName = ad.FirstName,
                    lastName = ad.LastName,
                    email = ad.Email,
                    confEmail = ad.Email,
                    phone = ad.Mobile,
                    address1 = ad.Address1,
                    address2 = ad.Address2,
                    city = ad.City,
                    state = r.Name,
                    adminId = ad.AdminId,
                    zipcode = ad.Zip,
                    allRegions = _adminInterface.GetAllRegion(),
                    an = an,
                    roleId = (int)ad.RoleId,
                    status = (int)ad.Status,
                    roleName = _adminInterface.RoleNameFromId((int)ad.RoleId),
                    altPhoneNo = ad.AltPhone,
                };
                ap.regions = _adminInterface.GetAdminRegionFromId(ad.AdminId);
                ap.regionOfAdmin = _adminInterface.GetAvailableRegionOfAdmin(ad.AdminId);
                return View(ap);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view admin profile";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "UserAccess")]
        public IActionResult EditProviderAccountFromUserAccess(int id)
        {
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "0");
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 11;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                EditProviderAccountViewModel ep = _adminInterface.ProviderEditAccount(id, an);
                return View(ep);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view provider profile";
                return RedirectToAction("UserAccess");
            }
        }



        [CustomAuthorize("Admin", "PatientRecords")]
        // function to return Patient Records view
        public IActionResult PatientRecords(int userid)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 17;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                PatientHistoryViewModel pr = _adminInterface.PatientRecordsData(userid, an);
                return View(pr);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view patient records";
                return RedirectToAction("PatientHistory");
            }
        }

        [CustomAuthorize("Admin", "PatientRecords")]
        public IActionResult PatientRecordsFilteredData(int userid, int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 17;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                PatientHistoryViewModel pr = _adminInterface.PatientRecordsFilteredData(userid, an, page, pageSize);
                return PartialView("PatientRecordsPagePartialView", pr);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view patient records";
                return RedirectToAction("PatientHistory");
            }
        }

        [CustomAuthorize("Admin", "ProviderMenu")]
        // function to return Provider Menu view
        public IActionResult ProviderMenu()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 5;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                ProviderMenuViewModel pm = _adminInterface.ProviderMenuFilteredData(an, null);
                return View(pm);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view menu of providers";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "ProviderMenu")]
        // function to filter the records of Provider Menu view
        public IActionResult ProviderMenuFilter(int? region = -1, int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 5;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                ProviderMenuViewModel pm = _adminInterface.ProviderMenuFilteredData(an, region, page, pageSize);
                return PartialView("ProviderMenuPartialView", pm);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view menu of providers";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "ProviderMenu")]
        // function called when we change the checkbox value of a record of Provider Menu view
        public IActionResult ChangeNotificationValue(int id)
        {
            try
            {
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                _adminInterface.ChangeNotificationValue(id);
                TempData["success"] = "Notification status updated successfully";
                return RedirectToAction("ProviderMenu");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to change the notification status";
                return RedirectToAction("ProviderMenu");
            }
        }

        [CustomAuthorize("Admin", "ProviderMenu")]
        // function called when we submit the Contact Your Provider modal
        public IActionResult SendMessageToPhysician(ProviderMenuViewModel model, string flexRadioDefault, string contactProviderMessage)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 5;
            try
            {
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                int count = 1;
                int smsSentTries = 1;
                string subject = "HalloDoc - Contact Your Provider";
                bool isSMSSent = false;
                bool isSent = false;
                if (flexRadioDefault != "SMS" && (flexRadioDefault == "Email" || flexRadioDefault == "Both"))
                {
                    while (count <= 3 && !isSent)
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



                            MailMessage mailMessage = new MailMessage
                            {
                                From = new MailAddress(senderEmail, "HalloDoc"),
                                Subject = subject,
                                IsBodyHtml = true,
                                Body = $"{contactProviderMessage}"
                            };

                            if (model.email != "")
                            {
                                mailMessage.To.Add(model.email);
                                client.SendMailAsync(mailMessage);
                                isSent = true;
                                DateTime temp = DateTime.Now;
                                _adminInterface.AddEmailLog(contactProviderMessage, subject, model.email, 2, null, null, null, null, (int)model.phyId, temp, isSent, count);
                                break;
                            }

                            else
                            {
                                ModelState.AddModelError("Email", "Invalid Email");
                                return RedirectToAction("ProviderMenu");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (count >= 3)
                            {
                                DateTime temp = DateTime.Now;
                                _adminInterface.AddEmailLog(contactProviderMessage, subject, model.email, 2, null, null, null, null, (int)model.phyId, temp, false, count);
                            }
                            count++;
                        }
                    }
                }

                if (flexRadioDefault == "SMS" || flexRadioDefault == "Both")
                {
                    string messageSMS = contactProviderMessage;
                    string recipient = "+917990117699";
                    var accountSid = _configuration["Twilio:accountSid"];
                    var authToken = _configuration["Twilio:authToken"];
                    var twilionumber = _configuration["Twilio:twilioNumber"];

                    while (smsSentTries <= 3 && !isSMSSent)
                    {
                        try
                        {
                            TwilioClient.Init(accountSid, authToken);
                            //var messageBody =
                            var message2 = MessageResource.Create(
                                from: new Twilio.Types.PhoneNumber(twilionumber),
                                body: messageSMS,
                                to: new Twilio.Types.PhoneNumber(recipient)
                            );

                            isSMSSent = true;
                            DateTime temp = DateTime.Now;
                            _adminInterface.AddSmsLogFromContactProvider(messageSMS, recipient, null, (int)model.phyId, temp, smsSentTries, isSMSSent, 3);
                            break;
                        }

                        catch (Exception ex)
                        {
                            if (smsSentTries >= 3)
                            {
                                DateTime temp = DateTime.Now;
                                _adminInterface.AddSmsLogFromContactProvider(messageSMS, recipient, null, (int)model.phyId, temp, smsSentTries, isSMSSent, 3);
                            }
                            smsSentTries++;
                        }
                    }

                }

                TempData["success"] = "Message has been sent to provider";
                return RedirectToAction("ProviderMenu");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to send the message";
                return RedirectToAction("ProviderMenu");
            }

        }

        [CustomAuthorize("Admin", "ProviderMenu")]
        // function to return Create Provider Account view
        public IActionResult CreateProviderAccount()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 5;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                EditProviderAccountViewModel ep = new EditProviderAccountViewModel();
                ep.adminNavbarModel = an;
                ep.regions = _adminInterface.GetAllRegion();
                ep.allRoles = _adminInterface.GetSpecifiedProviderRoles();
                return View(ep);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the provider account";
                return RedirectToAction("ProviderMenu");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "ProviderMenu")]
        public IActionResult CreateNewProviderAccount(EditProviderAccountViewModel model, List<int> regionNames)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 5;

                if (regionNames.IsNullOrEmpty())
                {
                    TempData["error"] = "Select regions from checkbox!";
                    return RedirectToAction("CreateAdminAccount");
                }

                if(PatientCheck(model.Email))
                {
                    TempData["error"] = "Email already exists in the other account";
                    return RedirectToAction("ProviderMenu");
                }

                _adminInterface.CreateNewProviderAccount(model, regionNames, ad.AdminId);
                TempData["success"] = "Provider account created successfully";
                return RedirectToAction("CreateProviderAccount");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the new provider account";
                return RedirectToAction("CreateProviderAccount");
            }
        }

        [CustomAuthorize("Admin", "UserAccess")]
        public IActionResult CreateProviderAccountFromUserAccess()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 11;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                EditProviderAccountViewModel ep = new EditProviderAccountViewModel();
                ep.adminNavbarModel = an;
                ep.regions = _adminInterface.GetAllRegion();
                ep.allRoles = _adminInterface.GetSpecifiedProviderRoles();
                return View(ep);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the provider account";
                return RedirectToAction("UserAccess");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "UserAccess")]
        public IActionResult CreateNewProviderAccountFromUserAccess(EditProviderAccountViewModel model, List<int> regionNames)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 11;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                if (regionNames.IsNullOrEmpty())
                {
                    TempData["error"] = "Select regions from checkbox!";
                    return RedirectToAction("CreateAdminAccount");
                }

                if (PatientCheck(model.Email))
                {
                    TempData["error"] = "Email already exists in the other account";
                    return RedirectToAction("ProviderMenu");
                }

                _adminInterface.CreateNewProviderAccount(model, regionNames, ad.AdminId);
                TempData["success"] = "Provider account created successfully";
                return RedirectToAction("UserAccess");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the new provider account";
                return RedirectToAction("UserAccess");
            }
        }

        // function to return Edit Provider Account view
        [CustomAuthorize("Admin", "ProviderMenu")]
        public IActionResult EditProviderAccount(int id)
        {
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "0");
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 5;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                EditProviderAccountViewModel ep = _adminInterface.ProviderEditAccount(id, an);
                return View(ep);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to edit the provider account";
                return RedirectToAction("ProviderMenu");
            }
        }

        [CustomAuthorize("Admin", "ProviderMenu")]
        // function to save changed password of provider
        public IActionResult SavePasswordOfProvider(EditProviderAccountViewModel ep)
        {
            try
            {
                _adminInterface.SavePasswordOfPhysician(ep);
                TempData["success"] = "Password changed successfully";
                return RedirectToAction("EditProviderAccount", new { id = ep.PhysicianId });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to change the password of account";
                return RedirectToAction("EditProviderAccount", new { id = ep.PhysicianId });
            }
        }

        [CustomAuthorize("Admin", "ProviderMenu")]
        // function to save changes of billing info of provider
        public IActionResult EditProviderBillingInfo(EditProviderAccountViewModel ep)
        {
            try
            {
                _adminInterface.EditProviderBillingInfo(ep);
                return RedirectToAction("EditProviderAccount", new { id = ep.PhysicianId });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to edit the billing info";
                return RedirectToAction("EditProviderAccount", new { id = ep.PhysicianId });
            }
        }

        [CustomAuthorize("Admin", "ProviderMenu")]
        // function to save provider info
        public IActionResult SaveProviderProfile(EditProviderAccountViewModel ep, string selectedRegionsList)
        {
            try
            {
                _adminInterface.SaveProviderProfile(ep, selectedRegionsList);
                TempData["success"] = "Provider information saved successfully";
                return RedirectToAction("EditProviderAccount", new { id = ep.PhysicianId });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to save the provider info";
                return RedirectToAction("EditProviderAccount", new { id = ep.PhysicianId });
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "ProviderMenu")]
        // function called when we upload file of signature of provider
        public IActionResult SetContentOfPhysician(IFormFile file, int PhysicianId, bool IsSignature)
        {
            try
            {
                _adminInterface.SetContentOfPhysician(file, PhysicianId, IsSignature);
                return RedirectToAction("EditProviderAccount", new { id = PhysicianId });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to upload files of provider";
                return RedirectToAction("EditProviderAccount", new { id = PhysicianId });
            }
        }

        [CustomAuthorize("Admin", "ProviderMenu")]
        // function to upload all other docs of provider
        public IActionResult SetAllDocOfPhysician(IFormFile file, int PhysicianId, int num)
        {
            try
            {
                _adminInterface.SetAllDocOfPhysician(file, PhysicianId, num);
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");
                return RedirectToAction("EditProviderAccountFromUserAccess", new { id = PhysicianId });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to upload files of provider";
                return RedirectToAction("EditProviderAccountFromUserAccess", new { id = PhysicianId });
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "ProviderMenu")]
        // function to save changes of provider profile
        public IActionResult PhysicianProfileUpdate(EditProviderAccountViewModel model)
        {
            try
            {
                _adminInterface.PhysicianProfileUpdate(model);
                return RedirectToAction("EditProviderAccount", new { id = model.PhysicianId });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to update the provider info";
                return RedirectToAction("EditProviderAccount", new { id = model.PhysicianId });
            }
        }


        [HttpPost]
        [CustomAuthorize("Admin", "ProviderMenu")]
        public IActionResult DeletePhysicianAccount(int id)
        {
            try
            {
                _adminInterface.DeletePhysicianAccount(id);
                TempData["success"] = "Account deleted successfully";
                return RedirectToAction("ProviderMenu");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to delete the provider profile";
                return RedirectToAction("ProviderMenu");
            }
        }

        [CustomAuthorize("Admin", "CreateRole")]
        // function to return Create Role view
        public IActionResult CreateRole()
        {
            try
            {

                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 10;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                CreateRoleViewModel cr = new CreateRoleViewModel
                {
                    adminNavbarModel = an,
                    allRoles = _adminInterface.GetAllMenus(),
                };
                return View(cr);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the role";
                return RedirectToAction("AccountAccess");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "CreateRole")]
        // function to create a new role
        public IActionResult CreateNewRole(string roleName, string acType, string menuIdString)
        {
            try
            {

                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 10;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                List<int> menuIds = null;
                if (!string.IsNullOrEmpty(menuIdString))
                {
                    menuIds = menuIdString.Split(',').Select(int.Parse).ToList();
                }
                _adminInterface.CreateNewRole2(roleName, acType, an.Admin_Name, menuIds);
                TempData["success"] = "New role created";
                return RedirectToAction("AccountAccess");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the role";
                return RedirectToAction("CreateRole");
            }
        }

        [CustomAuthorize("Admin", "AccountAccess")]
        // function to return view of Account Access page
        public IActionResult AccountAccess()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 10;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                CreateRoleViewModel cr = new CreateRoleViewModel
                {
                    adminNavbarModel = an,
                    allRoles = _adminInterface.GetAllMenus(),
                    roles = _adminInterface.GetAllRoles(),
                };
                return View(cr);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to access the accounts";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "AccountAccess")]
        // function to delete a role
        public IActionResult DeleteRole(int roleid)
        {
            try
            {
                _adminInterface.DeleteRoleFromId(roleid);
                TempData["success"] = "Role deleted successfully";
                return RedirectToAction("AccountAccess");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to delete the role";
                return RedirectToAction("AccountAccess");
            }
        }

        [CustomAuthorize("Admin", "EditRole")]
        // function to return Edit Role view
        public IActionResult EditRole(int roleid)
        {
            try
            {

                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 10;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                CreateRoleViewModel cr = new CreateRoleViewModel
                {
                    adminNavbarModel = an,
                    allRoles = _adminInterface.GetAllMenus(),
                    roles = _adminInterface.GetAllRoles(),
                    roleMenus = _adminInterface.GetAllRoleMenu(roleid),
                    NameOfRole = _adminInterface.GetNameFromRoleId(roleid),
                    accountType = _adminInterface.GetAccountTypeFromId(roleid),
                    roleId = roleid,
                };
                return View(cr);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to edit the role";
                return RedirectToAction("AccountAccess");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "EditRole")]
        // function to edit a role
        public IActionResult EditRoleSubmit(string menuIdString, int roleid)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 10;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                List<int> menuIds = null;
                if (!string.IsNullOrEmpty(menuIdString))
                {
                    menuIds = menuIdString.Split(',').Select(int.Parse).ToList();
                }
                _adminInterface.EditRoleSubmitAction(roleid, menuIds);
                TempData["success"] = "Role edited successfully";
                return RedirectToAction("EditRole", new { roleid = roleid });
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to edit the role";
                return RedirectToAction("EditRole", new { roleid = roleid });
            }
        }

        [CustomAuthorize("Admin", "CreateAdminAccount")]
        public IActionResult CreateAdminAccount()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 12;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                AdminProfile ap = new AdminProfile();
                ap.an = an;
                ap.allRoles = _adminInterface.GetSpecifiedAdminRoles();
                ap.allRegions = _adminInterface.GetAllRegions();
                return View(ap);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the admin account";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "CreateAdminAccount")]
        public IActionResult CreateNewAdminAccount(AdminProfile model, List<int> regionNames)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 12;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                if (regionNames.IsNullOrEmpty())
                {
                    TempData["error"] = "Select regions from checkbox!";
                    return RedirectToAction("CreateAdminAccount");
                }

                if(PatientCheck(model.email))
                {
                    TempData["error"] = "Email already exists in other account";
                    return RedirectToAction("CreateAdminAccount");
                }

                _adminInterface.CreateNewAdminAccount(model, regionNames, (int)userId);
                TempData["success"] = "Admin account created successfully";
                return RedirectToAction("AdminDashboard");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the admin account";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "UserAccess")]
        public IActionResult CreateAdminAccountFromUserAccess()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 11;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                AdminProfile ap = new AdminProfile();
                ap.an = an;
                ap.allRoles = _adminInterface.GetSpecifiedAdminRoles();
                ap.allRegions = _adminInterface.GetAllRegions();
                return View(ap);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the admin account";
                return RedirectToAction("AdminDashboard");
            }
        }

        [HttpPost]
        [CustomAuthorize("Admin", "UserAccess")]
        public IActionResult CreateNewAdminAccountFromUserAccess(AdminProfile model, List<int> regionNames)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 11;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                if (regionNames.IsNullOrEmpty())
                {
                    TempData["error"] = "Select regions from checkbox!";
                    return RedirectToAction("CreateAdminAccount");
                }

                if (PatientCheck(model.email))
                {
                    TempData["error"] = "Email already exists in other account";
                    return RedirectToAction("CreateAdminAccount");
                }

                _adminInterface.CreateNewAdminAccount(model, regionNames, (int)userId);
                TempData["success"] = "Admin account created successfully";
                return RedirectToAction("UserAccess");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the admin account";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "PatientHistory")]
        // function to return Patient History view
        public IActionResult PatientHistory()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 17;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                PatientHistoryViewModel pr = _adminInterface.PatientHistoryFilteredData(an, null, null, null, null);
                return View(pr);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view history of patients";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "PatientHistory")]
        // function to filter the records of Patient History
        public IActionResult PatientHistoryFilter(string? firstName = "", string? lastName = "", string? email = "", string? phoneNumber = "", int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 17;
                PatientHistoryViewModel pr = _adminInterface.PatientHistoryFilteredData(an, firstName, lastName, phoneNumber, email, page, pageSize);
                return PartialView("PatientHistoryPagePartialView", pr);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view history of patients";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "UserAccess")]
        public IActionResult UserAccess()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 11;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                UserAccessViewModel cr = new UserAccessViewModel
                {
                    adminNavbarModel = an,
                };
                return View(cr);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to access the accounts";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "UserAccess")]
        public IActionResult UserAccessFilter(int? accountType = -1)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 10;
                UserAccessViewModel ua = _adminInterface.UserAccessFilteredData(an, (int)accountType);
                return PartialView("UserAccessPagePartialView", ua);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view data of users";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "BlockedHistory")]
        public IActionResult BlockedHistory()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 18;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                BlockedHistoryViewModel bh = new BlockedHistoryViewModel
                {
                    adminNavbarModel = an,
                    allData = _adminInterface.GetBlockedHistoryData()
                };
                return View(bh);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to access the accounts";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "BlockedHistory")]
        public IActionResult BlockedHistoryFilteredData(DateOnly date, string? name = "", string? phoneNumber = "", string? email = "", int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 18;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                BlockedHistoryViewModel bh = _adminInterface.BlockedHistoryFilteredData(an, name, date, email, phoneNumber, page, pageSize);
                return PartialView("BlockedHistoryPagePartialView", bh);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to access the accounts";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "BlockedHistory")]
        public IActionResult UnblockTheRequest(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 11;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                _adminInterface.UnblockRequest(id);
                TempData["success"] = "Request unblocked successfully";
                return RedirectToAction("BlockedHistory");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to access the accounts";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "Scheduling")]
        public IActionResult Scheduling(string? monthId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 6;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                ViewBag.monthName = monthId;
                SchedulingViewModel svm = new SchedulingViewModel
                {
                    adminNavbarModel = an,
                    allRegions = _adminInterface.GetAllRegion(),
                };
                return View(svm);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view scheduling page";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "Scheduling")]
        public IActionResult GetScheduleData(int RegionId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 6;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                string[] color = { "#edacd2", "#a5cfa6" };
                List<ShiftDetail> shiftDetails = _adminInterface.GetScheduleData(RegionId);

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

        [CustomAuthorize("Admin", "Scheduling")]
        public IActionResult GetProviderDetailsForSchedule(int RegionId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 6;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;

                List<SchedulingViewModel> model = _adminInterface.GetProviderInformation(RegionId);

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
        public IActionResult ViewShift(int ShiftDetailId)
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
            return PartialView("~/Views/Shared/ViewShiftModalPartialView.cshtml", _adminInterface.GetViewShift(ShiftDetailId, an));
        }

        [CustomAuthorize("Admin Provider", "Scheduling")]
        public bool ReturnViewShift(int ShiftDetailId)
        {
            try
            {
                return _adminInterface.ReturnViewShift(ShiftDetailId);
            }
            catch { return false; }
        }

        [CustomAuthorize("Admin Provider", "Scheduling")]
        public IActionResult EditViewShift(EditViewShiftModel ShiftDetail, int pId)
        {
            try
            {
                int x = 0;
                x = _adminInterface.EditViewShift(ShiftDetail, pId);
                if (x == 0)
                {
                    TempData["success"] = "Shift edited successfully";
                    return RedirectToAction("Scheduling");
                }
                else if (x == 1)
                {
                    TempData["error"] = "Shift already exists at that time";
                    return RedirectToAction("Scheduling");
                }
                else
                {
                    TempData["error"] = "Unable to edit shift information";
                    return RedirectToAction("Scheduling");
                }
            }
            catch
            {
                return RedirectToAction("Scheduling");
            }
        }

        [CustomAuthorize("Admin Provider", "Scheduling")]
        public bool DeleteViewShift(int ShiftDetailId)
        {
            try
            {
                return _adminInterface.DeleteViewShift(ShiftDetailId);
            }
            catch { return false; }
        }

        [CustomAuthorize("Admin", "Scheduling")]
        public IActionResult CreateNewShift(SchedulingViewModel model, List<int> RepeatedDays)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 6;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                int x = _adminInterface.CreateNewShift(model, RepeatedDays, ad.AdminId);
                if (x == 1)
                {
                    TempData["success"] = "Shift created successfully";
                    return RedirectToAction("Scheduling");
                }
                else if (x == 0)
                {
                    TempData["error"] = "Shift already exists in the given time";
                    return RedirectToAction("Scheduling");
                }
                else
                {
                    TempData["error"] = "Sorry, shift is not created!";
                    return RedirectToAction("Scheduling");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Unable to create the new shift";
                return RedirectToAction("Scheduling");
            }
        }

        [CustomAuthorize("Admin", "Scheduling")]
        public IActionResult RequestedShifts()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 6;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                RequestedShiftsViewModel rs = new RequestedShiftsViewModel
                {
                    adminNavbarModel = an,
                    allRegions = _adminInterface.GetAllRegion(),
                    requestedShiftsTableData = _adminInterface.GetRequestedShiftsData(-1)
                };
                return View(rs);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view requested shifts page";
                return RedirectToAction("Scheduling");
            }
        }

        [CustomAuthorize("Admin", "Scheduling")]
        public IActionResult RequestedShiftsFilteredData(int? regionId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 6;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                RequestedShiftsViewModel rs = new RequestedShiftsViewModel
                {
                    adminNavbarModel = an,
                    allRegions = _adminInterface.GetAllRegion(),
                    requestedShiftsTableData = _adminInterface.GetRequestedShiftsData(regionId)
                };
                return PartialView("RequestedShiftsPagePartialView", rs);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view requested shifts page";
                return RedirectToAction("Scheduling");
            }
        }

        [CustomAuthorize("Admin", "Scheduling")]
        public IActionResult ApproveSelectedShifts(string shiftDetailIdString)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 6;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                _adminInterface.ApproveSelectedShifts(shiftDetailIdString);
                TempData["success"] = "Selected shifts approved successfully";
                return RedirectToAction("RequestedShifts");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to approve selected shifts";
                return RedirectToAction("RequestedShifts");
            }
        }

        [CustomAuthorize("Admin", "Scheduling")]
        public IActionResult DeleteSelectedShifts(string shiftDetailIdString)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 6;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                _adminInterface.DeleteSelectedShifts(shiftDetailIdString);
                TempData["success"] = "Selected shifts deleted successfully";
                return RedirectToAction("RequestedShifts");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to approve deleted shifts";
                return RedirectToAction("RequestedShifts");
            }
        }

        [CustomAuthorize("Admin", "Scheduling")]
        public IActionResult MDsOnCall()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 6;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                MdsOnCallViewModel moc = new MdsOnCallViewModel
                {
                    adminNavbarModel = an,
                    allRegions = _adminInterface.GetAllRegion(),
                };
                return View(moc);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to approve deleted shifts";
                return RedirectToAction("RequestedShifts");
            }
        }

        [CustomAuthorize("Admin", "Scheduling")]
        public IActionResult MdsOnCallFilteredData(int? region = -1)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 6;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                MdsOnCallViewModel moc = _adminInterface.GetMdsData(region);
                moc.adminNavbarModel = an;
                return PartialView("MDsOnCallPagePartialView", moc);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to approve deleted shifts";
                return RedirectToAction("RequestedShifts");
            }
        }

        [CustomAuthorize("Admin", "SearchRecords")]
        public IActionResult SearchRecords()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 14;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                SearchRecordsViewModel sr = new SearchRecordsViewModel
                {
                    adminNavbarModel = an
                };
                return View(sr);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to search records";
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin", "SearchRecords")]
        public IActionResult SearchRecordsFilteredData(DateTime fromDate, DateTime toDate, int page = 1, int pageSize = 10, int? requestStatus = -1, string? patientName = "", int? requestType = -1, string? providerName = "", string? email = "", string? phoneNumber = null)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 14;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                SearchRecordsViewModel sr = _adminInterface.SearchRecordsFilteredData(an, page, pageSize, requestStatus, patientName, requestType, fromDate, toDate, providerName, email, phoneNumber);
                return PartialView("SearchRecordsPagePartialView", sr);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to search records";
                return RedirectToAction("SearchRecords");
            }
        }

        [CustomAuthorize("Admin", "Scheduling")]
        public IActionResult ExportSearchRecords(SearchRecordsViewModel model)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 14;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                try
                {
                    DateTime temp = new DateTime(1, 1, 1, 0, 0, 0);
                    int reqStatus = (int)((model.requestStatus == null) ? -1 : model.requestStatus);
                    string pName = (model.patientName == null) ? "" : model.patientName;
                    int reqType = (int)((model.requestType == null) ? -1 : model.requestType);
                    DateTime fDate = (DateTime)((model.fromDate == null) ? temp : model.fromDate);
                    DateTime toDate = (DateTime)((model.toDate == null) ? temp : model.toDate);
                    string proName = (model.providerName == null) ? "" : model.providerName;
                    string e = (model.email == null) ? "" : model.email;
                    string pn = (model.phoneNumber == null) ? "" : model.phoneNumber;

                    SearchRecordsViewModel sr = _adminInterface.SearchRecordsFilteredData(an, 1, 10, reqStatus, pName, reqType, fDate, toDate, proName, e, pn);
                    List<SearchRecordsTableData> data = sr.allDataForExcel;
                    var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Data");

                    worksheet.Cell(1, 1).Value = "Patient Name";
                    worksheet.Cell(1, 2).Value = "Requestor";
                    worksheet.Cell(1, 3).Value = "Date Of Service";
                    worksheet.Cell(1, 4).Value = "Close Case Date";
                    worksheet.Cell(1, 5).Value = "Email";
                    worksheet.Cell(1, 6).Value = "Phone Number";
                    worksheet.Cell(1, 7).Value = "Address";
                    worksheet.Cell(1, 8).Value = "Zip";
                    worksheet.Cell(1, 9).Value = "Request Status";
                    worksheet.Cell(1, 10).Value = "Physician";
                    worksheet.Cell(1, 11).Value = "Physician Note";
                    worksheet.Cell(1, 12).Value = "Cancelled By Provider Note";
                    worksheet.Cell(1, 13).Value = "Admin Note";
                    worksheet.Cell(1, 14).Value = "Patient Note";

                    int row = 2;
                    string requestType = "";
                    string requestStatus = "";
                    foreach (var item in data)
                    {
                        worksheet.Cell(row, 1).Value = item.patientName;

                        if (item.requestor == 1)
                        {
                            requestType = "Patient";
                        }
                        else if (item.requestor == 2)
                        {
                            requestType = "Family";
                        }
                        else if (item.requestor == 3)
                        {
                            requestType = "Concierge";
                        }
                        else
                        {
                            requestType = "Business";
                        }

                        worksheet.Cell(row, 2).Value = requestType;
                        worksheet.Cell(row, 3).Value = item.dateOfService;
                        worksheet.Cell(row, 4).Value = item.closeCaseDate;
                        worksheet.Cell(row, 5).Value = item.email;
                        worksheet.Cell(row, 6).Value = item.phoneNumber;
                        worksheet.Cell(row, 7).Value = item.address;
                        worksheet.Cell(row, 8).Value = item.zipcode;

                        if (item.requestStatus == 1)
                        {
                            requestStatus = "Unassigned";
                        }
                        else if (item.requestStatus == 2)
                        {
                            requestStatus = "Accepted";
                        }
                        else if (item.requestStatus == 3)
                        {
                            requestStatus = "Cancelled";
                        }
                        else if (item.requestStatus == 4)
                        {
                            requestStatus = "MDEnRoute";
                        }
                        else if (item.requestStatus == 5)
                        {
                            requestStatus = "MDONSite";
                        }
                        else if (item.requestStatus == 6)
                        {
                            requestStatus = "Conclude";
                        }
                        else if (item.requestStatus == 7)
                        {
                            requestStatus = "Cancelled By Patient";
                        }
                        else if (item.requestStatus == 8)
                        {
                            requestStatus = "Closed";
                        }
                        else if (item.requestStatus == 9)
                        {
                            requestStatus = "Unpaid";
                        }
                        else if (item.requestStatus == 10)
                        {
                            requestStatus = "Clear";
                        }
                        else
                        {
                            requestStatus = "Blocked";
                        }
                        worksheet.Cell(row, 9).Value = item.requestStatus;
                        worksheet.Cell(row, 10).Value = item.physician;
                        worksheet.Cell(row, 11).Value = item.physicianNote;
                        worksheet.Cell(row, 12).Value = item.cancelledByProviderNote;
                        worksheet.Cell(row, 13).Value = item.adminNote;
                        worksheet.Cell(row, 14).Value = item.patientNote;
                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    var memoryStream = new MemoryStream();
                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Search_Records.xlsx");
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Unable to export records";
            }
            return RedirectToAction("Scheduling");
        }

        [CustomAuthorize("Admin", "SearchRecords")]
        public IActionResult DeleteFromSearchRecord(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 14;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                if (_adminInterface.DeleteSearchRecord(id))
                {
                    TempData["success"] = "Record delete successfully";
                }
                else
                {
                    TempData["error"] = "Unable to delete the record";
                }
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to delete the record";
            }
            return RedirectToAction("SearchRecords");
        }

        [CustomAuthorize("Admin", "SMSLogs")]
        public IActionResult SmsLogs()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 16;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                SmsLogsViewModel sl = new SmsLogsViewModel
                {
                    adminNavbarModel = an,
                };
                return View(sl);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view vendors information";
                return RedirectToAction("AdmiDashboard");
            }
        }

        [CustomAuthorize("Admin", "SMSLogs")]
        public IActionResult SmsLogsFilteredData(DateTime createdDate, DateTime sentDate, int page = 1, int pageSize = 10, int? role = 0, string? recipientName = "", string? phoneNumber = "")
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 16;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                SmsLogsViewModel sl = _adminInterface.SmsLogsFilteredData(an, page, pageSize, role, recipientName, phoneNumber, createdDate, sentDate);
                return PartialView("SmsLogsPagePartialView", sl);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view SMS logs";
                return RedirectToAction("AdmiDashboard");
            }
        }

        [CustomAuthorize("Admin", "EmailLogs")]
        public IActionResult EmailLogs()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 15;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                EmailLogsViewModel el = new EmailLogsViewModel
                {
                    adminNavbarModel = an
                };
                return View(el);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view vendors information";
                return RedirectToAction("AdmiDashboard");
            }
        }

        [CustomAuthorize("Admin", "EmailLogs")]
        public IActionResult EmailLogsFilteredData(DateTime createdDate, DateTime sentDate, int page = 1, int pageSize = 10, int? role = 0, string? recipientName = "", string? emailId = "")
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 15;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                EmailLogsViewModel el = _adminInterface.EmailLogsFilteredData(an, page, pageSize, role, recipientName, emailId, createdDate, sentDate);
                return PartialView("EmailLogsPagePartialView", el);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view Email logs";
                return RedirectToAction("AdmiDashboard");
            }
        }

        [CustomAuthorize("Admin", "Partners")]
        public IActionResult Vendors()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 8;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                VendorsViewModel v = new VendorsViewModel
                {
                    adminNavbarModel = an,
                    professionType = _adminInterface.GetHealthProfessionalType()
                };
                return View(v);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view vendors information";
                return RedirectToAction("AdmiDashboard");
            }
        }

        [CustomAuthorize("Admin", "Partners")]
        public IActionResult VendorsFilteredData(string? name = "", int? professionalId = -1, int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 8;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                VendorsViewModel v = _adminInterface.VendorsFilteredData(an, name, professionalId, page, pageSize);
                return PartialView("VendorsPagePartialView", v);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to view vendors information";
                return RedirectToAction("AdmiDashboard");
            }
        }

        [CustomAuthorize("Admin", "Partners")]
        public IActionResult AddBusiness()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 8;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                AddVendorViewModel av = new AddVendorViewModel
                {
                    adminNavbarModel = an,
                    professionType = _adminInterface.GetHealthProfessionalType(),
                    allRegions = _adminInterface.GetAllRegion()
                };
                return View(av);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to add vendors data";
                return RedirectToAction("Vendors");
            }
        }

        [CustomAuthorize("Admin", "Partners")]
        public IActionResult AddNewBusiness(AddVendorViewModel model)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 8;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                if (_adminInterface.AddNewVendor(model))
                {
                    TempData["success"] = "New vendor added successfully";
                }
                else
                {
                    TempData["error"] = "Unable to add new vendor";
                }
                return RedirectToAction("Vendors");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to add vendors data";
                return RedirectToAction("Vendors");
            }
        }

        [CustomAuthorize("Admin", "Partners")]
        public IActionResult EditBusiness(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 8;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                AddVendorViewModel av = _adminInterface.GetVendorDataFromId(id, an);
                return View(av);
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to edit vendor data";
                return RedirectToAction("Vendors");
            }
        }

        [CustomAuthorize("Admin", "Partners")]
        public IActionResult SaveEditedBusinessInfo(AddVendorViewModel model, int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("id");
                Admin ad = _adminInterface.GetAdminFromId((int)userId);
                AdminNavbarModel an = new AdminNavbarModel();
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.Tab = 8;
                string token = Request.Cookies["token"];
                string roleIdVal = _jwtToken.GetRoleId(token);
                List<string> menus = _adminInterface.GetAllMenus(roleIdVal);
                ViewBag.Menu = menus;
                if (_adminInterface.SaveEditedBusinessInfo(model, id))
                {
                    TempData["success"] = "Vendor info edited successfully";
                }
                else
                {
                    TempData["error"] = "Unable to edit vendor info";
                }
                return RedirectToAction("Vendors");
            }

            catch (Exception ex)
            {
                TempData["error"] = "Unable to edit vendor data";
                return RedirectToAction("Vendors");
            }
        }

        [CustomAuthorize("Admin", "Partners")]
        public IActionResult DeleteBusinessProfile(int id)
        {
            try
            {
                if (_adminInterface.DeleteBusinessProfile(id))
                {
                    TempData["success"] = "Profile deleted successfully";
                }
                else
                {
                    TempData["error"] = "Unable to delete the profile";
                }
                return RedirectToAction("Vendors");
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occured";
                return RedirectToAction("Vendors");
            }
        }

        public IActionResult PageNotFoundError()
        {
            return View();
        }
    }



}

