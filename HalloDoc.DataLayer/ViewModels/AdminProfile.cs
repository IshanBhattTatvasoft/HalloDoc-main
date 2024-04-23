using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels
{
    public class AdminProfile
    {
        public string Username { get; set; }
        public List<Role> allRoles { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Please enter the First Name")]
        [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "First Name should must contain letters only")]
        public string firstName { get; set; }
        [Required(ErrorMessage = "Please enter the Last Name")]
        [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "Last Name should must contain letters only")]
        public string lastName { get; set; }
        [Required(ErrorMessage = "Please enter the Email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid Email")]
        public string email { get; set; }
        [Required(ErrorMessage = "Please enter the Confirmation Email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid Email")]
        [Compare("email", ErrorMessage = "Entered email does not matches with the email address")]
        public string confEmail { get; set; }
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[1-9][0-9]{9}$", ErrorMessage = "Please enter a valid 10-digit phone number")]
        public string phone { get; set; }
        [Required(ErrorMessage = "Please enter the first address")]
        public string address1 { get; set; }
        [Required(ErrorMessage = "Please enter the second address")]
        public string address2 { get; set; }
        [Required(ErrorMessage = "Please enter the City")]
        [RegularExpression(@"[a-zA-Z]{2,}$", ErrorMessage = "City Name must contain letters only")]
        public string city { get; set; }
        [Required(ErrorMessage = "Please enter the State")]
        public string state { get; set; }
        [Required(ErrorMessage = "Please enter the Zipcode")]
        public string zipcode { get; set; }
        [Required(ErrorMessage = "Please enter the Mailing Phone Number")]
        [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter valid phone number")]
        public string mailingPhoneNo { get; set; }
        public List<AdminRegion> regions { get; set; } = new List<AdminRegion>();
        public List<Region> allRegions { get; set; } = new List<Region>();
        public List<AdminRegion> regionOfAdmin { get; set; } = new List<AdminRegion>();
        public string altPhoneNo { get; set; }
        public AdminNavbarModel? an { get; set; }
        public int adminId { get; set; }
        [Required(ErrorMessage = "Please select a role")]
        public int roleId { get; set; }
        [Required(ErrorMessage = "Please select a region")]
        public int regionId { get; set; }
        public int status { get; set; }
        public string roleName { get; set; }
    }
}
