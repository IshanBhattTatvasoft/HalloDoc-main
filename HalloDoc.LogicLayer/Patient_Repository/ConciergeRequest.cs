using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.DataLayer.Data;
using HalloDoc.LogicLayer.Patient_Interface;
using System.Collections;

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class ConciergeRequest : IConciergeRequest
    {
        private readonly ApplicationDbContext _context;

        public ConciergeRequest(ApplicationDbContext context)
        {
            _context = context;
        }

        public AspNetUser ValidateAspNetUser(ConceirgeRequestModel model)
        {
            return _context.AspNetUsers.SingleOrDefault(u => u.UserName == model.Email);
        }

        public void InsertDataConciergeRequest(ConceirgeRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            AspNetUserRole anur = new AspNetUserRole();
            User user = new User();
            Request request = new Request();
            DataLayer.Models.Region region2 = new DataLayer.Models.Region();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();
            Concierge concierge = new Concierge();
            RequestConcierge requestConcierge = new RequestConcierge();

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

                anur.UserId = aspNetUser.Id;
                anur.RoleId = 3;
                _context.AspNetUserRoles.Add(anur);
                _context.SaveChanges();

                user.AspNetUserId = aspNetUser.Id;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.Mobile = model.PhoneNumber;
                user.Street = model.ConciergeStreet;
                user.City = model.ConciergeCity;
                user.State = model.ConciergeState;
                user.ZipCode = model.ConciergeZipcode;
                user.IntDate = model.DOB.Day;
                user.StrMonth = model.DOB.Month.ToString();
                user.IntYear = model.DOB.Year;
                user.CreatedBy = aspNetUser.Id;
                user.CreatedDate = DateTime.Now;
                _context.Users.Add(user);
                _context.SaveChanges();

            }
            Region r = _context.Regions.Where(re => re.Name == model.ConciergeState).FirstOrDefault();
            requestClient.FirstName = model.FirstName;
            requestClient.LastName = model.LastName;
            requestClient.PhoneNumber = model.PhoneNumber;
            requestClient.Location = model.ConciergeCity;
            requestClient.Address = model.ConciergeStreet;
            requestClient.RegionId = r.RegionId;
            requestClient.Notes = model.Symptoms;
            requestClient.Email = model.Email;
            requestClient.IntDate = model.DOB.Day;
            requestClient.StrMonth = model.DOB.Month.ToString();
            requestClient.IntYear = model.DOB.Year;
            requestClient.Street = model.ConciergeStreet;
            requestClient.City = model.ConciergeCity;
            requestClient.State = model.ConciergeState;
            requestClient.ZipCode = model.ConciergeZipcode;
            _context.RequestClients.Add(requestClient);
            _context.SaveChanges();


            int requests = _context.Requests.Where(u => u.CreatedDate.Date == DateTime.Now.Date).Count();
            string ConfirmationNumber = string.Concat(r.Abbreviation, DateTime.Now.Date.ToString().Substring(0, 4), model.LastName.Substring(0, 2).ToUpper(), model.FirstName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));

            request.RequestTypeId = 3;
            if (!userExists)
            {
                request.UserId = user.UserId;
            }
            else
            {
                AspNetUser anu = _context.AspNetUsers.Where(a => a.Email == model.Email).FirstOrDefault();
                User u = _context.Users.Where(u => u.AspNetUserId == anu.Id).FirstOrDefault();
                request.UserId = u.UserId;
            }
            request.FirstName = model.ConciergeFirstName;
            request.LastName = model.ConciergeLastName;
            request.Email = model.ConciergeEmail;
            request.ConfirmationNumber = ConfirmationNumber;
            request.PhoneNumber = model.ConciergePhoneNumber;
            request.Status = 1;
            request.CreatedDate = DateTime.Now;
            request.RequestClientId = requestClient.RequestClientId;
            request.IsDeleted = new BitArray(1, false);
            _context.Requests.Add(request);
            _context.SaveChanges();


            //if (model.File != null)
            //{
            //    requestWiseFile.RequestId = request.RequestId;
            //    requestWiseFile.FileName = model.File;
            //    requestWiseFile.CreatedDate = DateTime.Now;
            //    _context.RequestWiseFiles.Add(requestWiseFile);
            //    _context.SaveChangesAsync();
            //}

           
            

            concierge.ConciergeName = model.ConciergeFirstName;
            concierge.Address = model.ConciergePropertyName;
            concierge.Street = model.ConciergeStreet;
            concierge.City = model.ConciergeCity;
            concierge.State = model.ConciergeState;
            concierge.ZipCode = model.ConciergeZipcode;
            concierge.CreatedDate = DateTime.Now;
            _context.Concierges.Add(concierge);
            

            requestConcierge.RequestId = request.RequestId;
            requestConcierge.ConciergeId = concierge.ConciergeId;
            _context.RequestConcierges.Add(requestConcierge);
            _context.SaveChangesAsync();
        }
    }
}
