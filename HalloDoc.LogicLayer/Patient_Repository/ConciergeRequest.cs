using HalloDoc.DataLayer.Data;
using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;

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
            return _context.AspNetUsers.SingleOrDefault(u => u.Email == model.Email);
        }

        public void InsertDataConciergeRequest(ConceirgeRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            User user = _context.Users.SingleOrDefault(u => u.Email == model.Email);
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
                _context.SaveChangesAsync();

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
                _context.SaveChangesAsync();
            }
            requestClient.FirstName = model.FirstName;
            requestClient.LastName = model.LastName;
            requestClient.PhoneNumber = model.PhoneNumber;
            requestClient.Location = model.City;
            requestClient.Address = model.Street;
            requestClient.RegionId = 1;
            requestClient.Notes = model.Symptoms;
            requestClient.Email = model.Email;
            requestClient.IntDate = model.DOB.Day;
            requestClient.StrMonth = model.DOB.Month.ToString();
            requestClient.IntYear = model.DOB.Year;
            requestClient.Street = model.Street;
            requestClient.City = model.City;
            requestClient.State = model.State;
            requestClient.ZipCode = model.Zipcode;
            _context.RequestClients.AddAsync(requestClient);
            _context.SaveChangesAsync();

            int requests = _context.Requests.Where(u => u.CreatedDate == DateTime.Now.Date).Count();
            string ConfirmationNumber = string.Concat(region2.Abbreviation, model.FirstName.Substring(0, 2).ToUpper(), model.LastName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));

            request.RequestTypeId = 3;
            if (!userExists)
            {
                request.UserId = user.UserId;
            }
            request.FirstName = model.ConciergeFirstName;
            request.LastName = model.ConciergeLastName;
            request.Email = model.ConciergeEmail;
            request.ConfirmationNumber = ConfirmationNumber;
            request.PhoneNumber = model.ConciergePhoneNumber;
            request.Status = 1;
            request.CreatedDate = DateTime.Now;
            request.RequestClientId = requestClient.RequestClientId;
            _context.Requests.Add(request);
            _context.SaveChangesAsync();

            if (model.File != null)
            {
                requestWiseFile.RequestId = request.RequestId;
                requestWiseFile.FileName = model.File;
                requestWiseFile.CreatedDate = DateTime.Now;
                _context.RequestWiseFiles.Add(requestWiseFile);
                _context.SaveChangesAsync();
            }

            requestStatusLog.RequestId = request.RequestId;
            requestStatusLog.Status = 1;
            requestStatusLog.Notes = model.Symptoms;
            requestStatusLog.CreatedDate = DateTime.Now;
            _context.RequestStatusLogs.Add(requestStatusLog);
            _context.SaveChangesAsync();

            concierge.ConciergeName = model.ConciergeFirstName;
            concierge.Address = model.ConciergePropertyName;
            concierge.Street = model.ConciergeStreet;
            concierge.City = model.ConciergeCity;
            concierge.State = model.ConciergeState;
            concierge.ZipCode = model.ConciergeZipcode;
            concierge.CreatedDate = DateTime.Now;
            _context.Concierges.Add(concierge);
            _context.SaveChangesAsync();

            requestConcierge.RequestId = request.RequestId;
            requestConcierge.ConciergeId = concierge.ConciergeId;
            _context.RequestConcierges.Add(requestConcierge);
            _context.SaveChangesAsync();
        }
    }
}
