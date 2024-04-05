using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;
public class VendorsTableData
{
    public string? profession { get; set; }
    public string? businessName { get; set; }
    public string? email { get; set; }
    public string? faxNumber { get; set; }
    public string? phoneNumber { get; set; }
    public string? businessContact { get; set; }
    public int? vendorId { get; set; }
}

