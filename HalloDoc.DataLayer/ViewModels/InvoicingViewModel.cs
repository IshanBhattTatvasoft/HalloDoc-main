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
    public List<ReimbursementViewModel> rvm { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
    public int? timesheetId { get; set; }
    public bool? isFinalized { get; set; }
    public bool? isExisting { get; set; }
}

