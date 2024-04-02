using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.DataLayer.ViewModels;

public class MdsOnCallViewModel
{
    public List<Physician>? providersOnCall;
    public List<Physician>? providersOffDuty;
    public List<Region>? allRegions;
    public AdminNavbarModel? adminNavbarModel;
}

