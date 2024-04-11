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
public class EmailLogsTableData
{
    public string? recipientName {  get; set; }
    public string? action { get; set; }
    public int? roleId { get; set; }
    public string? emailId { get; set; }
    public DateTime? createdDate { get; set; }
    public DateTime sentDate { get; set;}
    public string? isSent { get; set; }
    public int? sentTries { get; set; }
    public string? confirmationNo { get; set; }
    public int? emailLogId { get; set; }

}

