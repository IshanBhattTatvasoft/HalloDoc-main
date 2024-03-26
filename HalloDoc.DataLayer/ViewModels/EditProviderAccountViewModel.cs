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

public class EditProviderAccountViewModel
{
    public string? UserName { get; set; }
    public List<Role> allRoles { get; set; }
    [Required(ErrorMessage = "Please enter the  Password")]
    public string Password { get; set; }
    public string status { get; set; }
    public int Role { get; set; }
    public int selectedRole { get; set; }

    [Required(ErrorMessage = "Please enter the  First name")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Please enter the  last name")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Enter Email")]
    [Compare("Email", ErrorMessage = "Email is Mismatch")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Enter ConfirmEmail")]
    [Compare("Email", ErrorMessage = "Email is Mismatch")]
    public string? ConfirmEmail { get; set; }
    [Required(ErrorMessage = "Please enter the Phone No ")]
    public string? Phone { get; set; }
    public List<Region> regions { get; set; }
    public List<Region> selectedregions { get; set; }
    public List<Role> roles { get; set; }

    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    [Required(ErrorMessage = "Please enter the City ")]
    public string? City { get; set; }
    [Required(ErrorMessage = "Please enter the State ")]
    public string? State { get; set; }
    [Required(ErrorMessage = "Please enter the Zip")]
    public string? Zip { get; set; }
    [Required(ErrorMessage = "Please enter the Phone No")]
    public string? MailingPhoneNo { get; set; }
    public string userId { get; set; }
    public string? MedicalLicense { get; set; }
    public string? NPI { get; set; }
    public string? SyncEmail { get; set; }
    public int PhysicianId { get; set; }
    public string? BusinessName { get; set; }
    public string? BusinessWebsite { get; set; }
    public IFormFile? Photo { get; set; }
    public IFormFile? Signature { get; set; }
    public string? AdminNotes { get; set; }
    public string? SignatureName { get; set; }
    public bool? Contract { get; set; }
    public string Contractname { get; set; }
    public bool? BackgroundCheck { get; set; }
    public string? BackgroundCheckName { get; set; }
    public bool? Compilance { get; set; }
    public string? CompilanceName { get; set; }
    public bool? NonDisclosure { get; set; }
    public string? NonDisclosureName { get; set; }
    public bool? LicensedDoc { get; set; }
    public bool? LicensedDocName { get; set; }
    public AdminNavbarModel adminNavbarModel { get; set; }
    public IFormFile? ContractAgreementFile { get; set; }
    public IFormFile? BackgroundCheckFile { get; set; }
    public IFormFile? HippaFile { get; set; }
    public IFormFile? NonDisclosureAgreement { get; set; }
    public int roleId { get; set; }
    public int? regionId { get; set; }
}

