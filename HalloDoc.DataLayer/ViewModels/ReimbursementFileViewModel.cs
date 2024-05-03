using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;
public class ReimbursementViewModel
{
    public KeyValuePair<string, int> dateAndOnCallHour { get; set; }
    public KeyValuePair<string, string> dateAndFileName { get; set; }
    public int? totalHours { get; set; }
    public int? numberOfHouseCalls { get; set; }
    public int? numberOfPhoneConsult { get; set; }
    public bool isWeekend { get; set; }
    public string? items { get; set; }
    public int? amounts { get; set; }
    public IFormFile? file { get; set; }
    public bool? isHavingFile { get; set; }
    public string? phyId { get; set; }
    public bool? isFileDeleted { get; set; }
    public int? id { get; set; }
}

