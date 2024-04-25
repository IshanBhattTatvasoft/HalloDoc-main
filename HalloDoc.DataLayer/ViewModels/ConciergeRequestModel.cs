using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace HalloDoc.DataLayer.ViewModels;


public class ConceirgeRequestModel
{
    public string? Symptoms { get; set; }
    [Required(ErrorMessage = "First name is required")]
    [RegularExpression(@"[a-zA-Z]{2,}$", ErrorMessage = "First Name should must contain letters only and should have atleast two letters")]
    public required string FirstName { get; set; }
    [Required(ErrorMessage = "Last name is required")]
    [RegularExpression(@"[a-zA-Z]{2,}$", ErrorMessage = "Last Name should must contain letters only and should have atleast two letters")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public DateOnly DOB { get; set; }
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Please enter valid Email")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter valid phone number")]
    public string? PhoneNumber { get; set; }
    //[Required(ErrorMessage = "Street number is required")]
    //public string? Street { get; set; }
    //[Required(ErrorMessage = "City name is required")]
    //public string? City { get; set; }
    //[Required(ErrorMessage = "State name is required")]
    //public string? State { get; set; }
    //[Required(ErrorMessage = "Zipcode is required")]
    //public string? Zipcode { get; set; }
    public string? Room { get; set; }
    [Required(ErrorMessage = "Concierge First Name is required")]
    public string? ConciergeFirstName { get; set; }
    [Required(ErrorMessage = "Concierge Last Name is required")]
    public string? ConciergeLastName { get; set; }
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid Email")]
    public string? ConciergeEmail { get; set; }
    [Required(ErrorMessage = "Concierge Phone Number is required")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter valid phone number")]
    public string? ConciergePhoneNumber { get; set; }
    [Required(ErrorMessage = "Concierge Property Name is required")]
    public string? ConciergePropertyName { get; set; }
    [Required(ErrorMessage = "Concierge Street is required")]
    public string? ConciergeStreet { get; set; }
    [Required(ErrorMessage = "Concierge City is required")]
    [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "Concierge City Name should must contain letters only")]
    public string? ConciergeCity { get; set; }
    [Required(ErrorMessage = "Concierge State is required")]
    [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "Concierge State Name should must contain letters only")]
    public string? ConciergeState { get; set; }
    [Required(ErrorMessage = "Concierge Zipcode is required")]
    public string? ConciergeZipcode { get; set; }
    public string? Password { get; set; }
    public bool? isPassword { get; set; } = false;
}

