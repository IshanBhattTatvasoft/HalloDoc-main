using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = HalloDoc.Task;

namespace Assignment.Models.ViewModels;

public class AddTaskViewModel
{
    [Required(ErrorMessage = "Task Name is required")]
    public string taskName { get; set; }
    [Required(ErrorMessage = "Asignee Name is required")]
    public string asigneeName { get; set; }
    [Required(ErrorMessage = "Description is required")]
    public string description { get; set; }
    [Required(ErrorMessage = "Due Date is required")]
    public DateTime dueDate { get; set; }
    [Required(ErrorMessage = "City Name is required")]
    public int city { get; set; }
    [Required(ErrorMessage = "Category is required")]
    public string category { get; set; }
    public int? taskId { get; set; }
    public int? id { get; set; }
}

