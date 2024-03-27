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
using Microsoft.Office.Interop.Excel;
using HalloDoc.LogicLayer.Patient_Interface;
using static HalloDoc.DataLayer.Models.Enums;
using System.Collections;
using HalloDoc.LogicLayer.Patient_Repository;
using System.Net.Mail;
using System.Net;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Drawing;
using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
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

        public AdminController(IAdminInterface adminInterface, IHttpContextAccessor sescontext, IJwtToken jwtToken)
        {
            _adminInterface = adminInterface;
            _sescontext = sescontext;
            _jwtToken = jwtToken;
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
                        Admin ad = _adminInterface.ValidateUser(model);
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
        [CustomAuthorize("Admin")]
        public IActionResult AdminDashboard(string? status)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard(status, (int)userId, null, null, -1, 1, 10);
            return View(adminDashboardViewModel);
        }



        //[HttpPost]
        // function for new state of admin dashboard
        [CustomAuthorize("Admin")]
        public IActionResult New(string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("id");

            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("New", (int)userId, search, requestor, (int)region, page, pageSize);
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        // function for pending state of admin dashboard
        [CustomAuthorize("Admin")]
        public IActionResult Pending(string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("id");

            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Pending", (int)userId, search, requestor, (int)region, page, pageSize);
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        // function for active state of admin dashboard
        [CustomAuthorize("Admin")]
        public IActionResult Active(string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("id");

            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Active", (int)userId, search, requestor, (int)region, page, pageSize);
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        // function for conclude state of admin dashboard
        [CustomAuthorize("Admin")]
        public IActionResult Conclude(string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("id");

            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Conclude", (int)userId, search, requestor, (int)region, page, pageSize);
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        // function for to-close state of admin dashboard
        [CustomAuthorize("Admin")]
        public IActionResult Toclose(string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("id");

            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("ToClose", (int)userId, search, requestor, (int)region, page, pageSize);
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        // function for unpaid state of admin dashboard
        [CustomAuthorize("Admin")]
        public IActionResult Unpaid(string? search = "", string? requestor = "", int? region = -1, int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("id");

            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Unpaid", (int)userId, search, requestor, (int)region, page, pageSize);
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        [CustomAuthorize("Admin")]
        // function to get data for excel sheet
        public List<Request> GetTableData()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            List<Request> data = new List<Request>();

            data = _adminInterface.GetRequestDataInList();
            return data;
        }

        [CustomAuthorize("Admin")]
        // function to download the data of all requests in excel sheet
        public IActionResult DownloadAll()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
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

        [CustomAuthorize("Admin")]
        // function to dowload filtered data or data of particular state in excel sheet
        public IActionResult DownloadSpecificExcel(AdminDashboardTableView model)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            try
            {
                List<Request> data = model.requests;
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Data");

                if (model.status == "New")
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

                else if (model.status == "Pending" || model.status == "Active")
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

                else if (model.status == "Conclude")
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

                else if (model.status == "ToClose")
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

                else if (model.status == "Unpaid")
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
                string fileName = $"{model.status}.xlsx";
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        [CustomAuthorize("Admin")]
        // function to display data in View Case view
        public IActionResult ViewCase(int requestId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            Request request = _adminInterface.ValidateRequest(requestId);

            RequestClient user = _adminInterface.ValidateRequestClient(request.RequestClientId);

            int intYear = (int)user.IntYear;
            int intDate = (int)user.IntDate;
            string month = user.StrMonth;
            int mon = 0;
            if (month.Length > 1)
            {

                if (month == "January")
                {
                    mon = 1;
                }
                else if (month == "February")
                {
                    mon = 2;
                }
                else if (month == "March")
                {
                    mon = 3;
                }
                else if (month == "April")
                {
                    mon = 4;
                }
                else if (month == "May")
                {
                    mon = 5;
                }
                else if (month == "June")
                {
                    mon = 6;
                }
                else if (month == "July")
                {
                    mon = 7;
                }
                else if (month == "August")
                {
                    mon = 8;
                }
                else if (month == "September")
                {
                    mon = 9;
                }
                else if (month == "October")
                {
                    mon = 10;
                }
                else if (month == "November")
                {
                    mon = 11;
                }
                else if (month == "December")
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
                RequestId = requestId,
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
            };

            return View(viewCase);
        }


        [HttpPost]
        [CustomAuthorize("Admin")]
        // action to store edited information of view case in database
        public IActionResult EditViewCase(ViewCaseModel userProfile)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
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
            return RedirectToAction("ViewCase", new { requestId = requestId });
        }

        [CustomAuthorize("Admin")]
        // action to show data in View Notes view
        public IActionResult ViewNotes(int requestId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            Request r = _adminInterface.ValidateRequest(requestId);

            RequestNote rn = _adminInterface.FetchRequestNote(requestId);

            RequestStatusLog rsl = _adminInterface.FetchRequestStatusLogs(requestId);
            List<RequestStatusLog> rs = _adminInterface.GetAllRslData(requestId);
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
                    tNotes = tNotes + " " + item.Notes;
                }
            }

            int id = (int)rsl.PhysicianId;

            Physician py = _adminInterface.FetchPhysician(id);

            var viewModel = new ViewNotes
            {
                AdminNotes = adNotes,
                PhysicianNotes = phNotes,
                PhyName = py.FirstName,
                Notes = tNotes,
                CreatedDate = rsl.CreatedDate,
                RequestId = requestId,
                an = an,
            };
            return View(viewModel);
        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function to store edited information of View Notes view in database
        public IActionResult EditViewNotes(ViewNotes model)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            RequestNote rn = _adminInterface.FetchRequestNote(model.RequestId);

            _adminInterface.EditViewNotesAction(model);
            return RedirectToAction("ViewNotes", new { requestId = model.RequestId });

        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function called when we cancel the case OR submit the cancel case modal
        public IActionResult CancelCase(AdminDashboardTableView model, int selectedCaseTagId, string additionalNotes)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

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

        [CustomAuthorize("Admin")]
        // function called when we want to fetch all the physicians belonging to a certain region as given by RegionId
        public List<Physician> GetPhysicianByRegion(AdminDashboardTableView model, int RegionId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            List<Physician> p = _adminInterface.FetchPhysicianByRegion(RegionId);
            return p;
        }

        [CustomAuthorize("Admin")]
        public int sendAgreement2(int reqId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            Request r = _adminInterface.GetReqFromReqType(reqId);
            return r.RequestTypeId;
        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function called when we assign the case OR submit the Assign Case modal
        public IActionResult AssignCaseSubmitAction(AdminDashboardTableView model, string assignCaseDescription, int selectedPhysicianId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            Request r = _adminInterface.ValidateRequest(model.RequestId);
            r.Status = 2;
            r.PhysicianId = selectedPhysicianId;

            RequestStatusLog rsl = new RequestStatusLog();
            rsl.RequestId = model.RequestId;
            rsl.Notes = assignCaseDescription;
            rsl.Status = 2;
            rsl.CreatedDate = DateTime.Now;
            rsl.TransToPhysicianId = selectedPhysicianId;
            rsl.PhysicianId = selectedPhysicianId;

            _adminInterface.AddRequestStatusLogFromCancelCase(rsl);
            _adminInterface.UpdateRequest(r);
            TempData["success"] = "Successfully requested to assign the case";
            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function called when we transfer the case OR when we submit the transfer case modal
        public IActionResult TransferCaseSubmitAction(AdminDashboardTableView model, string assignCaseDescription, int selectedPhysicianId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

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

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function when we clear a case OR when we submit the clear case modal
        public IActionResult ClearCaseSubmitAction(AdminDashboardTableView model)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            Request r = _adminInterface.GetReqFromModel(model);
            if (r != null)
            {
                r.Status = 10;
                TempData["success"] = "Case cleared successfully";
                _adminInterface.UpdateRequest(r);
            }
            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        public IActionResult BlockCase(AdminDashboardTableView model, string reasonForBlockRequest)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            Request r = _adminInterface.ValidateRequest(model.RequestId);
            r.Status = 11;
            _adminInterface.UpdateRequest(r);

            RequestStatusLog rs = new RequestStatusLog();
            rs.Status = 11;
            rs.CreatedDate = DateTime.Now;
            rs.Notes = reasonForBlockRequest;
            rs.RequestId = model.RequestId;
            _adminInterface.AddRequestStatusLogFromCancelCase(rs);

            BlockRequest br = new BlockRequest();
            br.RequestId = model.RequestId;
            br.Email = r.Email;
            br.IsActive = new BitArray(1, true);
            br.Reason = reasonForBlockRequest;
            br.CreatedDate = DateTime.Now;
            _adminInterface.AddBlockRequestData(br);
            TempData["success"] = "Case blocked successfully";
            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function called when admin creates the request for a patient
        public async Task<IActionResult> CreateRequest(AdminCreateRequestModel model)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            

            if (ModelState.IsValid)
            {
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
                _adminInterface.InsertDataOfRequest(model);
            }
            TempData["success"] = "Request created successfully";
            return View("CreateRequest");
        }

        [CustomAuthorize("Admin")]
        // function to check whether the entered state name belongs to the areas the service is available
        public IActionResult VerifyLocation(string state)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            if (state == null)
            {
                return Json(new { isVerified = 2 });
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

        [CustomAuthorize("Admin")]
        // function to return View of Create Request
        public IActionResult CreateRequest()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            return View();
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

        [CustomAuthorize("Admin")]
        // function to return View of View Uploads page
        public IActionResult ViewUploads(int requestid)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

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

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function to store the newly uploaded file from View Uploads view
        public IActionResult SetImageContent(ViewUploadsModel model, int requestId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            var request = _adminInterface.GetRequestWithUser(requestId);

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

            return RedirectToAction("ViewUploads", new { requestID = model.requestId });
        }

        [CustomAuthorize("Admin")]
        // function to delete individual file from View Uploads view
        public IActionResult DeleteIndividual(int id)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            int reqId = _adminInterface.SingleDelete(id);
            return RedirectToAction("ViewUploads", new { requestID = reqId });
        }

        [CustomAuthorize("Admin")]
        // function to delete multiple files from View Uploads view
        public IActionResult DeleteMultiple(int requestid, string fileId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            _adminInterface.MultipleDelete(requestid, fileId);
            TempData["success"] = "File(s) deleted successfully";
            return RedirectToAction("ViewUploads", new { requestID = requestid });
        }

        [CustomAuthorize("Admin")]
        // (IMPORTANT) function to send selected files in mail from View Uploads view
        public IActionResult SendSelectedFiles(int requestid, string fileName)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            try
            {
                string[] files = fileName.Split(',').Select(x => x.Trim()).ToArray();
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
                Request r = _adminInterface.ValidateRequest(requestid);
                RequestClient rc = _adminInterface.ValidateRequestClient(r.RequestClientId);
                string name = rc.FirstName + " " + rc.LastName;


                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "HalloDoc"),
                    Subject = "Documents uploaded for patient request",
                    IsBodyHtml = true,
                    Body = $"<h2>Documents</h2><p>Here are the documents uploaded for the request of Patient: {name}</p>"
                };
                var user = _adminInterface.ValidAspNetUser(rc.Email);
                foreach (var file in files)
                {
                    var filePath = Path.Combine("wwwroot/uploads", file);
                    var attachment = new Attachment(filePath);
                    mailMessage.Attachments.Add(attachment);
                }
                if (user != null)
                {
                    mailMessage.To.Add(rc.Email);
                    client.SendMailAsync(mailMessage);
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
                return RedirectToAction("ForgotPassword");
            }
        }

        [CustomAuthorize("Admin")]
        public async Task<IActionResult> SendLink(AdminDashboardTableView model)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
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
                string resetLink = $"{Request.Scheme}://{Request.Host}/Login/SubmitRequestScreen?token={resetToken}";



                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "HalloDoc"),
                    Subject = "Create Request For Patient",
                    IsBodyHtml = true,
                    Body = $"Hey {model.FirstName + " " + model.LastName} !! Please click the following link to reset your password: <a href='{resetLink}'>Click Here</a>"
                };
                RequestClient rc = _adminInterface.ValidatePatientEmail(model.email);
                if (rc != null)
                {
                    mailMessage.To.Add(model.email);
                    await client.SendMailAsync(mailMessage);
                    TempData["success"] = "Mail Sent Successfully";
                    return RedirectToAction("AdminDashboard");
                }
                else
                {
                    ModelState.AddModelError("Email", "Invalid Email");
                    return RedirectToAction("AdminDashboard");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("AdminDashboard");
            }
        }

        [CustomAuthorize("Admin")]
        // function to return Send Orders view
        public IActionResult Orders(int id)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
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

        [CustomAuthorize("Admin")]
        // function to get data of HealthProfessional table in Send Orders view
        public List<HealthProfessional> GetBusinessData(int professionId, SendOrder model)

        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            List<HealthProfessional> healthProfessionals = _adminInterface.GetBusinessDataFromProfession(professionId);
            return healthProfessionals;
        }

        [CustomAuthorize("Admin")]
        // function to get other data based on selected BusinessName in Send Orders view
        public HealthProfessional GetOtherData(int businessId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            HealthProfessional hp = _adminInterface.GetOtherDataFromBId(businessId);
            return hp;
        }
        //[HttpPost]
        [CustomAuthorize("Admin")]
        public IActionResult GetAgreementData(int reqId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            RequestClient rc = _adminInterface.GetPatientData(reqId);
            return Json(new { response = rc });
        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function to send order to specified vendor
        public IActionResult SendOrder(SendOrder model, int vendorId, int noOfRefill)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

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

        [CustomAuthorize("Admin")]
        // function to send mail of agreement to particular AspNetUser based on RequestClient's Email
        public async Task<IActionResult> SendMailOfAgreement(AdminDashboardTableView model)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            string email = _adminInterface.GetMailToSentAgreement(model.RequestId);
            RequestClient rc = _adminInterface.GetPatientData(model.RequestId);
            string url = $"{Request.Scheme}://{Request.Host}/Admin/ReviewAgreement?id={rc.RequestClientId}";
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
                    Subject = "Review the agreement",
                    IsBodyHtml = true,
                    Body = $"Please click the following link to reset your password: <br><br><a href='{url}'>Click Here</a>"
                };


                mailMessage.To.Add(model.sendAgreeEmail);
                _sescontext.HttpContext.Session.SetString("UserEmail", model.sendAgreeEmail);
                await client.SendMailAsync(mailMessage);
                TempData["success"] = "Mail sent successfully. Please check it";

                ModelState.AddModelError("Email", "Invalid Email");


            }
            catch (Exception ex)
            {
                TempData["error"] = "Failed to send the agreement to the provided mail";
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
        public IActionResult ReviewAgreement(int id)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            RequestClient rc = _adminInterface.GetRequestClientFromId(id);
            return View(rc);
        }

        [HttpPost]
        // function called when a patient accepts the agreement
        public IActionResult AcceptAgreement(int id)
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

        [HttpPost]
        // function called when a patient cancels the agreement
        public IActionResult CancelAgreement(int id, string desc)
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

        public IActionResult PlatformCreatePassword()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;
            return View();
        }

        [CustomAuthorize("Admin")]
        // function to return Encounter Form view
        public IActionResult EncounterForm(int reqId)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            EncounterForm ef = _adminInterface.GetEncounterFormData(reqId);

            Request r = _adminInterface.ValidateRequest(reqId);

            RequestClient rc = _adminInterface.ValidateRequestClient(r.RequestClientId);
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
            return View(efm);
        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function called when we submit the encounter form
        public IActionResult EncounterFormSubmit(EncounterFormModel model)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            int requestId = (int)model.reqId;
            if (requestId != null)
            {
                Request r = _adminInterface.ValidateRequest(requestId);
                RequestClient rc = _adminInterface.ValidateRequestClient(r.RequestClientId);
                if (rc != null)
                {
                    _adminInterface.UpdateEncounterFormData(model, rc);

                }
            }
            TempData["success"] = "Welcome again!";
            return RedirectToAction("AdminDashboard");

        }

        [CustomAuthorize("Admin")]
        // function to return view of close case
        public IActionResult CloseCase(int id)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

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

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function called when we case is closed
        public IActionResult ClickOnCloseCase(int id)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

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
            return View("AdminDashboard");
        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function to store edited info of patient from Close Case view
        public IActionResult CloseCaseSubmitAction(CloseCaseModel model, int id)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 1;

            Request r = _adminInterface.ValidateRequest(id);

            RequestClient rc = _adminInterface.ValidateRequestClient(r.RequestClientId);
            rc.Email = model.email;
            rc.PhoneNumber = model.phoneNumber;
            _adminInterface.UpdateRequestClient(rc);

            TempData["success"] = "Updated data";
            return RedirectToAction("CloseCase", new { id });
        }

        [CustomAuthorize("Admin")]
        // function to return Admin Profile view
        public IActionResult MyProfile()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 3;

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

        [CustomAuthorize("Admin")]
        // function called when admin resets the password from My Profile page
        public IActionResult ProfilePasswordReset(AdminProfile model, int aid)
        {
            AspNetUser anur = _adminInterface.GetAspNetFromAdminId(aid);
            _adminInterface.AdminResetPassword(anur, model.Password);
            TempData["success"] = "Password Updated Successfully";
            return RedirectToAction("MyProfile");
        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function called to submit the changes made in Administrator Info section of Admin Profile
        public IActionResult ProfileAdministratorInfo(AdminProfile model, int aid, string selectedRegion)
        {
            string[] regionArr = selectedRegion.Split(',');
            char[] rId = selectedRegion.ToCharArray();

            _adminInterface.UpdateAdminDataFromId(model, aid, selectedRegion);

            TempData["success"] = "Administrator info updated successfully";
            return RedirectToAction("MyProfile");
        }

        [CustomAuthorize("Admin")]
        // function called to submit the changes made in Mailing Info of Admin Profile
        public IActionResult ProfileMailingInfo(AdminProfile model, int aid)
        {
            _adminInterface.UpdateMailingInfo(model, aid);
            TempData["success"] = "Mailing info updated successfully";
            return RedirectToAction("MyProfile");
        }

        [CustomAuthorize("Admin")]
        // function to return Patient History view
        public IActionResult PatientHistory()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 14;
            PatientHistoryViewModel pr = _adminInterface.PatientHistoryFilteredData(an, null, null, null, null);
            return View(pr);
        }

        [CustomAuthorize("Admin")]
        // function to filter the records of Patient History
        public IActionResult PatientHistoryFilter(string? firstName = "", string? lastName = "", string? email = "", string? phoneNumber = "", int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 14;
            PatientHistoryViewModel pr = _adminInterface.PatientHistoryFilteredData(an, firstName, lastName, phoneNumber, email, page, pageSize);
            return PartialView("PatientHistoryPagePartialView", pr);
        }

        [CustomAuthorize("Admin")]
        // function to return Patient Records view
        public IActionResult PatientRecords(int userid)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 14;
            PatientHistoryViewModel pr = _adminInterface.PatientRecordsData(userid, an);
            return View(pr);
        }

        [CustomAuthorize("Admin")]
        // function to return Provider Menu view
        public IActionResult ProviderMenu()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 5;
            ProviderMenuViewModel pm = _adminInterface.ProviderMenuFilteredData(an, null);
            return View(pm);
        }

        [CustomAuthorize("Admin")]
        // function to filter the records of Provider Menu view
        public IActionResult ProviderMenuFilter(int? region = -1, int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 5;
            ProviderMenuViewModel pm = _adminInterface.ProviderMenuFilteredData(an, region, page, pageSize);
            return PartialView("ProviderMenuPartialView", pm);
        }

        [CustomAuthorize("Admin")]
        // function called when we change the checkbox value of a record of Provider Menu view
        public IActionResult ChangeNotificationValue(int id)
        {
            _adminInterface.ChangeNotificationValue(id);
            TempData["success"] = "Notification status updated successfully";
            return RedirectToAction("ProviderMenu");
        }

        [CustomAuthorize("Admin")]
        // function called when we submit the Contact Your Provider modal
        public IActionResult SendMessageToPhysician(string sendType, string email, string message)
        {
            int count = 1;
            bool isSent = false;
            if (sendType == "Email" || sendType == "Both" || (count <= 3 && !isSent))
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
                    string resetLink = $"{Request.Scheme}://{Request.Host}/Login/SubmitRequestScreen?token={resetToken}";



                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, "HalloDoc"),
                        Subject = "Contact Your Provider",
                        IsBodyHtml = true,
                        Body = $"{message}"
                    };

                    if (email != "")
                    {
                        mailMessage.To.Add(email);
                        client.SendMailAsync(mailMessage);
                        count++;
                        isSent = true;
                        TempData["success"] = "Email sent successfully";
                        return RedirectToAction("ProviderMenu");
                    }

                    else
                    {
                        ModelState.AddModelError("Email", "Invalid Email");
                        return RedirectToAction("ProviderMenu");
                    }
                }
                catch (Exception ex)
                {
                    return RedirectToAction("ProviderMenu");
                }
            }

            else
            {
                ModelState.AddModelError("Email", "Invalid Email");
                return RedirectToAction("ProviderMenu");
            }

        }

        [CustomAuthorize("Admin")]
        // function to return Create Provider Account view
        public IActionResult CreateProviderAccount()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 5;
            EditProviderAccountViewModel ep = new EditProviderAccountViewModel();
            ep.adminNavbarModel = an;
            ep.regions = _adminInterface.GetAllRegion();
            ep.allRoles = _adminInterface.GetAllRoles();
            return View(ep);
        }

        [CustomAuthorize("Admin")]
        public IActionResult CreateNewProviderAccount(EditProviderAccountViewModel model, List<int> regionNames)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 5;
            _adminInterface.CreateNewProviderAccount(model, regionNames, ad.AdminId);
            TempData["success"] = "Provider account created successfully";
            return RedirectToAction("CreateProviderAccount");
        }

        // function to return Edit Provider Account view
        public IActionResult EditProviderAccount(int id)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 5;
            EditProviderAccountViewModel ep = _adminInterface.ProviderEditAccount(id, an);
            return View(ep);
        }

        [CustomAuthorize("Admin")]
        // function to save changed password of provider
        public IActionResult SavePasswordOfProvider(EditProviderAccountViewModel ep)
        {
            _adminInterface.SavePasswordOfPhysician(ep);
            return RedirectToAction("EditProviderAccount", new { id = ep.PhysicianId });
        }

        [CustomAuthorize("Admin")]
        // function to save changes of billing info of provider
        public IActionResult EditProviderBillingInfo(EditProviderAccountViewModel ep)
        {
            _adminInterface.EditProviderBillingInfo(ep);
            return RedirectToAction("EditProviderAccount", new { id = ep.PhysicianId });
        }

        [CustomAuthorize("Admin")]
        // function to save provider info
        public IActionResult SaveProviderProfile(EditProviderAccountViewModel ep, string selectedRegionsList)
        {
            _adminInterface.SaveProviderProfile(ep, selectedRegionsList);
            return RedirectToAction("EditProviderAccount", new { id = ep.PhysicianId });
        }

        [HttpPost]
        // function called when we upload file of signature of provider
        public IActionResult SetContentOfPhysician(IFormFile file, int PhysicianId, bool IsSignature)
        {
            _adminInterface.SetContentOfPhysician(file, PhysicianId, IsSignature);
            return RedirectToAction("EditProviderAccount", new { id = PhysicianId });
        }

        // function to upload all other docs of provider
        public IActionResult SetAllDocOfPhysician(IFormFile file, int PhysicianId, int num)
        {
            _adminInterface.SetAllDocOfPhysician(file, PhysicianId, num);
            return RedirectToAction("EditProviderAccount", new { id = PhysicianId });
        }

        // function to save changes of provider profile
        public IActionResult PhysicianProfileUpdate(EditProviderAccountViewModel model)
        {
            _adminInterface.PhysicianProfileUpdate(model);
            return RedirectToAction("EditProviderAccount", new { id = model.PhysicianId });
        }



        public IActionResult DeletePhysicianAccount(int id)
        {
            _adminInterface.DeletePhysicianAccount(id);
            TempData["success"] = "Account deleted successfully";
            return RedirectToAction("ProviderMenu");
        }

        [CustomAuthorize("Admin")]
        // function to return Create Role view
        public IActionResult CreateRole()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 9;
            CreateRoleViewModel cr = new CreateRoleViewModel
            {
                adminNavbarModel = an,
                allRoles = _adminInterface.GetAllMenus(),
            };
            return View(cr);
        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function to create a new role
        public IActionResult CreateNewRole(string roleName, string acType, string menuIdString)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 9;
            List<int> menuIds = null;
            if (!string.IsNullOrEmpty(menuIdString))
            {
                menuIds = menuIdString.Split(',').Select(int.Parse).ToList();
            }
            _adminInterface.CreateNewRole2(roleName, acType, an.Admin_Name, menuIds);
            TempData["success"] = "New role created";
            return RedirectToAction("CreateRole");
        }

        [CustomAuthorize("Admin")]
        // function to return view of Account Access page
        public IActionResult AccountAccess()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 9;
            CreateRoleViewModel cr = new CreateRoleViewModel
            {
                adminNavbarModel = an,
                allRoles = _adminInterface.GetAllMenus(),
                roles = _adminInterface.GetAllRoles(),
            };
            return View(cr);
        }

        [CustomAuthorize("Admin")]
        // function to delete a role
        public IActionResult DeleteRole(int roleid)
        {
            _adminInterface.DeleteRoleFromId(roleid);
            TempData["success"] = "Role deleted successfully";
            return RedirectToAction("AccountAccess");
        }

        [CustomAuthorize("Admin")]
        // function to return Edit Role view
        public IActionResult EditRole(int roleid)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 9;
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

        [HttpPost]
        [CustomAuthorize("Admin")]
        // function to edit a role
        public IActionResult EditRoleSubmit(string menuIdString, int roleid)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 9;
            List<int> menuIds = null;
            if (!string.IsNullOrEmpty(menuIdString))
            {
                menuIds = menuIdString.Split(',').Select(int.Parse).ToList();
            }
            _adminInterface.EditRoleSubmitAction(roleid, menuIds);
            TempData["success"] = "Role edited successfully";
            return RedirectToAction("AccountAccess");
        }

        [CustomAuthorize("Admin")]
        public IActionResult CreateAdminAccount()
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 5;
            AdminProfile ap = new AdminProfile();
            ap.an = an;
            ap.allRoles = _adminInterface.GetAllRoles();
            ap.allRegions = _adminInterface.GetAllRegions();
            return View(ap);
        }

        [CustomAuthorize("Admin")]
        public IActionResult CreateNewAdminAccount(EditProviderAccountViewModel model, List<int> regionNames)
        {
            var userId = HttpContext.Session.GetInt32("id");
            Admin ad = _adminInterface.GetAdminFromId((int)userId);
            AdminNavbarModel an = new AdminNavbarModel();
            an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
            an.Tab = 5;
            _adminInterface.CreateNewProviderAccount(model, regionNames, ad.AdminId);
            TempData["success"] = "Provider account created successfully";
            return RedirectToAction("CreateProviderAccount");
        }

    }
}

