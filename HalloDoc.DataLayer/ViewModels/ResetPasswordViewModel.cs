using System.ComponentModel.DataAnnotations;
namespace HalloDoc.DataLayer.ViewModels;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Enter Password")]
    [Compare("Password", ErrorMessage = "Password is Mismatch")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Confirm the entered password")]
    [Compare("Password", ErrorMessage = "Password is Mismatch")]
    public string? ConfirmPassword { get; set; }
}