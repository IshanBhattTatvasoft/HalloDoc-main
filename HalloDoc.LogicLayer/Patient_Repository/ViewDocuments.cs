using HalloDoc.DataLayer.Models;
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

            return _context.Requests.Include(r => r.RequestClient).FirstOrDefault(u => u.RequestId == requestId);

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

        public void AddFile(ViewDocumentModel model, int requestId)
        {
            var viewModel = new ViewDocumentModel
            {
                ImageContent = model.ImageContent,
            };
            if (model.ImageContent != null && model.ImageContent.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", model.ImageContent.FileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    model.ImageContent.CopyToAsync(stream);
                }
            }
            if (model.ImageContent != null)
            {
                RequestWiseFile requestWiseFile = new RequestWiseFile
                {
                    FileName = model.ImageContent.FileName,
                    CreatedDate = DateTime.Now,
                    RequestId = (int)model.requestId,
                    IsDeleted = new System.Collections.BitArray(1, false)
                };
                _context.RequestWiseFiles.Add(requestWiseFile);
            }
            _context.SaveChanges();
        }


    }
}
