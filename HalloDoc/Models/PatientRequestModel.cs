using System.ComponentModel.DataAnnotations;
namespace HalloDoc.Models;

public class PatientRequestModel
{
    public string Symptoms { get; set; }
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateOnly DOB { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zipcode { get; set; }
    public string Room { get; set; }

    public string Password { get; set; }
    public string File { get; set; }
    public bool isPassword { get; set; } = false;

    public string ConciergeFirstName { get; set; }
    public string ConciergeLastName { get; set; }
    public string ConciergeEmail { get; set; }
    public string ConciergePhoneNumber { get; set; }
    public string ConciergePropertyName { get; set; }
    public string ConciergeStreet { get; set; }
    public string ConciergeCity { get; set; }
    public string ConciergeState { get; set; }
    public string ConciergeZipcode { get; set; }


    public string BusinessFirstName { get; set; }
    public string BusinessLastName { get; set; }
    public string BusinessPhoneNumber { get; set; }
    public string BusinessEmail { get; set; }
    public string BusinessPropertyName { get; set; }
    public string BusinessCaseNumber { get; set; }


    public string FamilyFirstName { get; set; }
    public string FamilyLastName { get; set;}
    public string FamilyPhoneNumber { get; set;}
    public string FamilyEmail { get; set;}
    public string FamilyRelation { get; set;}


    //public string ConciergeFirstName { get; set; }
}
