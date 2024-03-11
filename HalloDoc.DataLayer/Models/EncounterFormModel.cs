using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;
public class EncounterFormModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Location { get; set; }
    public DateOnly DOB { get; set; }
    public DateOnly Date { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string HistoryOfIllness { get; set; }
    public string MedicalHistory { get; set; }
    public string Medications { get; set; }
    public string Allergies { get; set; }
    public decimal Temp { get; set; }
    public decimal HR { get; set; }
    public decimal RR { get; set; }
    public int BPS { get; set; }
    public int BPD { get; set; }
    public decimal O2 { get; set; }
    public string Pain { get; set; }
    public string Heent { get; set; }
    public string CV { get; set; }
    public string Chest { get; set; }
    public string ABD { get; set; }
    public string Extr { get; set; }
    public string Skin { get; set; }
    public string Neuro { get; set; }
    public string Other { get; set; }
    public string Diagnosis { get; set; }
    public string TreatmentPlan { get; set; }
    public string MedicationsDispensed { get; set; }
    public string Procedures { get; set; }
    public string FollowUp { get; set; }
    public int reqId { get; set; }
}

