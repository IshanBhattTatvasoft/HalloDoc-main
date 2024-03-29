﻿using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using HalloDoc.DataLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class PatientRequest : IPatientRequest
    {
        private readonly ApplicationDbContext _context;

        public PatientRequest(ApplicationDbContext context)
        {
            _context = context;
        }

        public Region ValidateRegion(PatientRequestModel model)
        {
            var temp = model.State.ToLower().Trim();
            return _context.Regions.FirstOrDefault(u => u.Name.ToLower().Trim().Equals(temp));
        }

        public BlockRequest ValidateBlockRequest(PatientRequestModel model)
        {
            return _context.BlockRequests.FirstOrDefault(u => u.Email == model.Email);
        }

        public AspNetUser ValidateAspNetUser(PatientRequestModel model)
        {
            return _context.AspNetUsers.SingleOrDefault(u => u.UserName == model.Email);
        }

        public void InsertDataPatientRequest(PatientRequestModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            User user = new User();
            Request request = new Request();
            Region region2 = new Region();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();
            int atIndex = model.Email.IndexOf("@");
            bool userExists = true;
            if (ValidateAspNetUser(model) == null)
            {
                userExists = false;
                aspNetUser.UserName = atIndex >= 0 ? model.Email.Substring(0, atIndex) : model.Email; ;
                aspNetUser.Email = model.Email;
                aspNetUser.PhoneNumber = model.PhoneNumber;
                aspNetUser.CreatedDate = DateTime.Now;
                aspNetUser.PasswordHash = model.Password;
                _context.AspNetUsers.Add(aspNetUser);
                

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
            //var temp = await _context.RequestClients.ToListAsync();
            _context.RequestClients.Add(requestClient);
            

            int requests = _context.Requests.Where(u => u.CreatedDate.Date == DateTime.Now.Date).Count();
            string ConfirmationNumber = string.Concat(region2.Abbreviation, model.FirstName.Substring(0, 2).ToUpper(), model.LastName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));

            request.RequestTypeId = 1;
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
            request.FirstName = model.FirstName;
            request.LastName = model.LastName;
            request.Email = model.Email;
            request.PhoneNumber = model.PhoneNumber;
            request.ConfirmationNumber = ConfirmationNumber;
            request.Status = 1;
            request.CreatedDate = DateTime.Now;
            request.RequestClientId = requestClient.RequestClientId;
            _context.Requests.Add(request);
            

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
                _context.RequestWiseFiles.Add(requestWiseFile);
                 
            }

            requestStatusLog.RequestId = request.RequestId;
            requestStatusLog.Status = 1;
            requestStatusLog.Notes = model.Symptoms;
            requestStatusLog.CreatedDate = DateTime.Now;
            _context.RequestStatusLogs.Add(requestStatusLog);
            _context.SaveChanges();
        }

        public AspNetUser GetEmailFromAspNet(string email)
        {
            return _context.AspNetUsers.SingleOrDefault(u => u.UserName == email);
        }

        public void InsertIntoAspNetUser(CreatePatientAccountViewModel model)
        {
            AspNetUser aspNetUser = new AspNetUser();
            int atIndex = model.email.IndexOf("@");
            aspNetUser.UserName = atIndex >= 0 ? model.email.Substring(0, atIndex) : model.email; ;
            aspNetUser.Email = model.email;
            aspNetUser.CreatedDate = DateTime.Now;
            aspNetUser.PasswordHash = model.Password;
            _context.AspNetUsers.Add(aspNetUser);
            _context.SaveChanges();
        }

        public void UpdateAspNetUserPass(CreatePatientAccountViewModel model)
        {
            AspNetUser anu = _context.AspNetUsers.SingleOrDefault(u => u.Email == model.email);
            anu.PasswordHash = model.Password;
            anu.CreatedDate = DateTime.Now;
            _context.AspNetUsers.Update(anu);
            _context.SaveChanges();
        }

        public void InsertPatientIntoUserRoles(CreatePatientAccountViewModel model)
        {
            AspNetUserRole anur = new AspNetUserRole();
            anur.RoleId = 3;
            AspNetUser anu = _context.AspNetUsers.Where(u => u.Email == model.email).SingleOrDefault();
            anur.UserId = anu.Id;
            _context.AspNetUserRoles.Add(anur);
            _context.SaveChanges();
        }

    }
}
