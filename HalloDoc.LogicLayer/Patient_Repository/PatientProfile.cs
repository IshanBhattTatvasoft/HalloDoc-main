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
    public class PatientProfile : IPatientProfile
    {
        private readonly ApplicationDbContext _context;

        public PatientProfile(ApplicationDbContext context)
        {
            _context = context;
        }

        public User ValidateUser(int user_id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == user_id);
        }

        public void EditPatientData(PatientProfileView model, int user_id)
        {
            User user = new User();

            user.UserId = (int)user_id;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Mobile = model.PhoneNumber;
            user.Street = model.Street;
            user.City = model.City;
            user.State = model.State;
            user.ZipCode = model.ZipCode;
            user.IntDate = model.DOB.Day;
            user.IntYear = model.DOB.Year;
            user.StrMonth = model.DOB.Month.ToString();
            _context.Users.Update(user);
            _context.SaveChanges();
        }
    }
}
