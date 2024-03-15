using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using HalloDoc.DataLayer.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;
public class AdminDashboardTableView
{
    public int new_count { get; set; }
    public int pending_count { get; set; }
    public int active_count { get; set; }
    public int conclude_count { get; set; }
    public int toclose_count { get; set; }
    public int unpaid_count { get; set; }
    //public AdminNavbarViewModel? adminNavbarViewModel { get; set; }
    public List<Request> requests { get; set; } = new List<Request>();
    public IQueryable<Request> query_requests { get; set; }
    public List<Region> regions { get; set; } = new List<Region>();
    public string status { get; set; }
    public string PatientName { get; set; }
    public string? Reason { get; set; }
    public string? AdditionalNotes { get; set; }
    public int RequestId { get; set; }
    public List<CaseTag> caseTags { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Email is required")]
    public required string email { get; set; }
    public int? requestTypeId { get; set; }
    public string sendAgreeEmail { get; set; }
    public AdminNavbarModel? an { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
