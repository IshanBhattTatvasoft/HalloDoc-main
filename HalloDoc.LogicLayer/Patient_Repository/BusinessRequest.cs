using HalloDoc.DataLayer.Data;
using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.DataLayer.Data;
using HalloDoc.LogicLayer.Patient_Interface;

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class BusinessRequest : IBusinessRequest
    {
        private readonly ApplicationDbContext _context;

        public BusinessRequest(ApplicationDbContext context)
        {
            _context = context;
        }
        public AspNetUser ValidateAspNetUser(BusinessRequestModel model)
        {
            return _context.AspNetUsers.SingleOrDefault(u => u.UserName == model.Email);
        }

        public DataLayer.Models.Region ValidateRegion(BusinessRequestModel model)
        {
            var temp = model.State.ToLower().Trim();
            return _context.Regions.FirstOrDefault(u => u.Name.ToLower().Trim().Equals(temp));
        }

        public void InsertDataBusinessRequest(BusinessRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            User user = new User();
            Request request = new Request();
            DataLayer.Models.Region region2 = new DataLayer.Models.Region();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();
            Business business = new Business();
            RequestBusiness requestBusiness = new RequestBusiness();
            int atIndex = model.Email.IndexOf("@");

            bool userExists = true;
            if (ValidateAspNetUser(model) == null)
            {
                userExists = false;
                aspNetUser.UserName = atIndex >= 0 ? model.Email.Substring(0, atIndex) : model.Email;
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
            Region r = _context.Regions.Where(re => re.Name == model.State).FirstOrDefault();
            requestClient.FirstName = model.FirstName;
            requestClient.LastName = model.LastName;
            requestClient.PhoneNumber = model.PhoneNumber;
            requestClient.Location = model.City;
            requestClient.Address = model.Street;
            requestClient.RegionId = r.RegionId;
            requestClient.Notes = model.Symptoms;
            requestClient.Email = model.Email;
            requestClient.IntDate = model.DOB.Day;
            requestClient.StrMonth = model.DOB.Month.ToString();
            requestClient.IntYear = model.DOB.Year;
            requestClient.Street = model.Street;
            requestClient.City = model.City;
            requestClient.State = model.State;
            requestClient.ZipCode = model.Zipcode;
            _context.RequestClients.Add(requestClient);
            _context.SaveChangesAsync();

            int requests = _context.Requests.Where(u => u.CreatedDate.Date == DateTime.Now.Date).Count();
            string ConfirmationNumber = string.Concat(region2.Abbreviation, model.FirstName.Substring(0, 2).ToUpper(), model.LastName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));

            request.RequestTypeId = 4;
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
            request.FirstName = model.BusinessFirstName;
            request.LastName = model.BusinessLastName;
            request.Email = model.BusinessEmail;
            request.ConfirmationNumber = ConfirmationNumber;
            request.PhoneNumber = model.BusinessPhoneNumber;
            request.Status = 1;
            request.CreatedDate = DateTime.Now;
            request.RequestClientId = requestClient.RequestClientId;
            _context.Requests.Add(request);
            _context.SaveChangesAsync();

            //if (model.File != null)
            //{
            //    requestWiseFile.RequestId = request.RequestId;
            //    requestWiseFile.FileName = model.File;
            //    requestWiseFile.CreatedDate = DateTime.Now;
            //    _context.RequestWiseFiles.Add(requestWiseFile);
            //    _context.SaveChangesAsync();
            //}

            requestStatusLog.RequestId = request.RequestId;
            requestStatusLog.Status = 1;
            requestStatusLog.Notes = model.Symptoms;
            requestStatusLog.CreatedDate = DateTime.Now;
            _context.RequestStatusLogs.Add(requestStatusLog);
            _context.SaveChangesAsync();

            business.Name = model.BusinessFirstName + " " + model.BusinessLastName;
            business.Address1 = model.BusinessPropertyName;
            business.Address2 = model.BusinessPropertyName;
            business.City = model.BusinessPropertyName;
            business.ZipCode = "361002";
            //business.PhoneNumber = model.BusinessPhoneNumber;
            business.CreatedDate = DateTime.Now;
            business.RegionId = 1;
            _context.Businesses.Add(business);
            _context.SaveChangesAsync();

            requestBusiness.RequestId = request.RequestId;
            requestBusiness.BusinessId = business.BusinessId;
            _context.RequestBusinesses.Add(requestBusiness);
            _context.SaveChangesAsync();
        }
    }
}
