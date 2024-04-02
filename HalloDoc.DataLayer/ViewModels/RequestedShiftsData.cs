using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;

public class RequestedShiftsData
{
    public string? physicianName { get; set; }
    public string? day { get; set; }
    public string? time { get; set; }
    public string? regionName { get; set; }
    public int? shiftDetailId { get; set; }
    public int? status { get; set; }
    public bool? isDeleted { get; set; }

}

