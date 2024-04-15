using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;

public class EditViewShiftModel
{
    public int ShiftDetailId { get; set; }
    public int PhysicianRegionVS { get; set; }
    public string? PhysicianRegionName { get; set; }
    public int PhysicianIdVS { get; set; }
    public string? PhysicianName { get; set; }
    public string ShiftDateVS { get; set; }
    public TimeOnly StartTimeVS { get; set; }
    public TimeOnly EndTimeVS { get; set; }
    public AdminNavbarModel? an { get; set; }
}
