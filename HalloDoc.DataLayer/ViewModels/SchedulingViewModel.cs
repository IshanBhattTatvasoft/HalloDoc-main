using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;

public class SchedulingViewModel
{
    public List<Region>? allRegions {  get; set; }
    public List<Physician>? physicianByRegion { get; set; }
    [Required(ErrorMessage = "Please select a region")]
    public int regionId { get; set; }
    public int? shiftId { get; set; }
    [Required(ErrorMessage = "Please select start date of the schedule")]
    public DateTime? startDate { get; set; }
    [Required(ErrorMessage = "Please select the start time")]
    public TimeOnly? startTime { get; set; }
    [Required(ErrorMessage = "Please select the end time")]
    public TimeOnly? endTime { get; set; }
    [Required(ErrorMessage = "Please choose whether shift will repeat or not")]
    public int repeat { get; set; }
    public AdminNavbarModel adminNavbarModel { get; set; }
    public string? repeatCount { get; set; }
    [Required(ErrorMessage = "Please select a phyisican")]
    public int physicianId { get; set; }
}

