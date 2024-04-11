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
public class SmsLogsTableData
{
    public string? recipient { get; set; }
    public int? action { get; set; }
    public int? roleId { get; set; }
    public string? roleName { get; set; }
    public string? phoneNumber { get; set; }
    public DateTime? createdDate { get; set; }
    public DateTime sentDate { get; set; }
    public bool? isSent { get; set; }
    public string? sent { get; set; }
    public int? sentTries { get; set; }
    public string? confirmationNo { get; set; }
    public int? smsLogId { get; set; }
}

