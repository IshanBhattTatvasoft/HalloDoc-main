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
using HalloDoc.DataLayer.Data;
using HalloDoc.LogicLayer.Patient_Interface;
using static HalloDoc.DataLayer.Models.Enums;
using System.Collections;
using HalloDoc.LogicLayer.Patient_Repository;
using System.Net.Mail;
using System.Net;
//using System.Diagnostics;
//using HalloDoc.Data;

namespace HalloDoc.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAdminInterface _adminInterface;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, IAdminInterface adminInterface)
        {
            _context = context;
            _adminInterface = adminInterface;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlatformLoginPage(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                AspNetUser user = _adminInterface.ValidateAspNetUser(model);
                if (user != null)
                {
                    if (model.PasswordHash == user.PasswordHash)
                    {
                        User user2 = _adminInterface.ValidateUser(model);
                        HttpContext.Session.SetInt32("id", user2.UserId);
                        HttpContext.Session.SetString("name", user2.FirstName);
                        HttpContext.Session.SetString("IsLoggedIn", "true");
                        return RedirectToAction("AdminDashboard");
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

        public IActionResult AdminDashboard(string? status)
        {
            //var count_new = _context.Requests.Count(r => r.Status == 1);
            //var count_pending = _context.Requests.Count(r => r.Status == 2);
            //var count_active = _context.Requests.Count(r => r.Status == 3);
            //var count_conclude = _context.Requests.Count(r => r.Status == 4);
            //var count_toclose = _context.Requests.Count(r => r.Status == 5);
            //var count_unpaid = _context.Requests.Count(r => r.Status == 6);

            //AdminDashboardTableView adminDashboardViewModel = new AdminDashboardTableView
            //{
            //    new_count = count_new,
            //    pending_count = count_pending,
            //    active_count = count_active,
            //    conclude_count = count_conclude,
            //    unpaid_count = count_unpaid,
            //    toclose_count = count_toclose,
            //    query_requests = _context.Requests.Include(r => r.RequestWiseFiles).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 1),
            //    requests = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 1).ToList(),
            //    regions = _context.Regions.ToList(),
            //    status = "New",
            //    caseTags = _context.CaseTags.ToList()
            //};

            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard(status);


            return View(adminDashboardViewModel);
        }



        //[HttpPost]
        public IActionResult New()
        {
            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("New");
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        public IActionResult Pending()
        {
            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Pending");

            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        public IActionResult Active()
        {
            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Active");

            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        public IActionResult Conclude()
        {
            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Conclude");

            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        public IActionResult Toclose()
        {
            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("ToClose");

            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        public IActionResult Unpaid()
        {
            AdminDashboardTableView adminDashboardViewModel = _adminInterface.ModelOfAdminDashboard("Unpaid");

            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }
        public List<Request> GetTableData()
        {
            List<Request> data = new List<Request>();
            //var user_id = HttpContext.Session.GetInt32("id");
            //data = _context.Requests.Include(r => r.RequestClient).Where(u => u.UserId == user_id).ToList();
            data = _context.Requests.Include(r => r.RequestClient).ToList();
            return data;
        }

        public IActionResult DownloadAll()
        {
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
                        statusClass = "business";
                    }
                    else if (item.RequestTypeId == 4)
                    {
                        statusClass = "patient";
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

        public IActionResult ViewCase(int requestId)
        {
            //var data = _context.Requests.Include(u => u.RequestClient).FirstOrDefault(u => u.RequestId == requestId);
            //var user = _context.RequestClients.Include(v => v.Requests).FirstOrDefault(v => v.RequestClientId == data.RequestClientId);
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
                DOB = date
            };

            return View(viewCase);
        }


        [HttpPost]
        public IActionResult EditViewCase(ViewCaseModel userProfile)
        {
            int requestId = (int)userProfile.RequestId;
            if (requestId != null)
            {
                Request rid = _adminInterface.ValidateRequest(requestId);
                RequestClient userToUpdate = _adminInterface.ValidateRequestClient(rid.RequestClientId);
                if (userToUpdate != null)
                {
                    //userToUpdate.FirstName = userProfile.FirstName;
                    //userToUpdate.LastName = userProfile.LastName;
                    //userToUpdate.PhoneNumber = userProfile.PhoneNumber;
                    //userToUpdate.Email = userProfile.Email;
                    //userToUpdate.IntDate = userProfile.DOB.Day;
                    //userToUpdate.IntYear = userProfile.DOB.Year;
                    //userToUpdate.StrMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(userProfile.DOB.Month);
                    //_context.RequestClients.Update(userToUpdate);
                    //_context.SaveChanges();
                    _adminInterface.EditViewCaseAction(userProfile, userToUpdate);
                }
            }
            return RedirectToAction("ViewCase", new { requestId = requestId });
        }

        public IActionResult ViewNotes(int requestId)
        {
            Request r = _adminInterface.ValidateRequest(requestId);
            RequestNote rn = _adminInterface.FetchRequestNote(requestId);
            RequestStatusLog rsl = _adminInterface.FetchRequestStatusLogs(requestId);

            int id = (int)rsl.PhysicianId;

            Physician py = _adminInterface.FetchPhysician(id);

            var viewModel = new ViewNotes
            {
                AdminNotes = rn.AdminNotes,
                PhysicianNotes = rn.PhysicianNotes,
                PhyName = py.FirstName,
                Notes = rsl.Notes,
                CreatedDate = rsl.CreatedDate,
                RequestId = requestId,
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult EditViewNotes(ViewNotes model)
        {
            //int id = model.RequestId;
            RequestNote rn = _adminInterface.FetchRequestNote(model.RequestId);

            //rn.AdminNotes = model.AdminNotes;
            //_context.RequestNotes.Update(rn);
            //_context.SaveChanges();
            _adminInterface.EditViewNotesAction(rn, model);

            return RedirectToAction("ViewNotes", new { requestId = model.RequestId });

        }

        [HttpPost]
        public IActionResult CancelCase(AdminDashboardTableView model, int selectedCaseTagId, string additionalNotes)
        {
            CaseTag ct = _adminInterface.FetchCaseTag(selectedCaseTagId);
            Request r = _adminInterface.ValidateRequest(model.RequestId);
            r.CaseTag = ct.Name;
            r.Status = 3;
            RequestStatusLog rs = new RequestStatusLog();
            rs.RequestId = model.RequestId;
            rs.Notes = additionalNotes;
            rs.Status = 3;
            rs.CreatedDate = DateTime.Now;
            //_context.RequestStatusLogs.Add(rs);
            //_context.SaveChanges();
            _adminInterface.AddRequestStatusLogFromCancelCase(rs);
            TempData["success"] = "Case cancelled successfully";

            return RedirectToAction("AdminDashboard");
        }

        public List<Physician> GetPhysicianByRegion(AdminDashboardTableView model, int RegionId)
        {
            List<Physician> p = _adminInterface.FetchPhysicianByRegion(RegionId);
            return p;
        }

        [HttpPost]
        public IActionResult AssignCaseSubmitAction(AdminDashboardTableView model, string assignCaseDescription, int selectedPhysicianId)
        {
            RequestStatusLog rsl = new RequestStatusLog();
            Request r = _adminInterface.ValidateRequest(model.RequestId);
            r.Status = 1; //when a case is assigned, status is set to 1 currently
            // but when the assigned case gets accepted, then its status can be 2 and will be shown in Pending state.
            r.PhysicianId = selectedPhysicianId;
            rsl.RequestId = model.RequestId;
            rsl.Notes = assignCaseDescription;
            rsl.Status = 1;
            rsl.CreatedDate = DateTime.Now;
            rsl.TransToPhysicianId = selectedPhysicianId;
            rsl.PhysicianId = selectedPhysicianId;
            //_context.RequestStatusLogs.Add(rsl);
            //_context.SaveChanges();
            _adminInterface.AddRequestStatusLogFromCancelCase(rsl);
            _adminInterface.UpdateRequest(r);
            TempData["success"] = "Successfully requested to assign the case";
            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        public IActionResult BlockCase(AdminDashboardTableView model, string reasonForBlockRequest)
        {
            Request r = _adminInterface.ValidateRequest(model.RequestId);
            RequestStatusLog rs = new RequestStatusLog();
            rs.Status = 11;
            rs.CreatedDate = DateTime.Now;
            rs.Notes = reasonForBlockRequest;
            rs.RequestId = model.RequestId;
            //_context.RequestStatusLogs.Add(rs);
            //_context.SaveChanges();
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
        public async Task<IActionResult> CreateRequest(AdminCreateRequestModel model)
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

            if (ModelState.IsValid)
            {
                _adminInterface.InsertDataOfRequest(model);
            }
            TempData["success"] = "Request created successfully";
            return View("CreateRequest");
        }

        public IActionResult VerifyLocation(string state)
        {
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

        public IActionResult CreateRequest()
        {
            return View();
        }

        public IActionResult PlatformLoginPage()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult modal_check()
        {
            return View();
        }

        public IActionResult ViewUploads(int requestid)
        {
            Request request = _adminInterface.ValidateRequest(requestid);
            User user = _adminInterface.ValidateUserByRequestId(request);
            List<RequestWiseFile> rwf = _adminInterface.GetFileData(requestid);
            ViewUploadsModel vum = new ViewUploadsModel()
            {
                confirmation_number = request.ConfirmationNumber,
                requestId = requestid,
                user = user,
                requestWiseFiles = rwf
            };
            return View(vum);
        }

        [HttpPost]
        public IActionResult SetImageContent(ViewUploadsModel model, int requestId)
        {
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
                //_context.RequestWiseFiles.Add(requestWiseFile);
                //_context.SaveChanges();
                _adminInterface.AddFile(requestWiseFile);
            }

            return RedirectToAction("ViewUploads", new { requestID = model.requestId });
        }

        public IActionResult DeleteIndividual(int id)
        {
            RequestWiseFile rwf = _context.RequestWiseFiles.Where(r => r.RequestWiseFileId == id).FirstOrDefault();
            rwf.IsDeleted = new BitArray(1, true);
            _context.SaveChanges();
            return RedirectToAction("ViewUploads", new { requestID = rwf.RequestId });
        }

        public IActionResult DeleteMultiple(int requestid, string fileId)
        {
            RequestWiseFile rwf = _context.RequestWiseFiles.Where(r => r.RequestId == requestid).FirstOrDefault();
            string[] fileid = fileId.Split(',').Select(x => x.Trim()).ToArray();
            for (int i = 0; i < fileid.Length; i++)
            {
                RequestWiseFile r = _context.RequestWiseFiles.Where(r => r.RequestWiseFileId == int.Parse(fileid[i])).FirstOrDefault();
                r.IsDeleted = new BitArray(1, true);
            }
            _context.SaveChanges();
            TempData["success"] = "File(s) deleted successfully";
            return RedirectToAction("ViewUploads", new { requestID = requestid });
        }

        public IActionResult SendSelectedFiles(int requestid, string fileName)
        {
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
                Request r = _context.Requests.Where(r => r.RequestId == requestid).FirstOrDefault();
                RequestClient rc = _context.RequestClients.Where(cl => cl.RequestClientId == r.RequestClientId).FirstOrDefault();
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
                    return RedirectToAction("ForgotPassword");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ForgotPassword");
            }
        }

        public async Task<IActionResult> SendLink(AdminDashboardTableView model)
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
                    Subject = "Create Request For Patient",
                    IsBodyHtml = true,
                    Body = $"Hey {model.FirstName + " " + model.LastName} !! Please click the following link to reset your password: <a href='{resetLink}'>Click Here</a>"
                };
                AspNetUser user = _adminInterface.ValidAspNetUser(model.email);
                if (user != null)
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

        public IActionResult Orders(int id)
        {
            List<HealthProfessionalType> hPT = _context.HealthProfessionalTypes.ToList();
            List<HealthProfessional> hP = _context.HealthProfessionals.ToList();
            SendOrder so = new SendOrder
            {
                hpType = hPT,
                hp = hP,
                requestId = id
            };
            return View(so);
        }

        public List<HealthProfessional> GetBusinessData(int professionId, SendOrder model)
        {
            List<HealthProfessional> healthProfessionals = _context.HealthProfessionals.Where(h => h.Profession == professionId).ToList();
            return healthProfessionals;
        }
    }
}
