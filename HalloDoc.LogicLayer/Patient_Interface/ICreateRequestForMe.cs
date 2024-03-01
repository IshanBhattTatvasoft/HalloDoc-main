using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface ICreateRequestForMe
    {
        public void RequestForMe(PatientRequestModel model, int user, Region region);
        public Region ValidateRegion(PatientRequestModel model);
        public User ValidateUser(int user_id);


    }
}
