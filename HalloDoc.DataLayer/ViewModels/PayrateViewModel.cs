using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;
public class PayrateViewModel
{
    public AdminNavbarModel? adminNavbarModel { get; set; }
    public int? physicianId { get; set; }
    public int? nightShiftWeekend { get; set; }
    public int? shift { get; set; }
    public int? houseCallNightsWeekend { get; set; }
    public int? phoneConsults { get; set; }
    public int? phoneConsultsNightWeekend { get; set; }
    public int? batchTesting { get; set; }
    public int? houseCalls { get; set; }
    public bool? isHavingEntry { get; set; }
}

