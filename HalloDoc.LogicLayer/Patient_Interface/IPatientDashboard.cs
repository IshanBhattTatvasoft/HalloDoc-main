using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface IPatientDashboard
    {
        public List<TableContent> GetDashboardData(int id);
        public string ValidateUsername(int id);

    }
}
