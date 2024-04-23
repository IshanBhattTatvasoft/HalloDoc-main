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
    [Required(ErrorMessage = "Please enter the Password")]
    public string Password { get; set; }
    public string status { get; set; }
    public int Role { get; set; }
    public int selectedRole { get; set; }

    [Required(ErrorMessage = "Please enter the First Name")]
    [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "First Name should must contain letters only")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Please enter the Last Name")]
    [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "Last Name should must contain letters only")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Please enter the Email")]
    [Compare("Email", ErrorMessage = "Email is Mismatch")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Enter ConfirmEmail")]
    [Compare("Email", ErrorMessage = "Email is Mismatch")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid Email")]
    public string? ConfirmEmail { get; set; }
    [Required(ErrorMessage = "Please enter the Phone Number")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter valid phone number")]
    public string? Phone { get; set; }
    public List<Region> regions { get; set; }
    public List<Region> selectedregions { get; set; }
    public List<Role> roles { get; set; }

    [Required(ErrorMessage = "First address is required")]
    public string? Address1 { get; set; }
    [Required(ErrorMessage = "Second address is required")]
    public string? Address2 { get; set; }
    [Required(ErrorMessage = "Please enter the City ")]
    [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "City Name should must contain letters only")]
    public string? City { get; set; }
    [Required(ErrorMessage = "Please enter the State ")]
    public string? State { get; set; }
    [Required(ErrorMessage = "Please enter the Zip")]
    public string? Zip { get; set; }
    [Required(ErrorMessage = "Please enter the Phone Number")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter valid phone number")]
    public string? MailingPhoneNo { get; set; }
    public string userId { get; set; }
    [Required(ErrorMessage = "Medical license number is required")]
    public string? MedicalLicense { get; set; }
    [Required(ErrorMessage = "NPI is required")]
    public string? NPI { get; set; }
    public string? SyncEmail { get; set; }
    public int PhysicianId { get; set; }
    [Required(ErrorMessage = "Please enter the Business Name")]
    public string BusinessName { get; set; }
    [Required(ErrorMessage = "Please enter the Business Website")]
    [RegularExpression(@"^((?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,6}$", ErrorMessage = "Please enter the valid Business Website")]
    public string BusinessWebsite { get; set; }
    [Required(ErrorMessage = "Please upload the Photo")]
    public IFormFile Photo { get; set; }
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
    [Required(ErrorMessage = "Please upload the agreement file")]
    public IFormFile? ContractAgreementFile { get; set; }
    [Required(ErrorMessage = "Please upload the Background Check file")]
    public IFormFile? BackgroundCheckFile { get; set; }
    [Required(ErrorMessage = "Please upload the HIPPA Compliance file")]
    public IFormFile? HippaFile { get; set; }
    [Required(ErrorMessage = "Please upload the Non-Disclosure Agreement file")]
    public IFormFile? NonDisclosureAgreement { get; set; }
    [Required(ErrorMessage = "Please select a role")]
    public int roleId { get; set; }
    [Required(ErrorMessage = "Please select a region")]
    public int? regionId { get; set; }
    public decimal? lati { get; set; }
    public decimal? longi { get; set; }
    public string? requestProfile { get; set; }
    public int? statusVal { get; set; }
    public string roleName { get; set; }
}

