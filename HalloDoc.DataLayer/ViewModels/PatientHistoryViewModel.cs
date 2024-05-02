using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;
public class PatientHistoryViewModel
{
    public AdminNavbarModel? AdminNavbarModel { get; set; }
    public List<Request> requests { get; set; } = new List<Request>();
    public List<Physician> p { get; set;} = new List<Physician>();
    public List<RequestWiseFile> Rwf { get; set; } = new List<RequestWiseFile>();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int? userId { get; set; }
}

