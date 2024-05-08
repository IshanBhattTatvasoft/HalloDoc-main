using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;
public class AdminInvoicingViewModel
{
    public AdminNavbarModel? adminNavbarModel { get; set; }
    public List<ReimbursementViewModel> rvm { get; set; }
    public bool? isTimesheetExists { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
    public int? physicianId { get; set; }
    public int? timesheetId { get; set; }
    public string? physicianName { get; set; }
    public string? status { get; set; }
    public bool? isApproved { get; set; }
}

