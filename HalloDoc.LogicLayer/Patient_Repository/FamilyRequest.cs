using HalloDoc.LogicLayer.Patient_Interface;
using HalloDoc.DataLayer.Data;
using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class FamilyRequest : IFamilyRequest
    {
        private readonly ApplicationDbContext _context;
        public FamilyRequest(ApplicationDbContext context)
        {
            _context = context;
        }

        public DataLayer.Models.Region ValidateRegion(FamilyRequestModel model)
        {
            var temp = model.State.ToLower().Trim();
            return _context.Regions.FirstOrDefault(u => u.Name.ToLower().Trim().Equals(temp));
        }

        public AspNetUser ValidateAspNetUser(FamilyRequestModel model)
        {
            return _context.AspNetUsers.SingleOrDefault(u => u.UserName == model.Email);
        }

        public void InsertDataFamilyRequest(FamilyRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            AspNetUserRole anur = new AspNetUserRole();
            User user = new User();
            Request request = new Request();
            RequestClient requestClient = new RequestClient();
            DataLayer.Models.Region region2 = new DataLayer.Models.Region();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();
            int atIndex = model.Email.IndexOf("@");
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
            Region r = _context.Regions.Where(re => re.Name == model.State).FirstOrDefault();
            requestClient.FirstName = model.FirstName;
            requestClient.LastName = model.LastName;
            requestClient.PhoneNumber = model.PhoneNumber;
            requestClient.Location = model.City;
            requestClient.Address = model.Street;
            requestClient.RegionId = r.RegionId;
            if (model.Symptoms != null)
            {
                requestClient.Notes = model.Symptoms;
            }
            requestClient.Email = model.Email;
            requestClient.IntDate = model.DOB.Day;
            requestClient.StrMonth = model.DOB.Month.ToString();
            requestClient.IntYear = model.DOB.Year;
            requestClient.Street = model.Street;
            requestClient.City = model.City;
            requestClient.State = model.State;
            requestClient.ZipCode = model.Zipcode;
            _context.RequestClients.Add(requestClient);
            _context.SaveChanges();


            int requests = _context.Requests.Where(u => u.CreatedDate.Date == DateTime.Now.Date).Count();
            string ConfirmationNumber = string.Concat(r.Abbreviation, DateTime.Now.Date.ToString("yyyyMMdd").Substring(0, 4), model.LastName.Substring(0, 2).ToUpper(), model.FirstName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));

            request.RequestTypeId = 2;
            if (!userExists)
            {
                request.UserId = user.UserId;
            }
            else
            {
                AspNetUser anu = _context.AspNetUsers.Where(a => a.UserName == model.Email).FirstOrDefault();
                User u = _context.Users.Where(u => u.AspNetUserId == anu.Id).FirstOrDefault();
                request.UserId = u.UserId;
            }
            request.FirstName = model.FamilyFirstName;
            request.LastName = model.FamilyLastName;
            request.Email = model.FamilyEmail;
            request.ConfirmationNumber = ConfirmationNumber;
            request.PhoneNumber = model.FamilyPhoneNumber;
            request.Status = 1;
            request.CreatedDate = DateTime.Now;
            request.RequestClientId = requestClient.RequestClientId;
            request.IsDeleted = new System.Collections.BitArray(1, false);
            _context.Requests.Add(request);
            _context.SaveChanges();


            if (model.ImageContent != null && model.ImageContent.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", model.ImageContent.FileName);

                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    model.ImageContent.CopyToAsync(stream);
                }
                var filePath = model.ImageContent.FileName;

                requestWiseFile.RequestId = request.RequestId;
                requestWiseFile.FileName = filePath;
                requestWiseFile.CreatedDate = request.CreatedDate;
                requestWiseFile.IsDeleted = new System.Collections.BitArray(1, false);
                _context.RequestWiseFiles.Add(requestWiseFile);
           
            }

           
            _context.SaveChangesAsync();
        }

    }
}
