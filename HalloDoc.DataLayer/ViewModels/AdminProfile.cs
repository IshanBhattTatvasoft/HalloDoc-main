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
        [Required(ErrorMessage = "Password is empty")]
        public string Password { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        [Required]
        [EmailAddress]
        public string email { get; set; }
        [Required]
        [Compare("email", ErrorMessage = "Entered email does not matches with the email address")]
        public string confEmail { get; set; }
        public string phone { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zipcode { get; set; }
        public string phoneNo { get; set; }
        public List<AdminRegion> regions { get; set; } = new List<AdminRegion>();
        public List<Region> allRegions { get; set; } = new List<Region>();
        public List<AdminRegion> regionOfAdmin { get; set; } = new List<AdminRegion>();
        public string altPhoneNo { get; set; }
        public AdminNavbarModel? an { get; set; }
        public int adminId { get; set; }
    }
}
