using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.Data;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class ViewDocuments : IViewDocuments
    {
        private readonly ApplicationDbContext _context;
        public ViewDocuments(ApplicationDbContext context)
        {
            _context = context;
        }

        public Request? GetRequestWithClient(int requestId)
        {

            return  _context.Requests.Include(r => r.RequestClient).FirstOrDefault(u => u.RequestId == requestId);

        }

        public List<RequestWiseFile>? ValidateFile(int requestid)
        {
            var rwf = _context.RequestWiseFiles.Include(u => u.Admin).Include(u => u.Physician).Where(u => u.RequestId == requestid).ToList();
            return rwf;

        }

        public User ValidateUser(int user_id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == user_id);
        }

        public string UserFirstName(int user_id)
        {
            return _context.Users.FirstOrDefault(t => t.UserId == user_id).FirstName;
        }

        public Request? GetRequestWithUser(int requestId)
        {
            using (_context)
            {
                return _context.Requests.Include(r => r.User).FirstOrDefault(u => u.RequestId == requestId);
            }
        }

        public void AddFile(string file, int id)
        {
            RequestWiseFile rwf = new RequestWiseFile();
            rwf.RequestId = id;
            rwf.FileName = file;
            rwf.CreatedDate = DateTime.Now;
            _context.RequestWiseFiles.Add(rwf);
            _context.SaveChanges();
        }


    }
}
