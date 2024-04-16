using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace HalloDoc.DataLayer.ViewModels;

//checks whether we have provided input to the username and password fields
public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    public string UserName { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public string PasswordHash { get; set; }
    public string? email { get; set; }
    public double? lat { get; set; }
    public double? lon { get; set; }
}

