﻿using System.ComponentModel.DataAnnotations;
namespace HalloDoc.Models;

public class ConceirgeRequestModel
{
    public string Symptoms { get; set; }
    [Required(ErrorMessage = "First name is required")]
    public required string FirstName { get; set; }
    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public DateOnly DOB { get; set; }
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Street number is required")]
    public string Street { get; set; }
    [Required(ErrorMessage = "City name is required")]
    public string City { get; set; }
    [Required(ErrorMessage = "State name is required")]
    public string State { get; set; }
    [Required(ErrorMessage = "Zipcode is required")]
    public string Zipcode { get; set; }
    public string Room { get; set; }
    [Required(ErrorMessage = "Concierge First Name is required")]
    public string ConciergeFirstName { get; set; }
    [Required(ErrorMessage = "Concierge Last Name is required")]
    public string ConciergeLastName { get; set; }
    [Required(ErrorMessage = "Concierge Email is required")]
    public string ConciergeEmail { get; set; }
    [Required(ErrorMessage = "Concierge Phone Number is required")]
    public string ConciergePhoneNumber { get; set; }
    [Required(ErrorMessage = "Concierge Property Name is required")]
    public string ConciergePropertyName { get; set; }
    [Required(ErrorMessage = "Concierge Street is required")]
    public string ConciergeStreet { get; set; }
    [Required(ErrorMessage = "Concierge City is required")]
    public string ConciergeCity { get; set; }
    [Required(ErrorMessage = "Concierge State is required")]
    public string ConciergeState { get; set; }
    [Required(ErrorMessage = "Concierge Zipcode is required")]
    public string ConciergeZipcode { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
    public string File { get; set; }
    public bool isPassword { get; set; } = false;
}

