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
//using System.Diagnostics;
//using HalloDoc.Data;

namespace HalloDoc.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AdminDashboard(string? status)
        {
            var count_new = _context.Requests.Count(r => r.Status == 1);
            var count_pending = _context.Requests.Count(r => r.Status == 2);
            var count_active = _context.Requests.Count(r => r.Status == 3);
            var count_conclude = _context.Requests.Count(r => r.Status == 4);
            var count_toclose = _context.Requests.Count(r => r.Status == 5);
            var count_unpaid = _context.Requests.Count(r => r.Status == 6);

            AdminDashboardTableView adminDashboardViewModel = new AdminDashboardTableView
            {
                new_count = count_new,
                pending_count = count_pending,
                active_count = count_active,
                conclude_count = count_conclude,
                unpaid_count = count_unpaid,
                toclose_count = count_toclose,
                query_requests = _context.Requests.Include(r => r.RequestWiseFiles).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 1),
                requests = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 1).ToList(),
                regions = _context.Regions.ToList(),
                status = "New",
            };
            
            return View(adminDashboardViewModel);
        }

        

        //[HttpPost]
        public IActionResult New()
        {
            var count_new = _context.Requests.Count(r => r.Status == 1);
            var count_pending = _context.Requests.Count(r => r.Status == 2);
            var count_active = _context.Requests.Count(r => r.Status == 3);
            var count_conclude = _context.Requests.Count(r => r.Status == 4);
            var count_toclose = _context.Requests.Count(r => r.Status == 5);
            var count_unpaid = _context.Requests.Count(r => r.Status == 6);

            IQueryable<Request> req = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 1);
            List<Request> list = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 1).ToList();
            List<Region> region = _context.Regions.ToList();

            AdminDashboardTableView adminDashboardViewModel = new AdminDashboardTableView
            {
                new_count = count_new,
                pending_count = count_pending,
                active_count = count_active,
                conclude_count = count_conclude,
                toclose_count = count_toclose,
                unpaid_count = count_unpaid,
                query_requests = req,
                requests = list,
                regions = region,
                status = "New",
            };
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        public IActionResult Pending()
        {
            var count_new = _context.Requests.Count(r => r.Status == 1);
            var count_pending = _context.Requests.Count(r => r.Status == 2);
            var count_active = _context.Requests.Count(r => r.Status == 3);
            var count_conclude = _context.Requests.Count(r => r.Status == 4);
            var count_toclose = _context.Requests.Count(r => r.Status == 5);
            var count_unpaid = _context.Requests.Count(r => r.Status == 6);

            IQueryable<Request> req = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 2);
            List<Request> list = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 2).ToList();
            List<Region> region = _context.Regions.ToList();

            AdminDashboardTableView adminDashboardViewModel = new AdminDashboardTableView
            {
                new_count = count_new,
                pending_count = count_pending,
                active_count = count_active,
                conclude_count = count_conclude,
                unpaid_count = count_unpaid,
                toclose_count = count_toclose,
                query_requests = req,
                requests = list,
                regions = region,
                status = "Pending",
            };
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        public IActionResult Active()
        {
            var count_new = _context.Requests.Count(r => r.Status == 1);
            var count_pending = _context.Requests.Count(r => r.Status == 2);
            var count_active = _context.Requests.Count(r => r.Status == 3);
            var count_conclude = _context.Requests.Count(r => r.Status == 4);
            var count_toclose = _context.Requests.Count(r => r.Status == 5);
            var count_unpaid = _context.Requests.Count(r => r.Status == 6);

            IQueryable<Request> req = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 3);
            List<Request> list = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 3).ToList();
            List<Region> region = _context.Regions.ToList();

            AdminDashboardTableView adminDashboardViewModel = new AdminDashboardTableView
            {
                new_count = count_new,
                pending_count = count_pending,
                active_count = count_active,
                conclude_count = count_conclude,
                unpaid_count = count_unpaid,
                toclose_count = count_toclose,
                query_requests = req,
                requests = list,
                regions = region,
                status = "Active",
            };
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        public IActionResult Conclude()
        {
            var count_new = _context.Requests.Count(r => r.Status == 1);
            var count_pending = _context.Requests.Count(r => r.Status == 2);
            var count_active = _context.Requests.Count(r => r.Status == 3);
            var count_conclude = _context.Requests.Count(r => r.Status == 4);
            var count_toclose = _context.Requests.Count(r => r.Status == 5);
            var count_unpaid = _context.Requests.Count(r => r.Status == 6);

            IQueryable<Request> req = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 4);
            List<Request> list = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 4).ToList();
            List<Region> region = _context.Regions.ToList();

            AdminDashboardTableView adminDashboardViewModel = new AdminDashboardTableView
            {
                new_count = count_new,
                pending_count = count_pending,
                active_count = count_active,
                conclude_count = count_conclude,
                unpaid_count = count_unpaid,
                toclose_count = count_toclose,
                query_requests = req,
                requests = list,
                regions = region,
                status = "Conclude",
            };
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        public IActionResult Toclose()
        {
            var count_new = _context.Requests.Count(r => r.Status == 1);
            var count_pending = _context.Requests.Count(r => r.Status == 2);
            var count_active = _context.Requests.Count(r => r.Status == 3);
            var count_conclude = _context.Requests.Count(r => r.Status == 4);
            var count_toclose = _context.Requests.Count(r => r.Status == 5);
            var count_unpaid = _context.Requests.Count(r => r.Status == 6);

            IQueryable<Request> req = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 5);
            List<Request> list = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 5).ToList();
            List<Region> region = _context.Regions.ToList();

            AdminDashboardTableView adminDashboardViewModel = new AdminDashboardTableView
            {
                new_count = count_new,
                pending_count = count_pending,
                active_count = count_active,
                conclude_count = count_conclude,
                unpaid_count = count_unpaid,
                toclose_count = count_toclose,
                query_requests = req,
                requests = list,
                regions = region,
                status = "ToClose",
            };
            return PartialView("AdminDashboardTablePartialView", adminDashboardViewModel);
        }

        //[HttpPost]
        public IActionResult Unpaid()
        {
            var count_new = _context.Requests.Count(r => r.Status == 1);
            var count_pending = _context.Requests.Count(r => r.Status == 2);
            var count_active = _context.Requests.Count(r => r.Status == 3);
            var count_conclude = _context.Requests.Count(r => r.Status == 4);
            var count_toclose = _context.Requests.Count(r => r.Status == 5);
            var count_unpaid = _context.Requests.Count(r => r.Status == 6);

            IQueryable<Request> req = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 6);
            List<Request> list = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 6).ToList();
            List<Region> region = _context.Regions.ToList();

            AdminDashboardTableView adminDashboardViewModel = new AdminDashboardTableView
            {
                new_count = count_new,
                pending_count = count_pending,
                active_count = count_active,
                conclude_count = count_conclude,
                unpaid_count = count_unpaid,
                toclose_count = count_toclose,
                query_requests = req,
                requests = list,
                regions = region,
                status = "Unpaid",
            };
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
            var request = _context.Requests.Where(r => r.RequestId == requestId).FirstOrDefault();
            var user = _context.RequestClients.FirstOrDefault(s => s.RequestClientId == request.RequestClientId);
            int intYear = (int)user.IntYear;
            int intDate = (int)user.IntDate;
            string month = user.StrMonth;
            var mon = 0;
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
            DateTime date = new DateTime(intYear, (int)mon, intDate);
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
            int requestId = userProfile.RequestId;
            if (requestId != null)
            {
                var rid = _context.Requests.Where(u => u.RequestId == requestId).FirstOrDefault();
                var userToUpdate = _context.RequestClients.Where(u => u.RequestClientId == rid.RequestClientId).FirstOrDefault();
                if (userToUpdate != null)
                {
                    userToUpdate.FirstName = userProfile.FirstName;
                    userToUpdate.LastName = userProfile.LastName;
                    userToUpdate.PhoneNumber = userProfile.PhoneNumber;
                    userToUpdate.Email = userProfile.Email;
                    userToUpdate.IntDate = userProfile.DOB.Day;
                    userToUpdate.IntYear = userProfile.DOB.Year;
                    userToUpdate.StrMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(userProfile.DOB.Month);
                    _context.RequestClients.Update(userToUpdate);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("ViewCase", new { requestId = requestId });
        }

       public IActionResult ViewNotes(int requestId)
       {
            Request r = _context.Requests.Where(r => r.RequestId == requestId).FirstOrDefault();
            RequestNote rn = _context.RequestNotes.FirstOrDefault(r => r.RequestId == requestId);
            RequestStatusLog rsl = _context.RequestStatusLogs.FirstOrDefault(r => r.RequestId == requestId);

            int id = (int)rsl.PhysicianId;

            Physician py = _context.Physicians.FirstOrDefault(p => p.PhysicianId == id);

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
            RequestNote rn = _context.RequestNotes.FirstOrDefault(rq => rq.RequestId == model.RequestId);

            rn.AdminNotes = model.AdminNotes;
            _context.RequestNotes.Update(rn);
            _context.SaveChanges();

            return RedirectToAction("ViewNotes", new { requestId = model.RequestId });

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
    }
}
