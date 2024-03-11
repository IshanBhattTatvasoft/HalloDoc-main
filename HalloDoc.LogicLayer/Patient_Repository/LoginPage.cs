using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDoc.LogicLayer.Patient_Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using HalloDoc.DataLayer.Data;
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

        public PasswordReset ValidateToken(string token)
        {
            return _context.PasswordResets.FirstOrDefault(pr => pr.Token == token);
        }

        public AspNetUser ValidateUserForResetPassword(ResetPasswordViewModel model, string useremail)
        {
            return _context.AspNetUsers.FirstOrDefault(x => x.Email == useremail);
        }

        public void SetPasswordForResetPassword(AspNetUser user, ResetPasswordViewModel model)
        {
            user.PasswordHash = model.Password;
            _context.SaveChanges();
        }

        public AspNetUserRole ValidateANUR(AspNetUser user)
        {
            return _context.AspNetUserRoles.Where(a => a.UserId == user.Id).FirstOrDefault();
        }

        public AspNetRole ValidateRole(AspNetUserRole anur)
        {
            return _context.AspNetRoles.Where(b => b.Id == anur.RoleId).FirstOrDefault();
        }
    }
}
