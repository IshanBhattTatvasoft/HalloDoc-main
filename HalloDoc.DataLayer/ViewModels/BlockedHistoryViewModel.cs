using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;
public class BlockedHistoryViewModel
{
    public List<BlockedHistoryData> allData {  get; set; }
    public AdminNavbarModel adminNavbarModel { get; set; }
    public int? CurrentPage { get; set; }
    public int? PageSize { get; set; }
    public int? TotalItems { get; set; }
    public int? TotalPages { get; set; }
}

