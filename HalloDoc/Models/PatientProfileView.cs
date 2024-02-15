namespace HalloDoc.Models;

public class PatientProfileView
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DOB { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}