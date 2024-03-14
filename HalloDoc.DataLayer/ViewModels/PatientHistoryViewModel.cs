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
public class PatientHistoryViewModel
{
    public AdminNavbarModel? AdminNavbarModel { get; set; }
    public List<Request> requests { get; set; } = new List<Request>();
}

