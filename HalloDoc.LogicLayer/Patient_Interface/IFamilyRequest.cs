using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface IFamilyRequest
    {
        public Region ValidateRegion(FamilyRequestModel model);
        public AspNetUser ValidateAspNetUser(FamilyRequestModel model);
        public void InsertDataFamilyRequest(FamilyRequestModel model);

    }
}
