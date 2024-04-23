using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;

public class AdminCreateRequestModel
{
    public string? Symptoms { get; set; }
    [Required(ErrorMessage = "First name is required")]
    public required string FirstName { get; set; }
    [Required(ErrorMessage = "Last name is required")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public required DateOnly DOB { get; set; }
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid Email")]
    public required string Email { get; set; }
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter valid phone number")]
    public required string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Street number is required")]
    public required string Street { get; set; }
    [Required(ErrorMessage = "City name is required")]
    public required string City { get; set; }
    [Required(ErrorMessage = "State name is required")]
    public required string State { get; set; }
    public string? Zipcode { get; set; }
    public string? AdminRequestZipCode { get; set; }
    public string? Room { get; set; }
    [Compare("Password", ErrorMessage = "Password is Mismatch")]
    public string? Password { get; set; }
    [Compare("Password", ErrorMessage = "Password is Mismatch")]
    public string? ConfirmPassword { get; set; }
    public string? File { get; set; }
    public bool isPassword { get; set; } = false;

    public IFormFile? ImageContent { get; set; }

    public string? AdminNotes { get; set; }

    public AdminNavbarModel? adminNavbarModel {  get; set; }
    //public string ConciergeFirstName { get; set; }
}
