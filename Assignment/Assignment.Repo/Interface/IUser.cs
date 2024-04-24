using Assignment.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment.Repo.Interface
{
    public interface IUser
    {
        public IndexViewModel GetAllTasks(int? page = 1, int? pageSize = 10, string? taskName = "");
        public bool DeleteTask(int id);
        public bool AddNewTask(AddTaskViewModel model);
        public AddTaskViewModel GetTaskFromId(int id);
    }
}
