using System.ComponentModel.DataAnnotations;
namespace HalloDoc.DataLayer.ViewModels;

public class BusinessRequestModel
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
    [Required(ErrorMessage = "Street number is required")]
    public string? Street { get; set; }
    [Required(ErrorMessage = "City name is required")]
    [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "City Name should must contain letters only")]
    public string? City { get; set; }
    [Required(ErrorMessage = "State name is required")]
    [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "State Name should must contain letters only")]
    public string? State { get; set; }
    [Required(ErrorMessage = "Zipcode is required")]
    public string? Zipcode { get; set; }
    public string? Room { get; set; }
    [Required(ErrorMessage = "Business First Name is required")]
    [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "Business First Name should must contain letters only")]
    public string? BusinessFirstName { get; set; }
    [Required(ErrorMessage = "Business Last Name is required")]
    [RegularExpression(@"[a-zA-Z]{1,}$", ErrorMessage = "Business Last Name should must contain letters only")]
    public string? BusinessLastName { get; set; }
    [Required(ErrorMessage = "Business Phone Number is required")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter valid phone number")]
    public string? BusinessPhoneNumber { get; set; }
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Please enter valid Email")]
    public string? BusinessEmail { get; set; }
    [Required(ErrorMessage = "Business Property Name is required")]
    public string? BusinessPropertyName { get; set; }
    public string? BusinessCaseNumber { get; set; }
    public string? Password { get; set; }
    public string? File { get; set; }
    public bool isPassword { get; set; } = false;
}

