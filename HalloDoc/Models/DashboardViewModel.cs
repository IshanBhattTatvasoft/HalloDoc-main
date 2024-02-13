using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.Models;

public class DashboardViewModel
{
    public string name { get; set; }
    
    public List<TableContent> requests { get; set; }

}

