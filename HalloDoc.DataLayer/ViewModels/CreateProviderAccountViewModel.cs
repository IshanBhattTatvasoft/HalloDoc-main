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

public class CreateProviderAccountViewModel
{
    [Required(ErrorMessage = "First Name is required")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Last Name is required")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Phone Number is required")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "License Number is required")]
    public string LicenseNo { get; set; }
    [Required(ErrorMessage = "NPI Number is required")]
    public string NpiNumber { get; set; }
    [Required(ErrorMessage = "Email address is required")]
    public string SyncEmailAddr { get; set; }
    [Required(ErrorMessage = "First address is required")]
    public string Address1 { get; set; }
    [Required(ErrorMessage = "Second address is required")]
    public string Address2 { get; set; }
    [Required(ErrorMessage = "City is required")]
    public string City { get; set; }
    [Required(ErrorMessage = "State is required")]
    public string State { get; set; }
    [Required(ErrorMessage = "Zipcode is required")]
    public string ZipCode { get; set; }
    [Required(ErrorMessage = "Alternate Number is required")]
    public string AlternateNum { get; set; }
    [Required(ErrorMessage = "Business Name is required")]
    public string BusinessName { get; set; }
    [Required(ErrorMessage = "Business Website is required")]
    public string BusinessWebsite { get; set; }
    [Required(ErrorMessage = "Photo is required")]
    public IFormFile Photo { get; set; }
    [Required(ErrorMessage = "Signature is required")]
    public IFormFile Signature { get; set; }
    public string AdminNotes { get; set; }
    [Required(ErrorMessage = "Contractor Agreement is required")]
    public IFormFile ICA { get; set; }
    [Required(ErrorMessage = "Background Check Document is required")]
    public IFormFile BackgroundCheck { get; set; }
    [Required(ErrorMessage = "HIPAA Compliance is required")]
    public IFormFile HIPAA { get; set; }
    [Required(ErrorMessage = "Non-disclosure agreement is required")]
    public IFormFile NonDisclosureAgreement { get; set; }
    [Required(ErrorMessage = "License Document is required")]
    public IFormFile LicenseDocument { get; set; }
    public AdminNavbarModel adminNavbarModel { get; set; }
}

