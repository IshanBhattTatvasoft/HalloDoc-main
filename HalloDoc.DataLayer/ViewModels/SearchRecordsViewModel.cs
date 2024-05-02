using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;

public class SearchRecordsViewModel
{
    public List<SearchRecordsTableData> tableData { get; set; }
    public List<SearchRecordsTableData> allDataForExcel { get; set; }
    public AdminNavbarModel? adminNavbarModel {  get; set; }
    public int? requestStatus { get; set; }
    public string? patientName { get; set; }
    public int? requestType { get; set; }
    public DateTime? fromDate { get; set; }
    public DateTime? toDate { get; set; }
    public string? providerName { get; set; }
    public string? email { get; set; }
    public string? phoneNumber { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

