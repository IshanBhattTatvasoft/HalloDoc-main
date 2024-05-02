using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;

public class UserAccessViewModel
{
    public List<Admin>? admins {  get; set; }
    public int? accountType { get; set; }
    public List<Request>? requests { get; set; }
    public List<Physician>? physicians { get; set; }
    public AdminNavbarModel? adminNavbarModel { get; set; }
}

