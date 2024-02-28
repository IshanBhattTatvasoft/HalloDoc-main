using HalloDoc.DataLayer.Data;
using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class LoginPage : ILoginPage
    {
        private readonly ApplicationDbContext _context;

        public LoginPage(ApplicationDbContext context)
        {
            _context = context;
        }
        public AspNetUser ValidateAspNetUser(LoginViewModel model)
        {
             return _context.AspNetUsers.FirstOrDefault(u => u.UserName == model.UserName);
             
        }

        public User ValidateUsers(LoginViewModel model)
        {
            User user = _context.Users.FirstOrDefault(x => x.Email == model.UserName);
            //User user1 = new User { UserId = user.UserId };
            return user;
        }
    }
}
