using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface IPatientProfile
    {
        public User ValidateUser(int user_id);
        public void EditPatientData(PatientProfileView model, int user_id);

    }
}
