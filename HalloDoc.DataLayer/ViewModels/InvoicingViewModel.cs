using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;
public class InvoicingViewModel
{
    public AdminNavbarModel? adminNavbarModel { get; set; }
    public string? fullDate { get; set; }
    public List<KeyValuePair<string, int>> dateAndOnCallHour { get; set; }
    public List<KeyValuePair<string, string>> dateAndFileName { get; set; }
    public List<int>? totalHours { get; set; }
    public List<int>? numberOfHouseCalls { get; set; }
    public List<bool>? holidays { get; set; }
    public List<int>? numberOfPhoneConsult { get; set; }
    public List<string>? items { get; set; }
    public List<int>? amounts { get; set; }
    public List<IFormFile>? files { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
}

