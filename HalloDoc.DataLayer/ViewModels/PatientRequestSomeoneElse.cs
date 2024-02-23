using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;

public class PatientRequestSomeoneElse
{
    public string Symptoms { get; set; }
    [Required(ErrorMessage = "First Name is required")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Last Name is required")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Date of Birth is required")]
    public DateTime DOB { get; set; }
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Phone Number is required")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Street is required")]
    public string Street { get; set; }
    [Required(ErrorMessage = "City is required")]
    public string City { get; set; }
    [Required(ErrorMessage = "State is required")]
    public string State { get; set; }
    [Required(ErrorMessage = "Zip Code is required")]
    public string ZipCode { get; set; }
    public string Room { get; set; }
    public string Relation { get; set; }
    public IFormFile File { get; set; }
}
