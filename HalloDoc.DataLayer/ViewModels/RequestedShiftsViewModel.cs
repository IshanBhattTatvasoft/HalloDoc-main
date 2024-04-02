using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;

public class RequestedShiftsViewModel
{
    public AdminNavbarModel adminNavbarModel { get; set; }
    public List<Region>? allRegions { get; set; }
    public int? regionId { get; set; }
    public List<RequestedShiftsData> requestedShiftsTableData { get; set; }
}

