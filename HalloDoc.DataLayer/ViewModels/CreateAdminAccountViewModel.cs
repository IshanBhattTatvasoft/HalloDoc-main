using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using HalloDoc.DataLayer.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;

public class CreateAdminAccountViewModel
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public int selectedRole { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string ConfEmail { get; set; }
    public string PhoneNo { get; set; }
    public List<Region> regions { get; set; }
    public List<Region> selectedregions { get; set; }
    public List<Role> roles { get; set; }
    public string Address1 { get; set; }
    public string? Address2 { get; set; }
    [Required(ErrorMessage = "Please enter the City ")]
    public string? City { get; set; }
    [Required(ErrorMessage = "Please enter the State ")]
    public string? State { get; set; }
    [Required(ErrorMessage = "Please enter the Zip")]
    public string? Zip { get; set; }
    [Required(ErrorMessage = "Please enter the Phone No")]
    public string? mailingPhoneNo { get; set; }
    public AdminNavbarModel adminNavbarModel { get; set; }
}
