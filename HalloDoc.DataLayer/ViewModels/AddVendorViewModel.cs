using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;

public class AddVendorViewModel
{
    public int? vendorId { get; set; }
    public AdminNavbarModel? adminNavbarModel { get; set; }
    [Required(ErrorMessage = "Please enter the business name")]
    public string businessName { get; set; }
    public List<HealthProfessionalType>? professionType { get; set; }
    public List<Region>? allRegions { get; set; }
    [Required(ErrorMessage = "Please enter the profession type")]
    public int professionId { get; set; }
    [Required(ErrorMessage = "Please enter the fax number")]
    public string faxNumber { get; set; }
    [Required(ErrorMessage = "Please enter the phone number")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter valid phone number")]
    public string phoneNumber { get; set; }
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid Email")]
    public string email { get; set; }
    [Required(ErrorMessage = "Please enter the business contact")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter valid business contact")]
    public string businessContact { get; set; }
    [Required(ErrorMessage = "Please enter the street number")]
    public string street { get; set; }
    [Required(ErrorMessage = "Please enter the city name")]
    public string city { get; set; }
    [Required(ErrorMessage = "Please select the region")]
    public int regionId { get; set; }
    [Required(ErrorMessage = "Please enter the zipcode")]
    public string zipCode { get; set; }

}

