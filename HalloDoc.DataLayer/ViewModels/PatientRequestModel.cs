using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Office.Interop.Excel;

namespace HalloDoc.DataLayer.ViewModels;


// Email regex meaning:
//^: This character denotes the start of the string.
//[a-zA-Z0-9._%+-]+: This is the pattern for the email username, which can consist of:
//One or more alphanumeric characters(represented by the character range a-z, A-Z, and 0-9).
//Special characters such as ., _, %, +, and - (_ because it's a special character in regex, should be escaped with a backslash).
//@: The required at symbol, which separates the username and domain.
//[a - zA - Z0 - 9.-]+: This is the pattern for the domain name, which can consist of:
//One or more alphanumeric characters(represented by the character range a-z, A-Z, and 0-9).
//Special characters such as . and -.
//\.: Escaped period(\.), representing the domain extension.It must be escaped because . in regex is a special character that matches any character.
//[a - zA - Z]{ 2,}$: This is the pattern for the domain extension, which must consist of:
//Two or more alphabetic characters(represented by the character range a-z and A-Z).
//$: This character denotes the end of the string.

public class PatientRequestModel
{
    public string? Symptoms { get; set; }
    [Required(ErrorMessage = "First name is required")]
    [RegularExpression(@"[a-zA-Z]{2,}$", ErrorMessage = "First Name should must contain letters only and should have atleast two letters")]
    public required string FirstName { get; set; }
    [Required(ErrorMessage = "Last name is required")]
    [RegularExpression(@"[a-zA-Z]{2,}$", ErrorMessage = "Last Name should must contain letters only and should have atleast two letters")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public DateOnly DOB { get; set; }
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid Email")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter valid phone number")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Street number is required")]
    public string Street { get; set; }
    [Required(ErrorMessage = "City name is required")]
    [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "City Name should must contain letters only")]
    public string City { get; set; }
    [Required(ErrorMessage = "State name is required")]
    [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "State Name should must contain letters only")]
    public string State { get; set; }
    [Required(ErrorMessage = "Zipcode is required")]
    public string Zipcode { get; set; }
    public string? AdminRequestZipCode { get; set; }
    public string? Room { get; set; }
    [Required(ErrorMessage = "Enter Password")]
    [Compare("Password", ErrorMessage = "Password is Mismatch")]
    public string? Password { get; set; }
    [Required(ErrorMessage = "Enter Password")]
    [Compare("Password", ErrorMessage = "Password is Mismatch")]
    public string? ConfirmPassword { get; set; }
    public string? File { get; set; }
    public bool isPassword { get; set; } = false;

    public IFormFile? ImageContent { get; set; }
    public AdminNavbarModel? an { get; set; }
    public string? AdminNotes { get; set; }


    //public string ConciergeFirstName { get; set; }
}


