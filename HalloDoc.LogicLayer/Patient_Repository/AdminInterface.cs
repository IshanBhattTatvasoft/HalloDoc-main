using DocumentFormat.OpenXml.Office2016.Excel;
using HalloDoc.DataLayer.Data;
using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using HalloDocMvc.Entity.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class AdminInterface : IAdminInterface
    {
        private readonly ApplicationDbContext _context;

        public AdminInterface(ApplicationDbContext context)
        {
            _context = context;
        }

        public AdminDashboardTableView ModelOfAdminDashboard(string? status)
        {
            var count_new = _context.Requests.Count(r => r.Status == 1);
            var count_pending = _context.Requests.Count(r => r.Status == 2);
            var count_active = _context.Requests.Count(r => r.Status == 3);
            var count_conclude = _context.Requests.Count(r => r.Status == 4);
            var count_toclose = _context.Requests.Count(r => r.Status == 5);
            var count_unpaid = _context.Requests.Count(r => r.Status == 6);

            AdminDashboardTableView adminDashboardViewModel = new AdminDashboardTableView
            {
                new_count = count_new,
                pending_count = count_pending,
                active_count = count_active,
                conclude_count = count_conclude,
                unpaid_count = count_unpaid,
                toclose_count = count_toclose,
                query_requests = _context.Requests.Include(r => r.RequestWiseFiles).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 1),
                requests = _context.Requests.Include(r => r.RequestClient).Include(r => r.Physician).Include(r => r.RequestStatusLogs).Where(r => r.Status == 1).ToList(),
                regions = _context.Regions.ToList(),
                status = status,
                caseTags = _context.CaseTags.ToList()
            };
            return adminDashboardViewModel;
        }

        public Request ValidateRequest(int requestId)
        {
            return _context.Requests.Where(r => r.RequestId == requestId).FirstOrDefault();
        }

        public RequestClient ValidateRequestClient(int requestClientId)
        {
            return _context.RequestClients.FirstOrDefault(s => s.RequestClientId == requestClientId);
        }

        public void EditViewCaseAction(ViewCaseModel userProfile, RequestClient userToUpdate)
        {
            userToUpdate.FirstName = userProfile.FirstName;
            userToUpdate.LastName = userProfile.LastName;
            userToUpdate.PhoneNumber = userProfile.PhoneNumber;
            userToUpdate.Email = userProfile.Email;
            userToUpdate.IntDate = userProfile.DOB.Day;
            userToUpdate.IntYear = userProfile.DOB.Year;
            userToUpdate.StrMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(userProfile.DOB.Month);
            _context.RequestClients.Update(userToUpdate);
            _context.SaveChanges();
        }

        public RequestNote FetchRequestNote(int requestId)
        {
            return _context.RequestNotes.FirstOrDefault(r => r.RequestId == requestId);
        }

        public RequestStatusLog FetchRequestStatusLogs(int requestId)
        {
            return _context.RequestStatusLogs.FirstOrDefault(r => r.RequestId == requestId);
        }

        public Physician FetchPhysician(int id)
        {
            return _context.Physicians.FirstOrDefault(p => p.PhysicianId == id);
        }

        public void EditViewNotesAction(RequestNote rn, ViewNotes model)
        {
            rn.AdminNotes = model.AdminNotes;
            _context.RequestNotes.Update(rn);
            _context.SaveChanges();
        }

        public CaseTag FetchCaseTag(int caseTagId)
        {
            return _context.CaseTags.Where(ct => ct.CaseTagId == caseTagId).FirstOrDefault();
        }

        public void AddRequestStatusLogFromCancelCase(RequestStatusLog rs)
        {
            _context.RequestStatusLogs.Add(rs);
            _context.SaveChanges();
        }

        public List<Physician> FetchPhysicianByRegion(int RegionId)
        {
            return _context.Physicians.Where(p => p.RegionId == RegionId).ToList();
        }

        public void AddBlockRequestData(BlockRequest br)
        {
            _context.BlockRequests.Add(br);
            _context.SaveChanges();
        }

        public void UpdateRequest(Request r)
        {
            _context.Requests.Update(r);
            _context.SaveChanges();
        }
    }
}
