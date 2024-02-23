using System.ComponentModel.DataAnnotations;
namespace HalloDoc.DataLayer.ViewModels;

public class BusinessRequestModel
{
    public string? Symptoms { get; set; }
    [Required(ErrorMessage = "First name is required")]
    public required string FirstName { get; set; }
    [Required(ErrorMessage = "Last name is required")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public DateOnly DOB { get; set; }
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Please enter valid Email")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Phone number is required")]
    public string? PhoneNumber { get; set; }
    [Required(ErrorMessage = "Street number is required")]
    public string? Street { get; set; }
    [Required(ErrorMessage = "City name is required")]
    public string? City { get; set; }
    [Required(ErrorMessage = "State name is required")]
    public string? State { get; set; }
    [Required(ErrorMessage = "Zipcode is required")]
    public string? Zipcode { get; set; }
    public string? Room { get; set; }
    [Required(ErrorMessage = "Business First Name is required")]
    public string? BusinessFirstName { get; set; }
    [Required(ErrorMessage = "Business Last Name is required")]
    public string? BusinessLastName { get; set; }
    [Required(ErrorMessage = "Business Phone Number is required")]
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

