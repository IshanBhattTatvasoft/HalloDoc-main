using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace HalloDoc.DataLayer.ViewModels;

//checks whether we have provided input to the username and password fields
public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid Email")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string PasswordHash { get; set; }
    }

