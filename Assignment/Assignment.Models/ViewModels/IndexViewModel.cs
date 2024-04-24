using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment.Models.ViewModels;

public class IndexViewModel
{
    public List<HomeViewModel> hvm { get; set; }
    public int? CurrentPage { get; set; }
    public int? PageSize { get; set; }
    public int? TotalItems { get; set; }
    public int? TotalPages { get; set; }
}

