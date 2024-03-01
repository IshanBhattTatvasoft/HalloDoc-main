using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface ICreateRequestForSomeoneElse
    {
        public Region ValidateRegion(PatientRequestSomeoneElse model);
        public User ValidateUser(PatientRequestSomeoneElse model, int user_id);
        public void RequestForSomeoneElse(PatientRequestSomeoneElse model, int user, User users, Region region);
        public BlockRequest CheckForBlockedRequest(PatientRequestSomeoneElse model);

    }
}
