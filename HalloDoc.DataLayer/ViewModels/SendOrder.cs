using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;

public class SendOrder
{
    public List<HealthProfessional> hp { get; set; }
    public List<HealthProfessionalType> hpType { get; set; }
    public int requestId { get; set; }
    [Required(ErrorMessage = "Please enter the health professional type")]
    public int? healthProfessionalType { get; set; }
    [Required(ErrorMessage = "Please enter the Business Contact Number")]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Please enter valid Business Contact Number")]
    public string businessContact { get; set; }
    [Required(ErrorMessage = "Please enter the Email")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid Email")]
    public string email { get; set; }
    public string? faxNumber { get; set; }
    public string? prescription { get; set; }
    [Required(ErrorMessage = "Please enter the number of refills required")]
    public int numOfRefill { get; set; }
    public AdminNavbarModel? an { get; set; }
}

