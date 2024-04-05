using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;
public class VendorsViewModel
{
    public List<HealthProfessional> vendorsTableData { get; set; }
    public AdminNavbarModel adminNavbarModel { get; set; }
    public HealthProfessionalType professionType { get; set; }
    public int? professionId { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

