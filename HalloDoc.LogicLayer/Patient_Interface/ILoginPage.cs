using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface ILoginPage
    {
        public AspNetUser ValidateAspNetUser(LoginViewModel model);
        public User ValidateUsers(LoginViewModel model);
    }
}
