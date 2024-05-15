using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class PatientDashboard : IPatientDashboard
    {
        private readonly ApplicationDbContext _context;
        public PatientDashboard(ApplicationDbContext context)
        {
            _context = context;
        }
        public DashboardViewModel GetDashboardData(int id)
        {
            User users = _context.Users.FirstOrDefault(us => us.UserId == id);
            AspNetUser anu = _context.AspNetUsers.FirstOrDefault(a => a.Id == users.AspNetUserId);

            IEnumerable<Request> req = _context.Requests.Where(r => r.UserId == users.UserId).OrderByDescending(r => r.CreatedDate);
            IEnumerable<RequestWiseFile> rwf = _context.RequestWiseFiles;
            List<Physician> ph = _context.Physicians.OrderByDescending(p => p.CreatedDate).ToList();

            var requestsAndFile = _context.Requests
                .Join(
                _context.RequestWiseFiles,
                r => r.RequestId,
                rf => rf.RequestId,
                (r, rf) => new RequestFileViewModel { RequestId = r.RequestId, fileName = rf.FileName }).ToList();

            var viewModel = new DashboardViewModel
            {
                UserModel = users,
                Requests = req,
                RequestsAndFiles = requestsAndFile,
                phy = ph,
                User = anu,
            };

            return viewModel;
        }
        public string ValidateUsername(int id)
        {
            return _context.Users.FirstOrDefault(t => t.UserId == id).FirstName;
        }

        public string FullNameFromUserId(int id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == id).FirstName + " " + _context.Users.FirstOrDefault(u => u.UserId == id).LastName;
        }
    }
}
