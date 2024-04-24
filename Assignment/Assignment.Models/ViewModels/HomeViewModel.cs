using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = HalloDoc.Task;

namespace Assignment.Models.ViewModels;

public class HomeViewModel
{
    public Task singleTask { get; set; }
    public string categoryName { get; set; }
}

