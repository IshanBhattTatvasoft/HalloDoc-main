using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public  class CreateRequestForMe : ICreateRequestForMe
    {
        private readonly ApplicationDbContext _context;

        public CreateRequestForMe(ApplicationDbContext context)
        {
            _context = context;
        }

        public Region ValidateRegion(PatientRequestModel model)
        {
            var temp = model.State.ToLower().Trim();
            return _context.Regions.FirstOrDefault(u => u.Name.ToLower().Trim().Equals(temp));
        }

        public User ValidateUser(int user_id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == user_id);
        }

        public void RequestForMe(PatientRequestModel model, int user, Region region)
        {
            Request request = new Request();
            RequestClient requestClient = new RequestClient();
            RequestWiseFile requestWiseFile = new RequestWiseFile();
            RequestStatusLog requestStatusLog = new RequestStatusLog();

            requestClient.FirstName = model.FirstName;
            requestClient.LastName = model.LastName;
            requestClient.PhoneNumber = model.PhoneNumber;
            requestClient.Location = model.City;
            requestClient.Address = model.Street;
            requestClient.RegionId = 1;

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
            _context.SaveChangesAsync();

            int requests = _context.Requests.Where(u => u.CreatedDate.Date == DateTime.Now.Date).Count();
            Region r = _context.Regions.Where(re => re.Name.ToLower() == model.State).FirstOrDefault();
            string ConfirmationNumber = string.Concat(r.Abbreviation, DateTime.Now.Date.ToString("yyyyMMdd").Substring(0, 4), model.LastName.Substring(0, 2).ToUpper(), model.FirstName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));
            request.RequestTypeId = 1;

            request.UserId = user;
            request.FirstName = model.FirstName;
            request.LastName = model.LastName;
            request.Email = model.Email;
            request.PhoneNumber = model.PhoneNumber;
            request.Status = 1;
            request.CreatedDate = DateTime.Now;
            request.RequestClientId = requestClient.RequestClientId;
            request.ConfirmationNumber = ConfirmationNumber;
            request.IsDeleted = new System.Collections.BitArray(1, false);
            _context.Requests.Add(request);
            _context.SaveChangesAsync();

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
                _context.SaveChangesAsync();
            }

          
            _context.SaveChangesAsync();
        }
    }
}
