using DocumentFormat.OpenXml.Office2016.Excel;
using HalloDoc.DataLayer.Data;
using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using HalloDocMvc.Entity.ViewModel;
using Microsoft.AspNetCore.Http;
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

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class AdminInterface : IAdminInterface
    {
        private readonly ApplicationDbContext _context;

        public AdminInterface(ApplicationDbContext context)
        {
            _context = context;
        }

        AdminDashboardTableView IAdminInterface.ModelOfAdminDashboard(string status, int id, string? search, string? requestor, int? region)
        {

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

            Admin ad = _context.Admins.Where(a => a.AdminId == id).FirstOrDefault();
            var count_new = _context.Requests.Count(r => r.Status == 1);
            var count_pending = _context.Requests.Count(r => r.Status == 2);
            var count_active = _context.Requests.Count(r => r.Status == 4 || r.Status == 5);
            var count_conclude = _context.Requests.Count(r => r.Status == 6);
            var count_toclose = _context.Requests.Count(r => r.Status == 3 || r.Status == 7 || r.Status == 8);
            var count_unpaid = _context.Requests.Count(r => r.Status == 9);
            List<HalloDoc.DataLayer.Models.Region> r = _context.Regions.ToList();
            List<CaseTag> c = _context.CaseTags.ToList();

            IQueryable<Request> query = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(exp).OrderByDescending(e => e.CreatedDate);

            if (search != null)
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
                Admin_Name = string.Concat(ad.FirstName, " ", ad.LastName),
                requests = query.ToList(),
            };
            //if (status == "New")
            //{
            //    adminDashboardViewModel.query_requests = _context.Requests.Include(r => r.RequestWiseFiles).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 1);
            //    adminDashboardViewModel.requests = query.ToList();
            //}
            //else if (status == "Pending")
            //{
            //    adminDashboardViewModel.query_requests = _context.Requests.Include(r => r.RequestWiseFiles).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 2);
            //    adminDashboardViewModel.requests = query.ToList();
            //}
            //else if (status == "Active")
            //{
            //    adminDashboardViewModel.query_requests = _context.Requests.Include(r => r.RequestWiseFiles).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 4 || r.Status == 5);
            //    adminDashboardViewModel.requests = query.ToList();
            //}
            //else if (status == "Conclude")
            //{
            //    adminDashboardViewModel.query_requests = _context.Requests.Include(r => r.RequestWiseFiles).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 6);
            //    adminDashboardViewModel.requests = query.ToList();
            //}
            //else if (status == "ToClose")
            //{
            //    adminDashboardViewModel.query_requests = _context.Requests.Include(r => r.RequestWiseFiles).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 3 || r.Status == 7 || r.Status == 8);
            //    adminDashboardViewModel.requests = query.ToList();
            //}
            //else
            //{
            //    adminDashboardViewModel.query_requests = _context.Requests.Include(r => r.RequestWiseFiles).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 9);
            //    adminDashboardViewModel.requests = query.ToList();
            //}
            return adminDashboardViewModel;
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

        public Physician FetchPhysician(int id)
        {
            return _context.Physicians.FirstOrDefault(p => p.PhysicianId == id);
        }

        public void EditViewNotesAction(RequestNote rn, ViewNotes model)
        {
            rn.AdminNotes = model.AdminNotes;
            _context.RequestNotes.Update(rn);
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

        public void AddRequestStatusLogFromAgreement(RequestStatusLog rsl)
        {
            _context.RequestStatusLogs.Add(rsl);
            _context.SaveChanges();
        }

        public List<Physician> FetchPhysicianByRegion(int RegionId)
        {
            return _context.Physicians.Where(p => p.RegionId == RegionId).ToList();
        }

        public void AddBlockRequestData(BlockRequest br)
        {
            _context.BlockRequests.Add(br);
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
            User user = new User();
            Request request = new Request();
            DataLayer.Models.Region region2 = new DataLayer.Models.Region();
            RequestClient requestClient = new RequestClient();
            RequestStatusLog requestStatusLog = new RequestStatusLog();
            RequestNote requestNote = new RequestNote();

            bool userExists = true;
            if (ValidateAspNetUser(model) == null)
            {
                userExists = false;
                aspNetUser.UserName = model.Email;
                aspNetUser.Email = model.Email;
                aspNetUser.PhoneNumber = model.PhoneNumber;
                aspNetUser.CreatedDate = DateTime.Now;
                aspNetUser.PasswordHash = model.Password;
                _context.AspNetUsers.Add(aspNetUser);
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

        public Admin ValidateUser(LoginViewModel model)
        {
            Admin user = _context.Admins.FirstOrDefault(x => x.Email == model.UserName);
            return user;
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
            return _context.HealthProfessionals.Where(h => h.Profession == professionId).ToList();
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

        public void UpdateEncounterFormData(EncounterFormModel model, RequestClient rc)
        {
            string address = model.Location;
            int firstCom = address.IndexOf(',');
            string street = firstCom >= 0 ? address.Substring(0, firstCom) : address;
            int secondCom = address.IndexOf(',', firstCom+1);
            string city = "";
            if (secondCom != -1)
            {
                city = address.Substring(firstCom + 2, secondCom - (firstCom + 2));
            }
            string[] parts = address.Split(',');
            string state = parts.Length >= 2 ? parts[parts.Length - 2].Trim() : "";
            int lastCommaIndex = address.LastIndexOf(',');
            string zipcode = address.Substring(lastCommaIndex + 1).Trim();

            

            rc.FirstName = model.FirstName;
            rc.LastName = model.LastName;
            rc.Email = model.Email;
            rc.PhoneNumber = model.PhoneNumber;
            rc.Street = street;
            rc.City = city;
            rc.State = state;
            rc.ZipCode = zipcode;

            _context.RequestClients.Update(rc);
            _context.SaveChanges();
        }

    }
}
