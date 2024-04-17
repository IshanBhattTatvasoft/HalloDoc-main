using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;


public class DashboardViewModel
{
    public string? name { get; set; }
    
    public List<TableContent>? requests { get; set; }
    public string? Username { get; set; }
    public string? ConfirmationNumber { get; set; }
    public AspNetUser? User { get; set; }
    public AdminNavbarModel? an { get; set; }
    public User UserModel { get; set; }
    public IEnumerable<Request> Requests { get; set; }
    public List<Physician> phy { get; set; }
    public List<RequestFileViewModel> RequestsAndFiles { get; set; }
}

