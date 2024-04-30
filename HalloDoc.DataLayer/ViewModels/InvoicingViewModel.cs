using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;
public class InvoicingViewModel
{
    public AdminNavbarModel? adminNavbarModel { get; set; }
    public string? fullDate { get; set; }
}

