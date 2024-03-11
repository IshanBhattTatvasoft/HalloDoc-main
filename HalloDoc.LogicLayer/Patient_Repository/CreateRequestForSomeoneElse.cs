using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using System;
using HalloDoc.DataLayer.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class CreateRequestForSomeoneElse : ICreateRequestForSomeoneElse
    {
        private readonly ApplicationDbContext _context;
        public CreateRequestForSomeoneElse(ApplicationDbContext context)
        {
            _context = context;
        }

        public Region ValidateRegion(PatientRequestSomeoneElse model)
        {
            return _context.Regions.FirstOrDefault(u => u.Name == model.State.Trim().ToLower().Replace(" ", ""));
        }

        public User ValidateUser(PatientRequestSomeoneElse model, int user_id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == user_id);
        }

        public BlockRequest CheckForBlockedRequest(PatientRequestSomeoneElse model)
        {
            return _context.BlockRequests.FirstOrDefault(u => u.Email == model.Email);
        }

        public void RequestForSomeoneElse(PatientRequestSomeoneElse model, int user, User users, Region region1)
        {
            Request request = new Request();
            DataLayer.Models.Region region = new DataLayer.Models.Region();
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
            requestClient.ZipCode = model.ZipCode;
            _context.RequestClients.Add(requestClient);
            _context.SaveChangesAsync();

            int requests = _context.Requests.Where(u => u.CreatedDate == DateTime.Now.Date).Count();
            string ConfirmationNumber = string.Concat(region1.Abbreviation, users.FirstName.Substring(0, 2).ToUpper(), users.LastName.Substring(0, 2).ToUpper(), requests.ToString("D" + 4));
            request.RequestTypeId = 2;

            request.CreatedUserId = users.UserId;
            request.FirstName = users.FirstName;
            request.LastName = users.LastName;
            request.Email = users.Email;
            request.PhoneNumber = users.Mobile;
            request.Status = 1;
            request.CreatedDate = DateTime.Now;
            request.RequestClientId = requestClient.RequestClientId;
            request.ConfirmationNumber = ConfirmationNumber;
            request.RelationName = model.Relation;
            _context.Requests.Add(request);
            _context.SaveChangesAsync();

            if (model.File != null)
            {
                requestWiseFile.RequestId = request.RequestId;
                requestWiseFile.FileName = model.File.FileName;
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
        }
    }
}
