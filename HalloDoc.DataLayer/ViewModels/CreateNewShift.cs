using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;

public class CreateNewShift
{
    public List<Region>? Region { get; set; }
    public int PhysicianRegion { get; set; }
    public int PhysicianId { get; set; }
    public string ShiftDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsRepeat { get; set; }
    public List<int>? RepeatDays { get; set; }
    public int RepeatUpto { get; set; }
}
