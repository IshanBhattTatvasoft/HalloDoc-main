using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using HalloDoc.DataLayer.Data;
using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using HalloDocMvc.Entity.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class AdminInterface : IAdminInterface
    {
        private readonly ApplicationDbContext _context;

        public AdminInterface(ApplicationDbContext context)
        {
            _context = context;
        }

        AdminDashboardTableView IAdminInterface.ModelOfAdminDashboard(string status, int id, string? search, string? requestor, int? region, int page = 1, int pageSize = 10)
        {
            int count_new = 0;
            int count_pending = 0;
            int count_active = 0;
            int count_conclude = 0;
            int count_toclose = 0;
            int count_unpaid = 0;

            Expression<Func<Request, bool>> exp;
            if (status == "New")
            {
                exp = r => r.Status == 1;
            }
            else if (status == "Pending")
            {
                exp = r => r.Status == 2;
            }
            else if (status == "Active")
            {
                exp = r => r.Status == 5 || r.Status == 4;
            }
            else if (status == "Conclude")
            {
                exp = r => r.Status == 6;
            }
            else if (status == "ToClose")
            {
                exp = r => r.Status == 3 || r.Status == 7 || r.Status == 8;
            }
            else
            {
                exp = r => r.Status == 9;
            }

            Admin ad = GetAdminFromId((int)id);
            Physician p = GetPhysicianFromId((int)id);
            AdminNavbarModel an = new AdminNavbarModel();
            if (ad != null)
            {
                an.Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName);
                an.roleName = "Admin";
                count_new = _context.Requests.Count(r => r.Status == 1);
                count_pending = _context.Requests.Count(r => r.Status == 2);
                count_active = _context.Requests.Count(r => r.Status == 4 || r.Status == 5);
                count_conclude = _context.Requests.Count(r => r.Status == 6);
                count_toclose = _context.Requests.Count(r => r.Status == 3 || r.Status == 7 || r.Status == 8);
                count_unpaid = _context.Requests.Count(r => r.Status == 9);
            }
            else
            {
                an.Admin_Name = string.Concat(p.FirstName, " ", p.LastName);
                an.roleName = "Provider";
                count_new = _context.Requests.Count(r => r.Status == 1 && r.PhysicianId == p.PhysicianId);
                count_pending = _context.Requests.Count(r => r.Status == 2 && r.PhysicianId == p.PhysicianId);
                count_active = _context.Requests.Count(r => (r.Status == 4 || r.Status == 5) && r.PhysicianId == p.PhysicianId);
                count_conclude = _context.Requests.Count(r => r.Status == 6 && r.PhysicianId == p.PhysicianId);
            }
            an.Tab = 1;


            List<HalloDoc.DataLayer.Models.Region> r = _context.Regions.ToList();
            List<CaseTag> c = _context.CaseTags.ToList();

            IQueryable<Request> query = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Include(e => e.EncounterForms).Where(exp).OrderByDescending(e => e.CreatedDate);

            if (search != null && search != "")
            {
                query = query.Where(r => r.RequestClient.FirstName.ToLower().Contains(search.ToLower()) || r.RequestClient.LastName.ToLower().Contains(search.ToLower()));
            }

            if (requestor == "Patient")
            {
                query = query.Where(r => r.RequestTypeId == 1);
            }

            if (requestor == "Family")
            {
                query = query.Where(r => r.RequestTypeId == 2);
            }

            if (requestor == "Concierge")
            {
                query = query.Where(r => r.RequestTypeId == 3);
            }

            if (requestor == "Business")
            {
                query = query.Where(r => r.RequestTypeId == 4);
            }
            if (region != null && region != -1)
            {
                query = query.Where(r => r.RequestClient.RegionId == region);
            }

            if (p != null)
            {
                query = query.Where(r => r.PhysicianId == p.PhysicianId);
            }

            AdminDashboardTableView adminDashboardViewModel = new AdminDashboardTableView
            {
                new_count = count_new,
                pending_count = count_pending,
                active_count = count_active,
                conclude_count = count_conclude,
                unpaid_count = count_unpaid,
                toclose_count = count_toclose,
                regions = r,
                status = status,
                caseTags = c,
                email = "abc",
                requests = query.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                an = an,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = query.Count(),
                TotalPages = (int)Math.Ceiling((double)query.Count() / pageSize),
            };
            return adminDashboardViewModel;
        }

        PatientHistoryViewModel IAdminInterface.PatientHistoryFilteredData(AdminNavbarModel an, string fname, string lname, string pno, string email, int page = 1, int pageSize = 10)
        {
            IQueryable<Request> query = _context.Requests.Include(r => r.RequestClient);
            if (fname != null)
            {
                query = query.Where(r => r.RequestClient.FirstName.ToLower().Contains(fname.ToLower()));
            }
            if (lname != null)
            {
                query = query.Where(r => r.RequestClient.LastName.ToLower().Contains(lname.ToLower()));
            }
            if (pno != null)
            {
                query = query.Where(r => r.RequestClient.PhoneNumber.Contains(pno));
            }
            if (email != null)
            {
                query = query.Where(r => r.RequestClient.Email.Contains(email));
            }
            PatientHistoryViewModel ph = new PatientHistoryViewModel
            {
                AdminNavbarModel = an,
                requests = query.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = query.Count(),
                TotalPages = (int)Math.Ceiling((double)query.Count() / pageSize),
            };
            return ph;
        }

        PatientHistoryViewModel IAdminInterface.PatientRecordsData(int userid, AdminNavbarModel an, int page = 1, int pageSize = 10)
        {
            IQueryable<Request> query = _context.Requests.Where(r => r.UserId == userid);
            PatientHistoryViewModel pr = new PatientHistoryViewModel
            {
                AdminNavbarModel = an,
                requests = query.ToList(),
                p = GetAllPhysicians(),
                Rwf = GetAllFiles(),
                userId = userid
            };
            return pr;
        }

        PatientHistoryViewModel IAdminInterface.PatientRecordsFilteredData(int userid, AdminNavbarModel an, int page = 1, int pageSize = 10)
        {
            IQueryable<Request> query = _context.Requests.Where(r => r.UserId == userid);
            PatientHistoryViewModel pr = new PatientHistoryViewModel
            {
                AdminNavbarModel = an,
                requests = query.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                p = GetAllPhysicians(),
                Rwf = GetAllFiles(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = query.Count(),
                TotalPages = (int)Math.Ceiling((double)query.Count() / pageSize),
            };
            return pr;
        }

        ProviderMenuViewModel IAdminInterface.ProviderMenuFilteredData(AdminNavbarModel an, int? region, int page = 1, int pageSize = 10)
        {
            IQueryable<Physician> phy = _context.Physicians.Include(pn => pn.PhysicianNotifications);

            if (region != null && region != -1)
            {
                phy = phy.Where(p => p.RegionId == region);
            }

            ProviderMenuViewModel pm = new ProviderMenuViewModel
            {
                an = an,
                physician = phy.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                roles = GetAllRoles(),
                regions = GetAllRegion(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = phy.Count(),
                TotalPages = (int)Math.Ceiling((double)phy.Count() / pageSize),
            };
            return pm;
        }

        UserAccessViewModel IAdminInterface.UserAccessFilteredData(AdminNavbarModel an, int accountType)
        {
            UserAccessViewModel ua = new UserAccessViewModel();
            ua.accountType = accountType;
            if (accountType == 1)
            {
                ua.admins = _context.Admins.ToList();
            }
            else if (accountType == 2)
            {
                ua.physicians = _context.Physicians.ToList();
            }
            else
            {
                ua.admins = _context.Admins.ToList();
                ua.physicians = _context.Physicians.ToList();
            }
            ua.adminNavbarModel = an;
            return ua;
        }

        BlockedHistoryViewModel IAdminInterface.BlockedHistoryFilteredData(AdminNavbarModel an, string name, DateOnly date, string email, string phoneNo)
        {
            var query = from b in _context.BlockRequests
                        join r in _context.Requests on b.RequestId equals r.RequestId
                        join rc in _context.RequestClients on r.RequestClientId equals rc.RequestClientId
                        where name == null || rc.FirstName.ToLower().Contains(name.ToLower()) || rc.LastName.ToLower().Contains(name.ToLower())
                        select b;


            DateOnly checkdate = new DateOnly(0001, 1, 1);


            if (email != null || email != "")
            {
                query = query.Where(r => r.Email.ToLower().Contains(email.ToLower()));
            }

            if (phoneNo != null || phoneNo != "")
            {
                query = query.Where(r => r.PhoneNumber.Contains(phoneNo));
            }

            if (date != null && date != checkdate)
            {
                query = query.Where(r => DateOnly.FromDateTime((DateTime)r.CreatedDate).Equals(date));
            }

            List<BlockedHistoryData> allData = new List<BlockedHistoryData>();
            List<BlockRequest> br = query.ToList();

            foreach (var item in br)
            {
                Request r = ValidateRequest(item.RequestId);
                RequestClient rc = GetPatientData(r.RequestId);
                BlockedHistoryData bh = new BlockedHistoryData();
                bh.PhoneNumber = item.PhoneNumber;
                bh.Email = item.Email;
                bh.CreatedDate = DateOnly.FromDateTime((DateTime)item.CreatedDate);
                bh.Notes = item.Reason;
                bh.IsActive = item.IsActive[0];
                bh.PatientName = string.Concat(rc.FirstName, ", ", rc.LastName);
                bh.RequestId = item.RequestId;
                bh.BlockRequestId = item.BlockRequestId;
                allData.Add(bh);
            }

            BlockedHistoryViewModel bhvm = new BlockedHistoryViewModel
            {
                allData = allData,
                adminNavbarModel = an,
            };

            return bhvm;
        }

        SearchRecordsViewModel IAdminInterface.SearchRecordsFilteredData(AdminNavbarModel an, int? page = 1, int? pageSize = 10, int? requestStatus = -1, string? patientName = "", int? requestType = -1, DateTime? fromDate = null, DateTime? toDate = null, string? providerName = "", string? email = "", string? phoneNo = null)
        {
            DateTime temp = new DateTime(1, 1, 1, 0, 0, 0);
            var q = from r in _context.Requests
                    join rc in _context.RequestClients
                    on r.RequestClientId equals rc.RequestClientId
                    select new SearchRecordsTableData
                    {
                        patientName = rc.FirstName + ", " + rc.LastName,
                        requestor = r.RequestTypeId,
                        dateOfService = (DateTime)r.AcceptedDate,
                        closeCaseDate = _context.RequestStatusLogs.Where(rs => rs.RequestId == r.RequestId && rs.Status == 8).OrderBy(rs => rs.CreatedDate).LastOrDefault().CreatedDate.Date,
                        email = rc.Email ?? "-",
                        phoneNumber = rc.PhoneNumber ?? "-",
                        address = rc.Street + ", " + rc.City + ", " + rc.State,
                        zipcode = rc.ZipCode,
                        requestStatus = r.Status,
                        physician = "Dr. " + _context.Physicians.FirstOrDefault(rp => rp.PhysicianId == r.PhysicianId).FirstName ?? "-" + _context.Physicians.FirstOrDefault(rp => rp.PhysicianId == r.PhysicianId).LastName ?? "-",
                        physicianNote = _context.RequestNotes.FirstOrDefault(re => re.RequestId == r.RequestId).PhysicianNotes ?? "-",
                        cancelledByProviderNote = _context.RequestStatusLogs.FirstOrDefault(re => re.RequestId == r.RequestId && re.Status == 3).Notes ?? "-",
                        adminNote = _context.RequestNotes.FirstOrDefault(rn => rn.RequestId == r.RequestId).AdminNotes ?? "-",
                        patientNote = rc.Notes ?? "-",
                        startDate = r.CreatedDate != null ? r.CreatedDate : DateTime.Today,
                        endDate = r.AcceptedDate != null ? r.AcceptedDate : DateTime.Today,
                        isDeleted = (r.IsDeleted == null) ? new BitArray(1, false) : r.IsDeleted,
                        requestId = r.RequestId,
                        cancellationReason = _context.RequestStatusLogs.FirstOrDefault(rs => rs.RequestId == r.RequestId && rs.Status == 3).Notes
                    };

            if (requestStatus != null && requestStatus != -1)
            {
                q = q.Where(r => r.requestStatus == requestStatus);
            }

            if (patientName != null && patientName != "")
            {
                q = q.Where(r => r.patientName.ToLower().Contains(patientName.ToLower()));
            }

            if (requestType != null && requestType != -1)
            {
                q = q.Where(r => r.requestor == requestType);
            }

            if (fromDate.Value != null && fromDate != temp)
            {
                q = q.Where(r => r.startDate >= fromDate.Value);
            }

            if (toDate.Value != null && toDate != temp)
            {
                q = q.Where(r => r.endDate <= toDate.Value);
            }

            if (providerName != null && providerName != "")
            {
                q = q.Where(r => r.physician.ToLower().Contains(providerName.ToLower()));
            }

            if (email != null && email != "")
            {
                q = q.Where(r => r.email.ToLower().Contains(email.ToLower()));
            }

            if (phoneNo != null && phoneNo != "")
            {
                q = q.Where(r => r.phoneNumber.ToLower().Contains(phoneNo.ToLower()));
            }

            SearchRecordsViewModel sr = new SearchRecordsViewModel();
            sr.adminNavbarModel = an;
            sr.CurrentPage = (int)page;
            sr.PageSize = (int)pageSize;
            sr.TotalItems = q.Count();
            sr.TotalPages = (int)Math.Ceiling((double)q.Count() / (int)pageSize);
            sr.tableData = q.Skip(((int)page - 1) * (int)pageSize).Take((int)pageSize).ToList();
            sr.allDataForExcel = q.ToList();


            return sr;
        }

        SmsLogsViewModel IAdminInterface.SmsLogsFilteredData(AdminNavbarModel an, int page = 1, int pageSize = 10, int? role = 0, string? recipientName = "", string? phoneNumber = "", DateTime? createdDate = null, DateTime? sentDate = null)
        {
            DateTime temp = new DateTime(1, 1, 1, 0, 0, 0);
            var q = from s in _context.Smslogs
                    select new SmsLogsTableData
                    {
                        recipient = _context.Physicians.FirstOrDefault(p => p.PhysicianId == s.PhysicianId).FirstName + " " + _context.Physicians.FirstOrDefault(p => p.PhysicianId == s.PhysicianId).LastName,
                        action = s.Action,
                        roleId = s.RoleId,
                        roleName = s.RoleId != null ? _context.AspNetRoles.FirstOrDefault(a => a.Id == s.RoleId).Name : "-",
                        phoneNumber = s.MobileNumber,
                        createdDate = s.CreateDate,
                        sentDate = (DateTime)s.SentDate,
                        sent = s.IsSmssent[0] == true ? "Yes" : "No",
                        sentTries = s.SentTries,
                        confirmationNo = "-",
                        smsLogId = s.SmslogId,
                    };

            if (role != null && role != 0)
            {
                q = q.Where(r => r.roleId == role);
            }

            if (recipientName != null && recipientName != "")
            {
                q = q.Where(r => r.recipient.ToLower().Contains(recipientName.ToLower()));
            }

            if (phoneNumber != null && phoneNumber != "")
            {
                q = q.Where(r => r.phoneNumber.Contains(phoneNumber));
            }

            if (createdDate != null && createdDate != temp)
            {
                DateOnly date1 = DateOnly.FromDateTime((DateTime)createdDate);
                q = q.Where(r => DateOnly.FromDateTime((DateTime)r.createdDate) == date1);
            }

            if (sentDate != null && sentDate != temp)
            {
                DateOnly date2 = DateOnly.FromDateTime((DateTime)sentDate);
                q = q.Where(r => DateOnly.FromDateTime((DateTime)r.sentDate) == date2);
            }

            SmsLogsViewModel smsLogsViewModel = new SmsLogsViewModel();
            smsLogsViewModel.tableData = q.Skip(((int)page - 1) * (int)pageSize).Take((int)pageSize).ToList();
            smsLogsViewModel.CurrentPage = (int)page;
            smsLogsViewModel.PageSize = (int)pageSize;
            smsLogsViewModel.TotalItems = q.Count();
            smsLogsViewModel.TotalPages = (int)Math.Ceiling((double)q.Count() / (int)pageSize);

            return smsLogsViewModel;

        }

        EmailLogsViewModel IAdminInterface.EmailLogsFilteredData(AdminNavbarModel an, int page = 1, int pageSize = 5, int? role = 0, string? recipientName = "", string? emailId = "", DateTime? createdDate = null, DateTime? sentDate = null)
        {
            DateTime temp = new DateTime(1, 1, 1, 0, 0, 0);

            List<EmailLog> allLogs = _context.EmailLogs.ToList();
            List<EmailLogsTableData> listOfData = new List<EmailLogsTableData>();

            string name = "";
            string conf = "";
            foreach (var q in allLogs)
            {
                if (q.PhysicianId != null)
                {
                    name = _context.Physicians.FirstOrDefault(p => p.PhysicianId == q.PhysicianId).FirstName + " " + _context.Physicians.FirstOrDefault(p => p.PhysicianId == q.PhysicianId).LastName;
                }
                else if (q.AdminId != null)
                {
                    name = _context.Admins.FirstOrDefault(a => a.AdminId == q.AdminId).FirstName + " " + _context.Admins.FirstOrDefault(a => a.AdminId == q.AdminId).LastName;
                }
                else if (q.RequestId == null)
                {
                    name = "";
                }
                else
                {
                    name = _context.Requests.FirstOrDefault(r => r.RequestId == q.RequestId).RequestClient.FirstName + " " + _context.Requests.FirstOrDefault(r => r.RequestId == q.RequestId).RequestClient.LastName;
                    conf = _context.Requests.FirstOrDefault(r => r.RequestId == q.RequestId).ConfirmationNumber;
                }

                EmailLogsTableData eltd = new EmailLogsTableData
                {
                    recipientName = name,
                    action = q.SubjectName,
                    roleId = q.RoleId,
                    emailId = q.EmailId,
                    createdDate = q.CreateDate,
                    sentDate = (DateTime)q.SentDate,
                    isSent = q.IsEmailSent[0] == true ? "Yes" : "No",
                    sentTries = q.SentTries,
                    confirmationNo = conf,
                    emailLogId = q.EmailLogId
                };

                listOfData.Add(eltd);
            }

            if (role != null && role != 0)
            {
                listOfData = listOfData.Where(r => r.roleId == role).ToList();
            }

            if (recipientName != null && recipientName != "")
            {
                listOfData = listOfData.Where(r => r.recipientName.ToLower().Contains(recipientName.ToLower())).ToList();
            }

            if (emailId != null && emailId != "")
            {
                listOfData = listOfData.Where(r => r.emailId.ToLower().Contains(emailId.ToLower())).ToList();
            }

            if (createdDate != null && createdDate != temp)
            {
                DateOnly date1 = DateOnly.FromDateTime((DateTime)createdDate);
                listOfData = listOfData.Where(r => DateOnly.FromDateTime((DateTime)r.createdDate) == date1).ToList();
            }

            if (sentDate != null && sentDate != temp)
            {
                DateOnly date2 = DateOnly.FromDateTime((DateTime)sentDate);
                listOfData = listOfData.Where(r => DateOnly.FromDateTime((DateTime)r.sentDate) == date2).ToList();
            }

            EmailLogsViewModel el = new EmailLogsViewModel
            {
                tableData = listOfData.Skip(((int)page - 1) * (int)pageSize).Take((int)pageSize).ToList(),
                adminNavbarModel = an,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = listOfData.Count(),
                TotalPages = (int)Math.Ceiling((double)listOfData.Count() / pageSize),
            };

            return el;
        }

        public VendorsViewModel VendorsFilteredData(AdminNavbarModel an, string? name = "", int? professionalId = -1, int page = 1, int pageSize = 10)
        {
            List<HealthProfessional> hp = _context.HealthProfessionals.ToList();

            IQueryable<HealthProfessional> query = _context.HealthProfessionals.OrderByDescending(r => r.CreatedDate);

            if (name != "" && name != null)
            {
                query = query.Where(r => r.VendorName.ToLower().Contains(name.ToLower()));
            }

            if (professionalId != null && professionalId != -1)
            {
                query = query.Where(r => r.Profession == professionalId);
            }

            VendorsViewModel v = new VendorsViewModel
            {
                vendorsTableData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = query.Count(),
                TotalPages = (int)Math.Ceiling((double)query.Count() / pageSize),
            };

            return v;
        }

        public void ChangeNotificationValue(int id)
        {
            PhysicianNotification pn = _context.PhysicianNotifications.Where(p => p.PhysicianId == id).FirstOrDefault();
            bool val = pn.IsNotificationStopped[0];
            if (val)
            {
                pn.IsNotificationStopped = new BitArray(1, false);
            }
            else
            {
                pn.IsNotificationStopped = new BitArray(1, true);
            }
            _context.PhysicianNotifications.Update(pn);
            _context.SaveChanges();
        }

        public Request ValidateRequest(int requestId)
        {
            return _context.Requests.Where(r => r.RequestId == requestId).FirstOrDefault();
        }

        public RequestClient ValidateRequestClient(int requestClientId)
        {
            return _context.RequestClients.FirstOrDefault(s => s.RequestClientId == requestClientId);
        }

        public void EditViewCaseAction(ViewCaseModel userProfile, RequestClient userToUpdate)
        {
            //userToUpdate.FirstName = userProfile.FirstName;
            //userToUpdate.LastName = userProfile.LastName;
            userToUpdate.PhoneNumber = userProfile.PhoneNumber;
            userToUpdate.Email = userProfile.Email;
            //userToUpdate.IntDate = userProfile.DOB.Day;
            //userToUpdate.IntYear = userProfile.DOB.Year;
            //userToUpdate.StrMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(userProfile.DOB.Month);
            _context.RequestClients.Update(userToUpdate);
            _context.SaveChanges();
        }

        public RequestNote FetchRequestNote(int requestId)
        {
            return _context.RequestNotes.FirstOrDefault(r => r.RequestId == requestId);
        }

        public RequestStatusLog FetchRequestStatusLogs(int requestId)
        {
            return _context.RequestStatusLogs.FirstOrDefault(r => r.RequestId == requestId);
        }

        public List<RequestStatusLog> GetAllRslData(int requestId)
        {
            return _context.RequestStatusLogs.Where(r => r.RequestId == requestId).ToList();
        }

        public Physician FetchPhysician(int id)
        {
            return _context.Physicians.FirstOrDefault(p => p.PhysicianId == id);
        }

        public Physician GetPhysicianFromAspNetUser(string email)
        {
            AspNetUser anu = _context.AspNetUsers.Where(r => r.Email == email).FirstOrDefault();
            return _context.Physicians.Where(p => p.AspNetUserId == anu.Id).FirstOrDefault();
        }

        public Admin GetAdminFromAspNetUser(string email)
        {
            AspNetUser anu = _context.AspNetUsers.Where(r => r.Email == email).FirstOrDefault();
            return _context.Admins.Where(p => p.AspNetUserId == anu.Id).FirstOrDefault();
        }

        public void EditViewNotesAction(ViewNotes model)
        {
            Request r = _context.Requests.Where(r => r.RequestId == model.RequestId).FirstOrDefault();
            User u = _context.Users.Where(u => u.UserId == r.UserId).FirstOrDefault();
            RequestNote rn = _context.RequestNotes.Where(r => r.RequestId == model.RequestId).FirstOrDefault();
            if (rn == null && model.an.roleName == "Admin")
            {
                RequestNote rn1 = new RequestNote();
                rn1.AdminNotes = model.AdminNotes;
                rn1.RequestId = model.RequestId;
                rn1.CreatedBy = (int)u.AspNetUserId;
                rn1.CreatedDate = DateTime.Now;
                _context.RequestNotes.Add(rn1);
            }

            else if (rn == null && model.an.roleName == "Provider")
            {
                RequestNote rn1 = new RequestNote();
                rn1.PhysicianNotes = model.PhysicianNotes;
                rn1.RequestId = model.RequestId;
                rn1.CreatedBy = (int)u.AspNetUserId;
                rn1.CreatedDate = DateTime.Now;
                _context.RequestNotes.Add(rn1);
            }

            else if (rn != null && model.an.roleName == "Admin")
            {
                rn.AdminNotes = model.AdminNotes;
                _context.RequestNotes.Update(rn);
            }
            else
            {
                rn.PhysicianNotes = model.PhysicianNotes;
                _context.RequestNotes.Update(rn);
            }

            _context.SaveChanges();
        }

        public CaseTag FetchCaseTag(int caseTagId)
        {
            return _context.CaseTags.Where(ct => ct.CaseTagId == caseTagId).FirstOrDefault();
        }

        public void AddRequestStatusLogFromCancelCase(RequestStatusLog rs)
        {
            _context.RequestStatusLogs.Add(rs);
            _context.SaveChanges();
        }

        public Request GetReqFromReqId(int id)
        {
            return _context.Requests.Where(r => r.RequestId == id).FirstOrDefault();
        }

        public bool AcceptCase(int id)
        {
            bool isAccepted = false;
            Request r = GetReqFromReqId(id);
            if (r.Status == 1)
            {
                r.Status = 2;
                _context.Requests.Update(r);
                _context.SaveChanges();
                isAccepted = true;
            }
            return isAccepted;
        }

        public bool ProviderTransferRequest(string notes, int id)
        {
            bool isTransferred = false;
            Request r = GetReqFromReqId(id);
            RequestStatusLog rsl = new RequestStatusLog();
            if (r.Status != 1)
            {
                r.Status = 1;
                r.PhysicianId = null;
                _context.Requests.Update(r);

                rsl.Status = 1;
                rsl.Notes = notes;
                rsl.RequestId = id;
                rsl.TransToAdmin = new BitArray(1, true);
                rsl.CreatedDate = DateTime.Now;
                _context.RequestStatusLogs.Add(rsl);

                _context.SaveChanges();
                isTransferred = true;
            }
            return isTransferred;
        }

        public int SelectCallTypeOfRequest(int id, int callType)
        {
            int x = 0;
            Request r = _context.Requests.Where(r => r.RequestId == id).FirstOrDefault();
            if (callType == 1)
            {
                r.Status = 5;
                r.CallType = 1;
                x = 1;
            }
            else if (callType == 2)
            {
                r.Status = 6;
                r.CallType = 2;
                x = 2;
            }
            _context.Requests.Update(r);
            _context.SaveChanges();
            return x;
        }

        public void AddRequestClosedData(RequestClosed rc)
        {
            _context.RequestCloseds.Add(rc);
            _context.SaveChanges();
        }

        public void AddRequestStatusLogFromAgreement(RequestStatusLog rsl)
        {
            _context.RequestStatusLogs.Add(rsl);
            _context.SaveChanges();
        }

        public List<Physician> FetchPhysicianByRegion(int RegionId)
        {
            BitArray isDeleted = new BitArray(1, false);

            return _context.PhysicianRegions.Where(pr => pr.RegionId == RegionId && pr.Physician.IsDeleted == isDeleted).Select(ph => ph.Physician).ToList();
        }

        public void AddBlockRequestData(int id, string num, string email, string notes)
        {
            BlockRequest br = _context.BlockRequests.Where(b => b.RequestId == id).FirstOrDefault();

            if (br == null)
            {
                BlockRequest br2 = new BlockRequest();
                br2.RequestId = id;
                br2.IsActive = new BitArray(1, true);
                br2.CreatedDate = DateTime.Now;
                br2.PhoneNumber = num;
                br2.Reason = notes;
                _context.BlockRequests.Add(br2);
            }

            else
            {
                br.IsActive = new BitArray(1, true);
                br.CreatedDate = DateTime.Now;
                br.Reason = notes;
                _context.BlockRequests.Update(br);
            }
            _context.SaveChanges();
        }

        public void UpdateRequest(Request r)
        {
            _context.Requests.Update(r);
            _context.SaveChanges();
        }

        public DataLayer.Models.Region ValidateRegion(AdminCreateRequestModel model)
        {
            var temp = model.State.ToLower().Trim();
            return _context.Regions.FirstOrDefault(u => u.Name.ToLower().Trim().Equals(temp));
        }

        public BlockRequest ValidateBlockRequest(AdminCreateRequestModel model)
        {
            return _context.BlockRequests.FirstOrDefault(u => u.Email == model.Email);
        }

        public AspNetUser ValidateAspNetUser(AdminCreateRequestModel model)
        {
            return _context.AspNetUsers.SingleOrDefault(u => u.Email == model.Email);
        }

        public bool VerifyLocation(string state)
        {
            var temp = state.ToLower().Trim();
            return _context.Regions.Any(r => r.Name == temp);
        }

        public void InsertDataOfRequest(AdminCreateRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            AspNetUserRole anur = new AspNetUserRole();
            User user = new User();
            Request request = new Request();
            DataLayer.Models.Region region2 = new DataLayer.Models.Region();
            RequestClient requestClient = new RequestClient();
            RequestStatusLog requestStatusLog = new RequestStatusLog();
            RequestNote requestNote = new RequestNote();
            int atIndex = model.Email.IndexOf("@");
            bool userExists = true;
            if (ValidateAspNetUser(model) == null)
            {
                userExists = false;
                aspNetUser.UserName = model.Email;
                aspNetUser.Email = model.Email;
                aspNetUser.PhoneNumber = model.PhoneNumber;
                aspNetUser.CreatedDate = DateTime.Now;
                aspNetUser.PasswordHash = atIndex >= 0 ? model.Email.Substring(0, atIndex) : model.Email;
                _context.AspNetUsers.Add(aspNetUser);
                _context.SaveChanges();

                anur.UserId = aspNetUser.Id;
                anur.RoleId = 3;
                _context.AspNetUserRoles.Add(anur);
                _context.SaveChanges();

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
                _context.SaveChanges();
            }
            requestClient.FirstName = model.FirstName;
            requestClient.LastName = model.LastName;
            requestClient.PhoneNumber = model.PhoneNumber;
            requestClient.Location = model.City;
            requestClient.Address = model.Street;
            requestClient.RegionId = 1;
            //requestClient.Notes = model.Symptoms;
            requestClient.Email = model.Email;
            requestClient.IntDate = model.DOB.Day;
            requestClient.StrMonth = model.DOB.Month.ToString();
            requestClient.IntYear = model.DOB.Year;
            requestClient.Street = model.Street;
            requestClient.City = model.City;
            requestClient.State = model.State;
            requestClient.ZipCode = model.Zipcode;
            //var temp = await _context.RequestClients.ToListAsync();
            _context.RequestClients.Add(requestClient);
            _context.SaveChanges();

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
            request.IsDeleted = new BitArray(1, false);
            _context.Requests.Add(request);
            _context.SaveChanges();

            requestStatusLog.RequestId = request.RequestId;
            requestStatusLog.Status = 1;
            requestStatusLog.Notes = model.Symptoms;
            requestStatusLog.CreatedDate = DateTime.Now;
            _context.RequestStatusLogs.Add(requestStatusLog);
            _context.SaveChanges();

            requestNote.RequestId = request.RequestId;
            requestNote.AdminNotes = model.AdminNotes;
            requestNote.CreatedDate = DateTime.Now;
            requestNote.CreatedBy = 34;
            _context.RequestNotes.Add(requestNote);
            _context.SaveChanges();
        }

        public AspNetUser ValidateAspNetUser(LoginViewModel model)
        {
            return _context.AspNetUsers.FirstOrDefault(u => u.UserName == model.UserName);
        }

        public Admin ValidateUser(string email)
        {
            Admin user = _context.Admins.FirstOrDefault(x => x.Email == email);
            return user;
        }

        public Physician ValidatePhysician(string email)
        {
            Physician p = _context.Physicians.FirstOrDefault(x => x.Email == email);
            return p;
        }

        public User ValidateUserByRequestId(Request r)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == r.UserId);
        }

        public List<RequestWiseFile> GetFileData(int requestid)
        {
            return _context.RequestWiseFiles.Where(r => r.RequestId == requestid).ToList();
        }

        public Request GetRequestWithUser(int requestid)
        {
            return _context.Requests.Include(r => r.User).FirstOrDefault(u => u.RequestId == requestid);
        }

        public void AddFile(RequestWiseFile requestWiseFile)
        {
            _context.RequestWiseFiles.Add(requestWiseFile);
            _context.SaveChanges();
        }

        public AspNetUser ValidAspNetUser(string email)
        {
            return _context.AspNetUsers.Where(a => a.Email == email).FirstOrDefault();
        }

        public bool FindAdminFromAspNetUser(int id)
        {
            return _context.Admins.Any(r => r.AspNetUserId == id);
        }

        public bool FindPhysicianFromAspNetUser(int id)
        {
            return _context.Physicians.Any(r => r.AspNetUserId == id);
        }

        public RequestClient ValidatePatientEmail(string email)
        {
            return _context.RequestClients.Where(a => a.Email == email).FirstOrDefault();
        }

        public List<HealthProfessional> getBusinessData(int professionId)
        {
            return _context.HealthProfessionals.Where(hp => hp.Profession == professionId).ToList();
        }

        public PasswordReset ValidateToken(string token)
        {
            return _context.PasswordResets.FirstOrDefault(pr => pr.Token == token);
        }

        public AspNetUser ValidateUserForResetPassword(ResetPasswordViewModel model, string useremail)
        {
            return _context.AspNetUsers.FirstOrDefault(x => x.Email == useremail);
        }

        public void SetPasswordForResetPassword(AspNetUser user, ResetPasswordViewModel model)
        {
            user.PasswordHash = model.Password;
            _context.SaveChanges();
        }

        public List<Request> GetRequestDataInList()
        {
            return _context.Requests.Include(r => r.RequestClient).ToList();
        }
        public int SingleDelete(int id)
        {
            RequestWiseFile rwf = _context.RequestWiseFiles.Where(r => r.RequestWiseFileId == id).FirstOrDefault();
            rwf.IsDeleted = new BitArray(1, true);
            _context.SaveChanges();
            return rwf.RequestId;
        }

        public List<DataLayer.Models.Region> GetAllRegion()
        {
            return _context.Regions.ToList();
        }

        public List<CaseTag> GetAllCaseTags()
        {
            return _context.CaseTags.ToList();
        }

        public Request GetReqFromReqType(int ReqId)
        {
            return _context.Requests.Where(r => r.RequestTypeId == ReqId).FirstOrDefault();
        }

        public Request GetReqFromModel(AdminDashboardTableView model)
        {
            return _context.Requests.Where(re => re.RequestId == model.RequestId).FirstOrDefault();
        }

        public void MultipleDelete(int requestid, string fileId)
        {
            RequestWiseFile rwf = _context.RequestWiseFiles.Where(r => r.RequestId == requestid).FirstOrDefault();
            string[] fileid = fileId.Split(',').Select(x => x.Trim()).ToArray();
            for (int i = 0; i < fileid.Length; i++)
            {
                RequestWiseFile r = _context.RequestWiseFiles.Where(r => r.RequestWiseFileId == int.Parse(fileid[i])).FirstOrDefault();
                r.IsDeleted = new BitArray(1, true);
            }
            _context.SaveChanges();
        }

        public List<HealthProfessionalType> GetHealthProfessionalType()
        {
            return _context.HealthProfessionalTypes.ToList();
        }

        public List<HealthProfessional> GetHealthProfessional()
        {
            return _context.HealthProfessionals.ToList();
        }

        public List<HealthProfessional> GetBusinessDataFromProfession(int professionId)
        {
            BitArray b = new BitArray(1, false);
            return _context.HealthProfessionals.Where(h => h.Profession == professionId && h.IsDeleted == b).ToList();
        }

        public HealthProfessional GetOtherDataFromBId(int businessId)
        {
            return _context.HealthProfessionals.Where(h => h.VendorId == businessId).FirstOrDefault();
        }

        public void AddOrderDetails(OrderDetail orderDetail)
        {
            _context.OrderDetails.Add(orderDetail);
            _context.SaveChanges();
        }

        public RequestClient GetPatientData(int id)
        {
            Request r = _context.Requests.Where(re => re.RequestId == id).FirstOrDefault();
            RequestClient rc = _context.RequestClients.Where(x => x.RequestClientId == r.RequestClientId).FirstOrDefault();
            return rc;
        }

        public string GetMailToSentAgreement(int reqId)
        {
            Request r = _context.Requests.Where(re => re.RequestId == reqId).FirstOrDefault();
            RequestClient rc = _context.RequestClients.Where(rct => rct.RequestClientId == r.RequestClientId).FirstOrDefault();
            return rc.Email;
        }

        public RequestClient GetRequestClientFromId(int id)
        {
            return _context.RequestClients.Where(r => r.RequestClientId == id).FirstOrDefault();
        }

        public Request GetReqFromReqClient(int id)
        {
            return _context.Requests.Where(r => r.RequestClientId == id).FirstOrDefault();
        }

        public RequestStatusLog GetLogFromReqId(int reqId)
        {
            return _context.RequestStatusLogs.Where(r => r.RequestId == reqId).FirstOrDefault();
        }

        public EncounterForm GetEncounterFormData(int reqId)
        {
            return _context.EncounterForms.Where(e => e.RequestId == reqId).FirstOrDefault();
        }

        public void UpdateEncounterFormData(EncounterFormModel model)
        {
            EncounterForm ef = _context.EncounterForms.Where(r => r.RequestId == model.reqId).FirstOrDefault();

            if (ef != null)
            {
                ef.HistoryIllness = model.HistoryOfIllness;
                ef.MedicalHistory = model.MedicalHistory;
                ef.Medications = model.Medications;
                ef.Allergies = model.Allergies;
                ef.Temp = model.Temp;
                ef.Hr = model.HR;
                ef.Rr = model.RR;
                ef.BpS = model.BPS;
                ef.BpD = model.BPD;
                ef.O2 = model.O2;
                ef.Pain = model.Pain;
                ef.Heent = model.Heent;
                ef.Cv = model.CV;
                ef.Chest = model.Chest;
                ef.Abd = model.ABD;
                ef.Extr = model.Extr;
                ef.Skin = model.Skin;
                ef.Neuro = model.Neuro;
                ef.Other = model.Other;
                ef.Diagnosis = model.Diagnosis;
                ef.TreatmentPlan = model.TreatmentPlan;
                ef.MedicationDispensed = model.MedicationsDispensed;
                ef.Procedures = model.Procedures;
                ef.FollowUp = model.FollowUp;
            }

            else
            {
                EncounterForm ef2 = new EncounterForm();
                ef2.RequestId = (int)model.reqId;
                ef2.HistoryIllness = model.HistoryOfIllness;
                ef2.MedicalHistory = model.MedicalHistory;
                ef2.Medications = model.Medications;
                ef2.Allergies = model.Allergies;
                ef2.Temp = model.Temp;
                ef2.Hr = model.HR;
                ef2.Rr = model.RR;
                ef2.BpS = model.BPS;
                ef2.BpD = model.BPD;
                ef2.O2 = model.O2;
                ef2.Pain = model.Pain;
                ef2.Heent = model.Heent;
                ef2.Cv = model.CV;
                ef2.Chest = model.Chest;
                ef2.Abd = model.ABD;
                ef2.Extr = model.Extr;
                ef2.Skin = model.Skin;
                ef2.Neuro = model.Neuro;
                ef2.Other = model.Other;
                ef2.Diagnosis = model.Diagnosis;
                ef2.TreatmentPlan = model.TreatmentPlan;
                ef2.MedicationDispensed = model.MedicationsDispensed;
                ef2.Procedures = model.Procedures;
                ef2.FollowUp = model.FollowUp;
            }

            _context.EncounterForms.Update(ef);
            _context.SaveChanges();
        }

        public bool FinalizeEncounterForm(int id)
        {
            bool isFinalized = false;
            EncounterForm ef = _context.EncounterForms.Where(r => r.RequestId == id).FirstOrDefault();
            if (ef != null)
            {
                ef.IsFinalized = new BitArray(1, true);
                _context.EncounterForms.Update(ef);
                _context.SaveChanges();
                isFinalized = true;
            }
            return isFinalized;
        }

        public void UpdateRequestClient(RequestClient rc)
        {
            _context.RequestClients.Update(rc);
            _context.SaveChanges();
        }

        public Admin GetAdminFromId(int id)
        {
            return _context.Admins.Where(a => a.AspNetUserId == id).FirstOrDefault();
        }

        public Physician GetPhysicianFromId(int id)
        {
            return _context.Physicians.Where(p => p.AspNetUserId == id).FirstOrDefault();
        }

        public List<HalloDoc.DataLayer.Models.Region> GetAllRegions()
        {
            return _context.Regions.ToList();
        }

        public AspNetUser GetAdminDataFromId(int id)
        {
            return _context.AspNetUsers.Where(a => a.Id == id).FirstOrDefault();
        }

        public HalloDoc.DataLayer.Models.Region GetRegFromId(int id)
        {
            return _context.Regions.Where(r => r.RegionId == id).FirstOrDefault();
        }

        public AspNetUser GetAspNetFromAdminId(int id)
        {
            Admin a = _context.Admins.Where(ad => ad.AdminId == id).FirstOrDefault();
            int anid = a.AspNetUserId;
            return _context.AspNetUsers.Where(x => x.Id == anid).FirstOrDefault();
        }

        public void AdminResetPassword(AspNetUser anur, string pass)
        {
            anur.PasswordHash = pass;
            _context.AspNetUsers.Update(anur);
            _context.SaveChanges();
        }

        public void UpdateAdminDataFromId(AdminProfile model, int id, string selectedRegion)
        {
            List<int> selectedRegionIds = null;
            int x = 1;
            AdminRegion arr = _context.AdminRegions.OrderByDescending(r => r.AdminRegionId).FirstOrDefault();
            if (!string.IsNullOrEmpty(selectedRegion))
            {
                selectedRegionIds = selectedRegion.Split(',').Select(int.Parse).ToList();
            }
            // for newly selected region
            foreach (var item in selectedRegionIds)
            {
                //check if selected region exists in AdminRegion
                bool isPresent = _context.AdminRegions.Any(r => r.RegionId == item && r.AdminId == model.adminId);

                //if exists, no need to do any change
                if (isPresent)
                {
                    continue;
                }
                // if does not exist, add record for that adminId and regionId
                else
                {
                    AdminRegion ar = new AdminRegion();
                    ar.AdminRegionId = arr.AdminRegionId + x;
                    ar.AdminId = id;
                    ar.RegionId = item;
                    _context.AdminRegions.Add(ar);
                    _context.SaveChanges();
                    x++;
                }
            }

            // when an already selected region needs to be removed

            // fetch all regionId from AdminRegion
            List<int> idInDb = _context.AdminRegions.Select(r => r.RegionId).ToList();

            foreach (var item in idInDb)
            {
                // if regionId from AdminRegion table does not exist in rId, remove it from AdminRegion table 
                if (!selectedRegionIds.Contains(item))
                {
                    AdminRegion ar = _context.AdminRegions.Where(a => a.RegionId == item).FirstOrDefault();
                    _context.AdminRegions.Remove(ar);
                }
            }

            Admin ad = _context.Admins.Where(ad => ad.AdminId == id).FirstOrDefault();
            ad.FirstName = model.firstName;
            ad.LastName = model.lastName;
            ad.Email = model.email;
            ad.Mobile = model.phone;
            _context.Admins.Update(ad);
            _context.SaveChanges();
        }

        public List<AdminRegion> GetAdminRegionFromId(int id)
        {
            return _context.AdminRegions.Where(a => a.AdminId == id).ToList();
        }

        public List<AdminRegion> GetAvailableRegionOfAdmin(int id)
        {
            return _context.AdminRegions.Include(ad => ad.Region).Where(a => a.AdminId == id).ToList();
        }

        public void UpdateMailingInfo(AdminProfile model, int aid)
        {
            Admin ad = _context.Admins.Where(ad => ad.AdminId == aid).FirstOrDefault();
            ad.Address1 = model.address1;
            ad.Address2 = model.address2;
            ad.City = model.city;
            ad.Zip = model.zipcode;
            ad.AltPhone = model.altPhoneNo;
            _context.Admins.Update(ad);
            _context.SaveChanges();
        }

        public List<Request> GetPatientRecordsData(int userId)
        {
            return _context.Requests.Where(r => r.UserId == userId).ToList();
        }

        public List<Physician> GetAllPhysicians()
        {
            return _context.Physicians.ToList();
        }

        public List<RequestWiseFile> GetAllFiles()
        {
            return _context.RequestWiseFiles.ToList();
        }

        public List<Menu> GetAllMenus()
        {
            return _context.Menus.ToList();
        }

        public void CreateNewRole2(string name, string acType, string adminName, List<int> menuIds)
        {
            Role r = new Role();
            r.Name = name;
            if (acType == "Admin")
            {
                r.AccountType = 1;
            }
            else if (acType == "Physician")
            {
                r.AccountType = 2;
            }
            r.CreatedDate = DateTime.Now;
            r.IsDeleted = new BitArray(1, false);
            r.CreatedBy = adminName;
            _context.Roles.Add(r);
            _context.SaveChanges();

            foreach (var item in menuIds)
            {
                RoleMenu rm = new RoleMenu();
                rm.MenuId = item;
                rm.RoleId = r.RoleId;
                _context.RoleMenus.Add(rm);
            }

            _context.SaveChanges();
        }

        public List<Role> GetAllRoles()
        {
            return _context.Roles.ToList();
        }

        public List<Role> GetSpecifiedAdminRoles()
        {
            return _context.Roles.Where(r => r.AccountType == 1 && r.IsDeleted == new BitArray(1, false)).ToList();
        }

        public List<Role> GetSpecifiedProviderRoles()
        {
            return _context.Roles.Where(r => r.AccountType == 2 && r.IsDeleted == new BitArray(1, false)).ToList();
        }

        public void DeleteRoleFromId(int roleId)
        {
            Role r = _context.Roles.Where(r => r.RoleId == roleId).FirstOrDefault();
            r.IsDeleted = new BitArray(1, true);
            _context.Roles.Update(r);
            _context.SaveChanges();
        }

        public string GetNameFromRoleId(int id)
        {
            Role r = _context.Roles.Where(r => r.RoleId == id).FirstOrDefault();
            return r.Name;
        }

        public int GetAccountTypeFromId(int id)
        {
            Role r = _context.Roles.Where(r => r.RoleId == id).FirstOrDefault();
            return r.AccountType;
        }

        public List<RoleMenu> GetAllRoleMenu(int id)
        {
            return _context.RoleMenus.Where(r => r.RoleId == id).ToList();
        }

        public void EditRoleSubmitAction(int roleid, List<int> menuIds)
        {
            foreach (var item in menuIds)
            {
                bool r = _context.RoleMenus.Where(r => r.RoleId == roleid).Any(rom => rom.MenuId == item);
                if (r == false)
                {
                    RoleMenu rm = new RoleMenu();
                    rm.MenuId = item;
                    rm.RoleId = roleid;
                    _context.RoleMenus.Add(rm);
                }
            }
            List<RoleMenu> rm2 = _context.RoleMenus.Where(r => r.RoleId == roleid).ToList();
            foreach (RoleMenu rmItem in rm2)
            {
                int menuId = rmItem.MenuId;
                if (!menuIds.Contains(menuId))
                {
                    RoleMenu r = _context.RoleMenus.Where(rm => rm.RoleId == roleid && rm.MenuId == menuId).FirstOrDefault();
                    _context.RoleMenus.Remove(r);
                }
            }

            _context.SaveChanges();
        }

        public EditProviderAccountViewModel ProviderEditAccount(int id, AdminNavbarModel an)
        {
            var physician = _context.Physicians.FirstOrDefault(r => r.PhysicianId == id);
            List<PhysicianRegion> PRegions = _context.PhysicianRegions.Where(r => r.PhysicianId == physician.PhysicianId).ToList();
            List<DataLayer.Models.Region> reg = _context.Regions.ToList();
            var selectedRegions = from r in reg
                                  join pr in PRegions
                                  on r.RegionId equals pr.RegionId
                                  select r;
            var data = selectedRegions.ToList();
            AspNetUser user = _context.AspNetUsers.FirstOrDefault(r => r.Id == physician.AspNetUserId);
            EditProviderAccountViewModel viewmodel = new EditProviderAccountViewModel
            {
                UserName = user.UserName,
                FirstName = physician.FirstName,
                LastName = physician.LastName,
                Password = user.PasswordHash,
                Email = physician.Email,
                ConfirmEmail = "",
                Phone = physician.Mobile,
                regions = _context.Regions.ToList(),
                selectedregions = data,
                Address1 = physician.Address1,
                Address2 = physician.Address2,
                City = physician.City,
                State = physician.City,
                Zip = physician.Zip,
                MedicalLicense = physician.MedicalLicense,
                NPI = physician.Npinumber,
                SyncEmail = physician.SyncEmailAddress,
                MailingPhoneNo = physician.AltPhone,
                BusinessName = physician.BusinessName,
                BusinessWebsite = physician.BusinessWebsite,
                SignatureName = physician.Signature,
                PhysicianId = id,
                Contract = physician.IsAgreementDoc != null ? physician.IsAgreementDoc[0] : null,
                BackgroundCheck = physician.IsBackgroundDoc != null ? physician.IsBackgroundDoc[0] : null,
                Compilance = physician.IsTrainingDoc != null ? physician.IsTrainingDoc[0] : null,
                NonDisclosure = physician.IsNonDisclosureDoc != null ? physician.IsNonDisclosureDoc[0] : null,
                LicensedDoc = physician.IsLicenseDoc != null ? physician.IsLicenseDoc[0] : null,
                adminNavbarModel = an,
                Photo = null,
                roles = _context.Roles.Where(r => r.AccountType == (short)2).ToList(),
                regionId = physician.RegionId
            };
            return viewmodel;
        }

        public void SavePasswordOfPhysician(EditProviderAccountViewModel model)
        {
            Physician physician = _context.Physicians.FirstOrDefault(r => r.PhysicianId == model.PhysicianId);
            AspNetUser user = _context.AspNetUsers.FirstOrDefault(r => r.Id == physician.AspNetUserId);

            user.PasswordHash = model.Password;
            physician.ModifiedDate = DateTime.Now;
            _context.Physicians.Update(physician);
            _context.AspNetUsers.Update(user);
            _context.SaveChanges();
        }

        public void EditProviderBillingInfo(EditProviderAccountViewModel model)
        {
            Physician physician = _context.Physicians.FirstOrDefault(r => r.PhysicianId == model.PhysicianId);
            PhysicianLocation pl = _context.PhysicianLocations.FirstOrDefault(plo => plo.PhysicianId == model.PhysicianId);
            if (!physician.IsDeleted[0])
            {
                physician.Address1 = model.Address1;
                physician.Address2 = model.Address2;
                physician.City = model.City;
                physician.Zip = model.Zip;
                physician.RegionId = model.regionId;
                physician.AltPhone = model.MailingPhoneNo;
                physician.ModifiedDate = DateTime.Now;

                pl.Latitude = model.lati;
                pl.Longitude = model.longi;
                pl.Address = model.Address1 + ", " + model.Address2 + ", " + model.City;
            }
            _context.Physicians.Update(physician);
            _context.PhysicianLocations.Update(pl);
            _context.SaveChanges();
        }

        public void SaveProviderProfile(EditProviderAccountViewModel model, string selectedRegionsList)
        {
            var PRegions = _context.PhysicianRegions.Where(r => r.PhysicianId == model.PhysicianId).ToList();
            List<int> selectedRegionIds = null;

            if (!string.IsNullOrEmpty(selectedRegionsList))
            {
                selectedRegionIds = selectedRegionsList.Split(',').Select(int.Parse).ToList();
            }

            foreach (var region in PRegions)
            {
                _context.PhysicianRegions.Remove(region);
            }

            if (selectedRegionIds != null && selectedRegionIds.Count > 0)
            {
                for (int ele = 0; ele < selectedRegionIds.Count; ele++)
                {
                    PhysicianRegion ar = new PhysicianRegion
                    {
                        PhysicianId = model.PhysicianId,
                        RegionId = selectedRegionIds[ele]
                    };
                    _context.PhysicianRegions.Add(ar);
                }
            }

            var currentPhysician = _context.Physicians.FirstOrDefault(r => r.PhysicianId == model.PhysicianId);
            var user = _context.Physicians.FirstOrDefault(r => r.AspNetUserId == currentPhysician.AspNetUserId);

            if (!user.IsDeleted[0])
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.Mobile = model.Phone;
                user.SyncEmailAddress = model.SyncEmail;
                user.MedicalLicense = model.MedicalLicense;
                user.Npinumber = model.NPI;
                user.ModifiedDate = DateTime.Now;
            }

            _context.SaveChanges();
        }

        public List<PhysicianLocation> GetPhysicianLocation()
        {
            return _context.PhysicianLocations.ToList();
        }

        public void SetContentOfPhysician(IFormFile file, int id, bool IsSignature)
        {
            var physician = _context.Physicians.FirstOrDefault(r => r.PhysicianId == id);
            var FileName = "Signature.png";
            if (file != null && file.Length > 0)
            {
                var physicianFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Physician", id.ToString());

                if (!Directory.Exists(physicianFolderPath))
                {
                    Directory.CreateDirectory(physicianFolderPath);
                }
                if (!IsSignature)
                {
                    FileName = "Profile.png";
                }
                var filePath = Path.Combine(physicianFolderPath, FileName);
                if (physician.Signature != null && IsSignature)
                {
                    var SavedFile = Path.Combine(physicianFolderPath, physician.Signature);
                    System.IO.File.Delete(SavedFile);
                }
                if (physician.Photo != null && !IsSignature)
                {
                    var SavedFile = Path.Combine(physicianFolderPath, physician.Photo);
                    System.IO.File.Delete(SavedFile);
                }
                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyToAsync(stream)
        ;
                }
            }
            if (file != null)
            {
                if (IsSignature)
                {
                    physician.Signature = FileName;
                    physician.ModifiedDate = DateTime.Now;
                }
                else
                {
                    physician.Photo = FileName;
                    physician.ModifiedDate = DateTime.Now;
                }
            }
            _context.SaveChanges();
        }

        public void SetAllDocOfPhysician(IFormFile file, int id, int num)
        {
            var physician = _context.Physicians.FirstOrDefault(r => r.PhysicianId == id);
            var FileName = "Signature.png";

            if (file != null && file.Length > 0)
            {
                var physicianFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Physician", id.ToString());
                var ext = Path.GetExtension(file.FileName); ;
                if (!Directory.Exists(physicianFolderPath))
                {
                    Directory.CreateDirectory(physicianFolderPath);
                }
                if (num == 1) FileName = "AgreementDoc" + ext;
                else if (num == 2) FileName = "BackgroundDoc" + ext;
                else if (num == 3) FileName = "Compilance" + ext;
                else if (num == 4) FileName = "NonDisclosure" + ext;
                else FileName = "LicenseDoc" + ext;

                var filePath = Path.Combine(physicianFolderPath, FileName);
                if (num == 1)
                {
                    if (physician.IsAgreementDoc != null)
                    {
                        var SavedFile = Path.Combine(physicianFolderPath, "AgreementDoc.pdf");
                        System.IO.File.Delete(SavedFile);
                    }
                    physician.IsAgreementDoc = new BitArray(1, true);
                    physician.ModifiedDate = DateTime.Now;
                }
                else if (num == 2)
                {
                    if (physician.IsBackgroundDoc != null)
                    {
                        var SavedFile = Path.Combine(physicianFolderPath, "BackgroundDoc.pdf");
                        System.IO.File.Delete(SavedFile);
                    }
                    physician.IsBackgroundDoc = new BitArray(1, true);
                    physician.ModifiedDate = DateTime.Now;
                }
                else if (num == 3)
                {
                    if (physician.IsTrainingDoc != null)
                    {
                        var SavedFile = Path.Combine(physicianFolderPath, "Compilance.pdf");
                        System.IO.File.Delete(SavedFile);
                    }
                    physician.IsTrainingDoc = new BitArray(1, true);
                    physician.ModifiedDate = DateTime.Now;
                }
                else if (num == 4)
                {
                    if (physician.IsNonDisclosureDoc != null)
                    {
                        var SavedFile = Path.Combine(physicianFolderPath, "NonDisclosure.pdf");
                        System.IO.File.Delete(SavedFile);
                    }
                    physician.IsNonDisclosureDoc = new BitArray(1, true);
                    physician.ModifiedDate = DateTime.Now;
                }
                else
                {
                    if (physician.IsLicenseDoc != null)
                    {
                        var SavedFile = Path.Combine(physicianFolderPath, "LicenseDoc.pdf");
                        System.IO.File.Delete(SavedFile);
                    }
                    physician.IsLicenseDoc = new BitArray(1, true);
                    physician.ModifiedDate = DateTime.Now;
                }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }

            _context.SaveChanges();
        }

        public void PhysicianProfileUpdate(EditProviderAccountViewModel model)
        {
            var physician = _context.Physicians.FirstOrDefault(r => r.PhysicianId == model.PhysicianId);

            physician.BusinessName = model.BusinessName;
            physician.BusinessWebsite = model.BusinessWebsite;
            physician.AdminNotes = model.AdminNotes;
            _context.SaveChanges();
        }

        public void DeletePhysicianAccount(int id)
        {
            Physician p = _context.Physicians.Where(p => p.PhysicianId == id).FirstOrDefault();
            p.IsDeleted = new BitArray(1, true);
            _context.Physicians.Update(p);
            _context.SaveChanges();
        }

        public void CreateNewProviderAccount(EditProviderAccountViewModel model, List<int> regionNames, int userId)
        {
            Admin ad = GetAdminFromId(userId);

            AspNetUser anu = new AspNetUser();
            AspNetUserRole anur = new AspNetUserRole();
            Physician p = new Physician();
            PhysicianLocation pl = new PhysicianLocation();
            PhysicianNotification pn = new PhysicianNotification();
            Physician ph = _context.Physicians.OrderByDescending(r => r.CreatedDate).FirstOrDefault();

            anu.UserName = "MD." + model.LastName + "." + model.FirstName[0];
            anu.PasswordHash = model.Password;
            anu.Email = model.Email;
            anu.PhoneNumber = model.Phone;
            anu.CreatedDate = DateTime.Now;
            _context.AspNetUsers.Add(anu);
            _context.SaveChanges();

            anur.UserId = anu.Id;
            anur.RoleId = 2;
            _context.AspNetUserRoles.Add(anur);

            p.PhysicianId = ph.PhysicianId + 1;
            p.AspNetUserId = anu.Id;
            p.FirstName = model.FirstName;
            p.LastName = model.LastName;
            p.Email = model.Email;
            p.Mobile = model.Phone;
            p.Photo = model.Photo.FileName;
            p.AdminNotes = model.AdminNotes;
            p.IsAgreementDoc = model.ContractAgreementFile == null ? new BitArray(1, false) : new BitArray(1, true);
            p.IsBackgroundDoc = model.BackgroundCheckFile == null ? new BitArray(1, false) : new BitArray(1, true);
            p.IsTrainingDoc = model.HippaFile == null ? new BitArray(1, false) : new BitArray(1, true);
            p.IsNonDisclosureDoc = model.NonDisclosureAgreement == null ? new BitArray(1, false) : new BitArray(1, true);
            p.Address1 = model.Address1;
            p.Address2 = model.Address2;
            p.City = model.City;
            p.RegionId = model.regionId;
            p.Zip = model.Zip;
            p.AltPhone = model.MailingPhoneNo;
            p.CreatedBy = ad.AspNetUserId;
            p.CreatedDate = DateTime.Now;
            p.Status = 1;
            p.BusinessName = model.BusinessName;
            p.BusinessWebsite = model.BusinessWebsite;
            p.IsDeleted = new BitArray(1, false);
            p.RoleId = model.roleId;
            p.Status = 1;
            _context.Physicians.Add(p);
            //_context.SaveChanges();

            pn.Physician = p;
            pn.IsNotificationStopped = new BitArray(1, false);
            _context.PhysicianNotifications.Add(pn);
            _context.SaveChanges();

            foreach (var item in regionNames)
            {
                PhysicianRegion pr = new PhysicianRegion();
                pr.Physician = p;
                pr.RegionId = item;
                _context.PhysicianRegions.Add(pr);
                _context.SaveChanges();

            }

            pl.PhysicianId = p.PhysicianId;
            pl.Latitude = model.lati;
            pl.Longitude = model.longi;
            pl.CreatedDate = DateTime.Now;
            pl.PhysicianName = model.FirstName + " " + model.LastName;
            pl.Address = model.Address1 + ", " + model.Address2 + ", " + model.City;

            if (model.ContractAgreementFile != null)
            {
                SetAllDocOfPhysician(model.ContractAgreementFile, p.PhysicianId, 1);
            }
            if (model.BackgroundCheckFile != null)
            {
                SetAllDocOfPhysician(model.BackgroundCheckFile, p.PhysicianId, 2);
            }
            if (model.HippaFile != null)
            {
                SetAllDocOfPhysician(model.HippaFile, p.PhysicianId, 3);
            }
            if (model.NonDisclosureAgreement != null)
            {
                SetAllDocOfPhysician(model.NonDisclosureAgreement, p.PhysicianId, 4);
            }

            _context.SaveChanges();
        }

        public void CreateNewAdminAccount(EditProviderAccountViewModel model, List<int> regionNames, int userId)
        {
            Admin ad = GetAdminFromId(userId);
            AdminRegion arr = _context.AdminRegions.OrderByDescending(r => r.AdminRegionId).FirstOrDefault();
            AspNetUser anu = new AspNetUser();
            AspNetUserRole anur = new AspNetUserRole();
            Admin a = new Admin();
            int id = arr.AdminRegionId + 1;

            anu.UserName = model.LastName + model.FirstName[0];
            anu.PasswordHash = model.Password;
            anu.Email = model.Email;
            anu.PhoneNumber = model.Phone;
            anu.CreatedDate = DateTime.Now;
            _context.AspNetUsers.Add(anu);
            _context.SaveChanges();

            anur.UserId = anu.Id;
            anur.RoleId = 1;
            _context.AspNetUserRoles.Add(anur);

            a.AspNetUserId = anu.Id;
            a.FirstName = model.FirstName;
            a.LastName = model.LastName;
            a.Email = model.Email;
            a.Mobile = model.Phone;
            a.Address1 = model.Address1;
            a.Address2 = model.Address2;
            a.City = model.City;
            a.RegionId = model.regionId;
            a.Zip = model.Zip;
            a.AltPhone = model.MailingPhoneNo;
            a.CreatedBy = ad.AspNetUserId;
            a.CreatedDate = DateTime.Now;
            a.Status = 1;
            a.IsDeleted = false;
            a.RoleId = model.roleId;
            _context.Admins.Add(a);

            foreach (var item in regionNames)
            {
                AdminRegion ar = new AdminRegion();
                ar.AdminRegionId = id;
                ar.Admin = a;
                ar.RegionId = item;
                id++;
                _context.AdminRegions.Add(ar);
                _context.SaveChanges();
            }

            _context.SaveChanges();
        }



        public List<string> GetAllMenus(string roleId)
        {
            List<RoleMenu> rm = _context.RoleMenus.Where(m => m.RoleId == int.Parse(roleId)).ToList();
            var menus = from r in rm join m in _context.Menus on r.MenuId equals m.MenuId select m.Name;
            return menus.ToList();
        }

        public List<BlockedHistoryData> GetBlockedHistoryData()
        {
            List<BlockedHistoryData> allData = new List<BlockedHistoryData>();
            List<BlockRequest> br = _context.BlockRequests.ToList();

            foreach (var item in br)
            {
                Request r = ValidateRequest(item.RequestId);
                RequestClient rc = GetPatientData(r.RequestId);
                BlockedHistoryData bh = new BlockedHistoryData();
                bh.PhoneNumber = item.PhoneNumber;
                bh.Email = item.Email;
                bh.CreatedDate = DateOnly.FromDateTime((DateTime)item.CreatedDate);
                bh.Notes = item.Reason;
                bh.IsActive = item.IsActive[0];
                bh.PatientName = string.Concat(rc.FirstName, ", ", rc.LastName);
                bh.RequestId = item.RequestId;
                allData.Add(bh);
            }

            return allData;
        }

        public List<RequestedShiftsData> GetRequestedShiftsData(int? regionId)
        {
            List<RequestedShiftsData> rsd = new List<RequestedShiftsData>();
            List<ShiftDetail> sd = _context.ShiftDetails.Where(s => s.Status == 0).ToList();

            if (regionId != -1 && regionId != null)
            {
                sd = sd.Where(r => r.RegionId == regionId).ToList();
            }

            foreach (var item in sd)
            {
                DataLayer.Models.Region r = _context.Regions.Where(re => re.RegionId == item.RegionId).FirstOrDefault();
                Shift s = _context.Shifts.Where(s => s.ShiftId == item.ShiftId).FirstOrDefault();
                Physician p = _context.Physicians.Where(ph => ph.PhysicianId == s.PhysicianId).FirstOrDefault();
                RequestedShiftsData oneShiftDetail = new RequestedShiftsData();
                oneShiftDetail.physicianName = string.Concat(p.FirstName, ", ", p.LastName);
                oneShiftDetail.day = item.ShiftDate.ToString("MMM dd, yyyy");
                oneShiftDetail.time = item.StartTime.ToString("hh:mm tt") + '-' + item.EndTime.ToString("hh:mm tt");
                oneShiftDetail.regionName = r.Name;
                oneShiftDetail.shiftDetailId = item.ShiftDetailId;
                oneShiftDetail.status = item.Status;
                oneShiftDetail.isDeleted = item.IsDeleted[0];
                rsd.Add(oneShiftDetail);
            }
            return rsd;
        }

        public void UnblockRequest(int id)
        {
            BlockRequest br = _context.BlockRequests.Where(b => b.RequestId == id).FirstOrDefault();
            br.IsActive = new BitArray(1, false);
            _context.BlockRequests.Update(br);

            Request r = _context.Requests.Where(r => r.RequestId == id).FirstOrDefault();
            r.Status = 1;
            _context.Requests.Update(r);
            _context.SaveChanges();
        }

        public List<ShiftDetail> GetScheduleData(int RegionId)
        {
            try
            {
                return _context.ShiftDetails.Include(m => m.Shift).Where(m => (RegionId == 0 || m.RegionId == RegionId) && m.IsDeleted == new System.Collections.BitArray(new[] { false })).ToList();

            }
            catch { return new List<ShiftDetail> { }; }
        }

        public List<SchedulingViewModel> GetProviderInformation(int Region)
        {
            try
            {
                var physician = _context.PhysicianRegions.Include(m => m.Physician).Where(m => Region == 0 || m.RegionId == Region);


                List<SchedulingViewModel> model = new List<SchedulingViewModel>();
                foreach (var item in physician)
                {
                    if (item.Physician.IsDeleted == null || item.Physician.IsDeleted[0] == false)
                    {
                        SchedulingViewModel providerInformationViewModel = new SchedulingViewModel()
                        {
                            physicianId = item.Physician.PhysicianId,
                            ProviderName = item.Physician.FirstName + " " + item.Physician.LastName,
                            ProviderEmail = item.Physician.Email,
                            Role = item.Physician.RoleId.ToString(),
                            Status = item.Physician.Status.ToString()
                        };
                        model.Add(providerInformationViewModel);
                    }
                }
                return model.ToList();
            }
            catch
            {
                return new List<SchedulingViewModel>();
            }
        }

        public string GetPhysicianNameFromId(int id, int shiftId)
        {
            Physician p = _context.Physicians.Where(s => s.PhysicianId == id).FirstOrDefault();
            ShiftDetail shiftDetail = _context.ShiftDetails.Where(s => s.ShiftId == shiftId).FirstOrDefault();
            DataLayer.Models.Region r = _context.Regions.Where(re => re.RegionId == shiftDetail.RegionId).FirstOrDefault();
            return p.FirstName + ", " + p.LastName + ", " + r.Abbreviation;
        }



        public EditViewShiftModel GetViewShift(int ShiftDetailId)
        {
            try
            {
                ShiftDetail shiftDetail = _context.ShiftDetails.Include(m => m.Shift).ThenInclude(m => m.Physician).FirstOrDefault(m => m.ShiftDetailId == ShiftDetailId);
                if (shiftDetail != null)
                {
                    EditViewShiftModel editViewShift = new EditViewShiftModel()
                    {
                        ShiftDetailId = ShiftDetailId,
                        PhysicianRegionVS = (int)shiftDetail.RegionId,
                        PhysicianRegionName = _context.Regions.FirstOrDefault(m => m.RegionId == shiftDetail.RegionId).Name,
                        PhysicianIdVS = shiftDetail.Shift.PhysicianId,
                        PhysicianName = shiftDetail.Shift.Physician.FirstName + " " + shiftDetail.Shift.Physician.LastName,
                        ShiftDateVS = shiftDetail.ShiftDate.ToString("yyyy-MM-dd"),
                        StartTimeVS = shiftDetail.StartTime,
                        EndTimeVS = shiftDetail.EndTime,
                    };
                    return editViewShift;
                }
                return new EditViewShiftModel();
            }
            catch { return new EditViewShiftModel(); }
        }

        public bool ReturnViewShift(int ShiftDetailId)
        {
            try
            {
                ShiftDetail shiftDetail = _context.ShiftDetails.FirstOrDefault(m => m.ShiftDetailId == ShiftDetailId);
                if (shiftDetail != null)
                {
                    if (shiftDetail.Status == 1)
                    {
                        shiftDetail.Status = 0;
                    }
                    else
                    {
                        shiftDetail.Status = 1;
                    }
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        public bool EditViewShift(EditViewShiftModel Shift)
        {
            try
            {
                ShiftDetail shiftDetail = _context.ShiftDetails.FirstOrDefault(m => m.ShiftDetailId == Shift.ShiftDetailId);
                if (shiftDetail != null)
                {
                    shiftDetail.ShiftDate = DateTime.Parse(Shift.ShiftDateVS);
                    shiftDetail.StartTime = Shift.StartTimeVS;
                    shiftDetail.EndTime = Shift.EndTimeVS;

                    _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        public bool DeleteViewShift(int ShiftDetailId)
        {
            try
            {
                ShiftDetail shiftDetail = _context.ShiftDetails.FirstOrDefault(m => m.ShiftDetailId == ShiftDetailId);
                if (shiftDetail != null)
                {
                    shiftDetail.IsDeleted = new BitArray(1, true);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        public bool CreateNewShift(SchedulingViewModel model, List<int> RepeatedDays, int id)
        {
            // one entry in shift and multiple entries in shiftdetail

            AspNetUser anu = GetAspNetFromAdminId(id);

            Shift shift = new Shift();
            shift.PhysicianId = (int)model.physicianId;
            shift.StartDate = DateOnly.FromDateTime((DateTime)model.startDate);

            // check if shift is repeated or not
            if (model.repeat != 0)
            {
                shift.IsRepeat = new BitArray(1, true);
            }
            else
            {
                shift.IsRepeat = new BitArray(1, false);
            }

            // if shift is repeated and checkboxes are checked, set 1 to those weekdays in shift.WeekDays else set 0
            if (shift.IsRepeat != new BitArray(1, false) && RepeatedDays != null)
            {

                for (int i = 0; i < 7; i++)
                {
                    if (RepeatedDays!.Any(u => u == i))
                    {
                        shift.WeekDays = shift.WeekDays + "1";
                    }
                    else
                    {
                        shift.WeekDays = shift.WeekDays + "0";
                    }
                }
            }
            else
            {
                shift.WeekDays = "0000000";
            }

            shift.RepeatUpto = model.repeat;
            shift.CreatedBy = anu.Id;
            shift.CreatedDate = DateTime.Now;
            _context.Shifts.Add(shift);
            _context.SaveChanges();

            // common entry of shift whose startDate = model.startDate
            ShiftDetail sd = new ShiftDetail();
            sd.Shift = shift;
            sd.ShiftDate = (DateTime)model.startDate;
            sd.RegionId = model.regionId;
            sd.StartTime = (TimeOnly)model.startTime;
            sd.EndTime = (TimeOnly)model.endTime;
            sd.Status = 0;
            sd.IsDeleted = new BitArray(1, false);
            _context.ShiftDetails.Add(sd);

            // if shift is repeated and atleast one checkbox is checked
            if (shift.IsRepeat != new BitArray(1, false) && RepeatedDays != null)
            {
                int current = 0; //variable to count and stop when the total numbered of entered data is greater than 'total'
                int total = RepeatedDays.Count() * (int)model.repeat; //total number of entries in ShiftDetail other than current entry (when we submit)

                for (int i = 0; i <= model.repeat; i++)
                {
                    DateTime shiftDate = (DateTime)model.startDate;
                    DateTime tempdate = new DateTime();

                    // when we want to store the repeated shift related data in current week
                    if (i == 0)
                    {
                        // if day of shiftDate is wednesday, then 0-4 = -4 i.e. day of tempdate would be sunday
                        tempdate = shiftDate.AddDays((7 * i) - (int)shiftDate.DayOfWeek);

                        // make entry for each checked day
                        foreach (var day in RepeatedDays)
                        {
                            // if checked day is greater than day of shiftDate, which is Sunday
                            if (day > (int)shiftDate.DayOfWeek)
                            {
                                int count = day;
                                ShiftDetail shiftDetail1 = new ShiftDetail();
                                shiftDetail1.Shift = shift;
                                shiftDetail1.ShiftDate = tempdate.AddDays(count);
                                shiftDetail1.RegionId = model.regionId;
                                shiftDetail1.StartTime = (TimeOnly)model.startTime!;
                                shiftDetail1.EndTime = (TimeOnly)model.endTime!;
                                shiftDetail1.Status = 0;
                                shiftDetail1.IsDeleted = new BitArray(1, false);
                                _context.ShiftDetails.Add(shiftDetail1);
                                current++;
                            }
                        }
                    }

                    // to store data for the shift which is going to repeat in next week
                    else
                    {
                        // start from Sunday of next week
                        tempdate = shiftDate.AddDays((7 * i) - (int)shiftDate.DayOfWeek);
                        for (int j = 0; j < 7; j++)
                        {
                            // break when number of entered entries in ShiftDetail increases than total variable
                            if (total <= current)
                            {
                                break;
                            }

                            // check if j ==  any of the checked day and if that is true, do entry in table and set current = current + 1;
                            if (RepeatedDays.Any(r => r == j))
                            {
                                ShiftDetail shiftDetail2 = new ShiftDetail();
                                shiftDetail2.Shift = shift;
                                shiftDetail2.ShiftDate = tempdate.AddDays(j);
                                shiftDetail2.RegionId = model.regionId;
                                shiftDetail2.StartTime = (TimeOnly)model.startTime!;
                                shiftDetail2.EndTime = (TimeOnly)model.endTime!;
                                shiftDetail2.Status = 0;
                                shiftDetail2.IsDeleted = new BitArray(1, false);

                                _context.ShiftDetails.Add(shiftDetail2);
                                current = current + 1;
                            }
                        }
                    }
                }
            }

            _context.SaveChanges();
            return true;
        }

        public void ApproveSelectedShifts(string shiftDetailIdString)
        {
            string[] detailId = shiftDetailIdString.Split(',').Select(x => x.Trim()).ToArray();
            for (int i = 0; i < detailId.Length; i++)
            {
                ShiftDetail sd = _context.ShiftDetails.Where(s => s.ShiftDetailId == int.Parse(detailId[i])).FirstOrDefault();
                sd.Status = 1;
                _context.ShiftDetails.Update(sd);
            }
            _context.SaveChanges();
        }

        public void DeleteSelectedShifts(string shiftDetailIdString)
        {
            string[] detailId = shiftDetailIdString.Split(',').Select(x => x.Trim()).ToArray();
            for (int i = 0; i < detailId.Length; i++)
            {
                ShiftDetail sd = _context.ShiftDetails.Where(s => s.ShiftDetailId == int.Parse(detailId[i])).FirstOrDefault();
                sd.IsDeleted = new BitArray(1, true);
                _context.ShiftDetails.Update(sd);
            }
            _context.SaveChanges();
        }

        public MdsOnCallViewModel GetMdsData(AdminNavbarModel an)
        {
            List<Physician> allPhysician = _context.Physicians.ToList();
            List<Physician> onCall = new List<Physician>();
            List<Physician> offDuty = new List<Physician>();

            List<ShiftDetail> shifts = _context.ShiftDetails.Where(s => s.ShiftDate.Date == DateTime.Now.Date && TimeOnly.FromDateTime(DateTime.Now) >= s.StartTime && TimeOnly.FromDateTime(DateTime.Now) <= s.EndTime && s.Status == 1 && s.IsDeleted == new BitArray(1, false)).Include(sh => sh.Shift).Include(shi => shi.Shift.Physician).ToList();

            foreach (var item in shifts)
            {
                Physician p = item.Shift.Physician;
                onCall.Add(p);
            }

            foreach (var item in allPhysician)
            {
                if (onCall.Any(r => r == item))
                {
                    continue;
                }
                else
                {
                    offDuty.Add(item);
                }
            }

            MdsOnCallViewModel moc = new MdsOnCallViewModel
            {
                providersOnCall = onCall,
                providersOffDuty = offDuty,
                allRegions = GetAllRegion(),
                adminNavbarModel = an,
            };

            return moc;
        }

        public bool AddNewVendor(AddVendorViewModel model)
        {
            HalloDoc.DataLayer.Models.Region r = GetRegFromId(model.regionId);
            HealthProfessional hp = new HealthProfessional
            {
                VendorName = model.businessName,
                Profession = model.professionId,
                FaxNumber = model.faxNumber,
                Address = model.street,
                City = model.city,
                State = r.Name,
                Zip = model.zipCode,
                RegionId = model.regionId,
                CreatedDate = DateTime.Now,
                IsDeleted = new BitArray(1, false),
                Email = model.email,
                PhoneNumber = model.phoneNumber,
                BusinessContact = model.businessContact
            };

            _context.HealthProfessionals.Add(hp);
            _context.SaveChanges();
            return true;
        }

        public AddVendorViewModel GetVendorDataFromId(int id, AdminNavbarModel an)
        {
            HealthProfessional hp = _context.HealthProfessionals.Where(h => h.VendorId == id).FirstOrDefault();
            AddVendorViewModel av = new AddVendorViewModel
            {
                vendorId = id,
                adminNavbarModel = an,
                businessName = hp.VendorName,
                professionId = (int)hp.Profession,
                faxNumber = hp.FaxNumber,
                phoneNumber = hp.PhoneNumber,
                email = hp.Email,
                businessContact = hp.BusinessContact,
                street = hp.Address,
                city = hp.City,
                regionId = (int)hp.RegionId,
                zipCode = hp.Zip,
                professionType = GetHealthProfessionalType(),
                allRegions = GetAllRegion(),
            };
            return av;
        }

        public bool SaveEditedBusinessInfo(AddVendorViewModel model, int id)
        {
            HealthProfessional hp = _context.HealthProfessionals.Where(h => h.VendorId == id).FirstOrDefault();
            HalloDoc.DataLayer.Models.Region r = GetRegFromId(model.regionId);

            hp.VendorName = model.businessName;
            hp.Profession = model.professionId;
            hp.FaxNumber = model.faxNumber;
            hp.Address = model.street;
            hp.City = model.city;
            hp.State = r.Name;
            hp.Zip = model.zipCode;
            hp.RegionId = model.regionId;
            hp.CreatedDate = DateTime.Now;
            hp.IsDeleted = new BitArray(1, false);
            hp.Email = model.email;
            hp.PhoneNumber = model.phoneNumber;
            hp.BusinessContact = model.businessContact;

            _context.HealthProfessionals.Update(hp);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteBusinessProfile(int id)
        {
            HealthProfessional hp = _context.HealthProfessionals.Where(h => h.VendorId == id).FirstOrDefault();
            hp.IsDeleted = new BitArray(1, true);
            _context.HealthProfessionals.Update(hp);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteSearchRecord(int id)
        {
            Request r = _context.Requests.Where(re => re.RequestId == id).FirstOrDefault();
            r.IsDeleted = new BitArray(1, true);
            _context.Requests.Update(r);
            _context.SaveChanges();
            return true;
        }

        public void AddSmsLogFromSendLink(string body, string number, int? adminId, DateTime temp, int count, bool isSMSSent, int action)
        {
            Smslog sl = new Smslog();
            sl.Smstemplate = body;
            sl.MobileNumber = number;
            sl.AdminId = adminId;
            sl.CreateDate = temp;
            sl.SentDate = isSMSSent ? DateTime.Now : null;
            sl.IsSmssent = isSMSSent ? new BitArray(1, true) : new BitArray(1, false);
            sl.SentTries = count;
            sl.Action = action;
            sl.RoleId = 2;

            _context.Smslogs.Add(sl);
            _context.SaveChanges();
        }

        public void AddSmsLogFromSendOrder(string body, string number, int? adminId, DateTime temp, int count, bool isSMSSent, int action)
        {
            Smslog sl = new Smslog();
            sl.Smstemplate = body;
            sl.MobileNumber = number;
            sl.AdminId = adminId;
            sl.CreateDate = temp;
            sl.SentDate = isSMSSent ? DateTime.Now : null;
            sl.IsSmssent = isSMSSent ? new BitArray(1, true) : new BitArray(1, false);
            sl.SentTries = count;
            sl.Action = action;

            _context.Smslogs.Add(sl);
            _context.SaveChanges();
        }

        public void AddSmsLogFromContactProvider(string body, string number, int? adminId, int phyId, DateTime temp, int count, bool isSMSSent, int action)
        {
            Smslog sl = new Smslog();
            sl.Smstemplate = body;
            sl.MobileNumber = number;
            sl.AdminId = adminId;
            sl.CreateDate = temp;
            sl.SentDate = isSMSSent ? DateTime.Now : null;
            sl.IsSmssent = isSMSSent ? new BitArray(1, true) : new BitArray(1, false);
            sl.SentTries = count;
            sl.Action = action;
            sl.RoleId = 2;
            sl.PhysicianId = phyId;

            _context.Smslogs.Add(sl);
            _context.SaveChanges();
        }

        public void AddEmailLog(string body, string subject, string email, int? RoleId, string? filePath, string? ConfirmationNumber, int? RequestId, int? AdminId, int? PhysicianId, DateTime? createdDate, bool isEmailSent, int emailSentCount)
        {

            EmailLog emailLog = new EmailLog
            {
                EmailTemplate = body,
                SubjectName = subject,
                EmailId = email,
                ConfirmationNumber = ConfirmationNumber,
                FilePath = filePath,
                RoleId = RoleId == -1 ? null : RoleId,
                RequestId = RequestId,
                AdminId = AdminId,
                PhysicianId = PhysicianId,
                IsEmailSent = new BitArray(1, isEmailSent),
                SentTries = emailSentCount,
                CreateDate = (DateTime)createdDate,
                SentDate = DateTime.Now
            };

            _context.EmailLogs.Add(emailLog);
            _context.SaveChanges();

        }

    }
}
