using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;

public class ProviderMenuViewModel
{
    public List<Physician> physician { get; set; } = new List<Physician>();
    //public List<PhysicianNotification> ?physicianNotifications { get; set; } = new List<PhysicianNotification>();
    public List<Region> regions { get; set; } = new List<Region>();
    public List<Role>? roles { get; set; }
    public AdminNavbarModel? an { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int? phyId { get; set; }
    public string? messageType { get; set; }
    public string? email { get; set; }
    public string? phoneNumber { get; set; }
    public string? sendMessage { get; set; }
}

