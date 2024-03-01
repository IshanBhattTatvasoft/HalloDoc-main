﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;

public class ViewCaseModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DOB { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Region { get; set; }
    public string BusinessAddress { get; set; }
    public string Room { get; set; }
    public string ConfirmationNo { get; set; }
    public string Requestor { get; set; }
    public string Status { get; set; }
    public string PatientNotes { get; set; }
    public int RequestId { get; set; }
}