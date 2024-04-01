using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using Microsoft.EntityFrameworkCore;
using System;
using HalloDoc.DataLayer.Data;
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
        public List<TableContent> GetDashboardData(int id)
        {
            Request r = _context.Requests.Where(r => r.UserId == id).FirstOrDefault();
            int count = _context.RequestWiseFiles.Where(rwf => rwf.RequestId == r.RequestId).Count();

            var data = (
        from req in _context.Requests
        join file in _context.RequestWiseFiles on req.RequestId equals file.RequestId into files
        from file in files.DefaultIfEmpty()
        where req.UserId == id
        group file by new { req.RequestId, req.CreatedDate, req.Status } into fileGroup
        select new TableContent
        {
            RequestId = fileGroup.Key.RequestId,
            CreatedDate = fileGroup.Key.CreatedDate,
            Status = fileGroup.Key.Status,
            Count = count,
        }).ToList();

            
            return data;
        }
        public string ValidateUsername(int id)
        {
            return _context.Users.FirstOrDefault(t => t.UserId == id).FirstName;
        }
    }
}
