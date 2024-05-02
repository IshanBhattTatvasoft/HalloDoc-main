using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;
public class BlockedHistoryData
{
    public string PhoneNumber { get; set; }
    public string? PatientName { get; set; }
    public string Email { get; set; }
    public DateOnly CreatedDate { get; set; }
    public string Notes { get; set; }
    public bool IsActive { get; set; }
    public int RequestId { get; set; }
    public int BlockRequestId { get; set; }
}

