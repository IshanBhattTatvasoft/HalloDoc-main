using System.ComponentModel.DataAnnotations;
namespace HalloDoc.DataLayer.ViewModels;

public class CreatePatientAccountViewModel
{
    [Required(ErrorMessage = "Email is required")]
    public string email {  get; set; }
    [Required(ErrorMessage = "Enter Password")]
    [Compare("Password", ErrorMessage = "Password is Mismatch")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm the entered password")]
    [Compare("Password", ErrorMessage = "Password is Mismatch")]
    public string ConfirmPassword { get; set; }
}