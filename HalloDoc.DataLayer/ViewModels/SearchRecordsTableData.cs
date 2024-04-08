using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;


public class SearchRecordsTableData
{
    public string? patientName { get; set; }
    public int? requestor { get; set; }
    public DateTime? dateOfService { get; set; }
    public DateTime? closeCaseDate { get; set; }
    public string? email { get; set; }
    public string? phoneNumber { get; set; }
    public string? address { get; set; }
    public string? zipcode { get; set; }
    public int? requestStatus { get; set; }
    public string? physician { get; set; }
    public string? physicianNote { get; set; }
    public string? cancelledByProviderNote { get; set; }
    public string? adminNote { get; set; }
    public string? patientNote { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
}

