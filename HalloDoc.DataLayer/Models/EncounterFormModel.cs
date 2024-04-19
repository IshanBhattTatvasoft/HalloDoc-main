using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;
public class EncounterFormModel
{
    //[Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "Name must not be longer than 100 characters", MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name must contain letters only")]
    public string? FirstName { get; set; }

    //[Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Name must not be longer than 100 characters", MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name must contain letters only")]
    public string? LastName { get; set; }
    //[Required(ErrorMessage = "Location is required")]
    public string? Location { get; set; }
    public DateTime? DOB { get; set; }
    public DateTime? Date { get; set; }
    //[Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^[1-9][0-9]{9}$", ErrorMessage = "Please enter a valid 10-digit phone number")]
    public string? PhoneNumber { get; set; }
    //[Required(ErrorMessage = "Email is required")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid Email")]
    public string? Email { get; set; }
    public string? HistoryOfIllness { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Medications { get; set; }
    [Required(ErrorMessage = "Please mention the allergies")]
    public string Allergies { get; set; }
    public decimal? Temp { get; set; }
    public decimal? HR { get; set; }
    public decimal? RR { get; set; }
    public int? BPS { get; set; }
    public int? BPD { get; set; }
    public decimal? O2 { get; set; }
    public string? Pain { get; set; }
    public string? Heent { get; set; }
    public string? CV { get; set; }
    public string? Chest { get; set; }
    public string? ABD { get; set; }
    public string? Extr { get; set; }
    public string? Skin { get; set; }
    public string? Neuro { get; set; }
    public string? Other { get; set; }
    public string? Diagnosis { get; set; }
    [Required(ErrorMessage = "Please mention about treatment plan")]
    public string TreatmentPlan { get; set; }
    public string? MedicationsDispensed { get; set; }
    [Required(ErrorMessage = "Please mention about the procedures")]
    public string Procedures { get; set; }
    [Required(ErrorMessage = "Please mention about the followup")]
    public string FollowUp { get; set; }
    public int? reqId { get; set; }
    public AdminNavbarModel? an { get; set; }
}

