using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;

public class SendOrder
{
    public List<HealthProfessional> hp { get; set; }
    public List<HealthProfessionalType> hpType { get; set; }
    public int requestId { get; set; }
    public string? businessContact { get; set; }
    public string? email { get; set; }
    public string? faxNumber { get; set; }
    public string? prescription { get; set; }
    public int? numOfRefill { get; set; }
    public AdminNavbarModel? an { get; set; }
}

