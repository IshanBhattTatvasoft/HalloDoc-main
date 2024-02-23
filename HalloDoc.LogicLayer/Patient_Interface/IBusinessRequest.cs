using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface IBusinessRequest
    {
        public AspNetUser ValidateAspNetUser(BusinessRequestModel model);
        public DataLayer.Models.Region ValidateRegion(BusinessRequestModel model);
        public void InsertDataBusinessRequest(BusinessRequestModel model);

    }
}
