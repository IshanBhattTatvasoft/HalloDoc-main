using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDocMvc.Entity.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface IProviderInterface
    {
        public bool ConcludeCaseSubmitAction(ViewUploadsModel model, int id, Physician p);
        public List<ShiftDetail> GetProviderScheduleData(int id);
        public List<SchedulingViewModel> GetProviderInformation(int phyId);
        public bool CreateNewShift(SchedulingViewModel model, List<int> RepeatedDays, int id);
        public List<DataLayer.Models.Region> GetProviderRegionFromId(int id);
        public EditProviderAccountViewModel GetProviderProfile(int id, AdminNavbarModel an);
        public List<Admin> GetAllAdmins();
        public bool isEncounterFinalized(int id);
        public InvoicingViewModel GetBiWeeklyTimesheet(DateTime startDate, DateTime endDate, AdminNavbarModel an, int userId);
        public bool CheckFinalized(DateTime startDate, DateTime endDate, int userId);
        public InvoicingViewModel GetTimesheetOnInvoicing(DateTime startDate, DateTime endDate, AdminNavbarModel an, int userId);
        public bool SubmitTimesheet(InvoicingViewModel model, DateTime startDate, DateTime endDate, int id);
        public bool AddReimbursementData(int ind, DateTime startDate, DateTime endDate, int id, string item, int amount, IFormFile? upload);
        public void DeleteFile(int id);
        public bool FinalizeTimesheet(int id);
    }
}
