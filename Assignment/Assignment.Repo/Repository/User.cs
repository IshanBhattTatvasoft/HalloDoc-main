using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assignment.Models;
using Assignment.Models.ViewModels;
using HalloDoc;
using Assignment.Repo.Interface;
using Task = HalloDoc.Task;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace Assignment.Repo.Repository
{
    public class User : IUser
    {
        private readonly ApplicationDbContext _context;
        public User(ApplicationDbContext context)
        {
            _context = context;
        }

        public IndexViewModel GetAllTasks(int? page = 1, int? pageSize = 10, string? taskName = "")
        {
            IQueryable<Task> query = _context.Tasks.OrderBy(t => t.DueDate);

            if (taskName != null && taskName != "")
            {
                query = query.Where(q => q.TaskName.ToLower().Contains(taskName.ToLower()));
            }

            List<Task> allTasks = query.ToList();
            List<HomeViewModel> hvm1 = new List<HomeViewModel>();
            foreach (var item in allTasks)
            {
                HomeViewModel h = new HomeViewModel();
                h.singleTask = item;
                string name = _context.Categories.FirstOrDefault(c => c.Id == item.CategoryId).Name;
                h.categoryName = name.Replace("-", " ");
                hvm1.Add(h);
            }

            IndexViewModel ind = new IndexViewModel
            {
                hvm = hvm1.Skip((int)((page - 1) * pageSize)).Take((int)pageSize).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = hvm1.Count(),
                TotalPages = (int)Math.Ceiling((double)((double)hvm1.Count() / pageSize)),
            };

            return ind;
        }

        public bool DeleteTask(int id)
        {
            bool isDeleted = false;
            Task t = _context.Tasks.FirstOrDefault(ta => ta.Id == id);

            if (t != null)
            {
                _context.Tasks.Remove(t);
                _context.SaveChanges();
                isDeleted = true;
            }

            return isDeleted;
        }

        public bool AddNewTask(AddTaskViewModel model)
        {
            bool isAdded = false;
            string str = "";
            if (model.city == 1)
            {
                str = "Indore";
            }

            else if (model.city == 2)
            {
                str = "Ahmedabad";
            }

            else if (model.city == 3)
            {
                str = "Banglore";
            }

            else if (model.city == 4)
            {
                str = "Pune";
            }

            else if (model.city == 5)
            {
                str = "Jabalpur";
            }
            else if (model.city == 6)
            {
                str = "Hyderabad";
            }
            else
            {
                str = "Mumbai";
            }

            List<string> allCategories = new List<string>();
            foreach (var item in _context.Categories.ToList())
            {
                allCategories.Add(item.Name.Replace(" ", "").Replace("-", "").ToLower());
            }

            // Replace space and hypen with empty string and convert into lower case
            string str4 = model.category.Replace(" ", "").Replace("-", "").ToLower();

            // check if model.category is present in all the existing categories
            bool isExistingCategory = allCategories.Contains(str4);

            // if not present, add new record in Category table to store new category
            Category cat = new Category();
            if (!isExistingCategory)
            {
                cat.Name = model.category;
                _context.Categories.Add(cat);
                _context.SaveChanges();
            }

            if (str4 == "completed")
            {
                str4 = "Completed";
            }
            else if (str4 == "highpriority")
            {
                str4 = "High-Priority";
            }
            else if (str4 == "mediumpriority")
            {
                str4 = "Medium-Priority";
            }
            else if (str4 == "lowpriority")
            {
                str4 = "Low-Priority";
            }
            else if (str4 == "inprogress")
            {
                str4 = "In-Progress";
            }
            else if (str4 == "todo")
            {
                str4 = "To-Do";
            }
            else if (str4 == "upcoming")
            {
                str4 = "Upcoming";
            }
            else if(str4 == "pending")
            {
                str4 = "Pending";
            }

            int id2 = _context.Categories.FirstOrDefault(c => c.Name.Equals(str4)).Id;

            if (model.id == null)
            {
                Task t = new Task
                {
                    TaskName = model.taskName,
                    Assignee = model.asigneeName,
                    Description = model.description,
                    DueDate = model.dueDate,
                    City = str,
                    CategoryId = id2,
                };

                _context.Tasks.Add(t);
                _context.SaveChanges();
                isAdded = true;
            }

            else if (model.id != null)
            {
                Task t = _context.Tasks.FirstOrDefault(te => te.Id == model.taskId);
                t.TaskName = model.taskName;
                t.Assignee = model.asigneeName;
                t.Description = model.description;
                t.DueDate = model.dueDate;
                t.City = str;
                t.CategoryId = id2;

                _context.Tasks.Update(t);
                _context.SaveChanges();
                isAdded = true;
            }

            return isAdded;
        }

        public AddTaskViewModel GetTaskFromId(int id)
        {
            Task t = _context.Tasks.FirstOrDefault(ta => ta.Id == id);

            string categoryName = _context.Categories.FirstOrDefault(ca => ca.Id == t.CategoryId).Name;
            int city = -1;
            if (t.City == "Indore")
            {
                city = 1;
            }

            else if (t.City == "Ahmedabad")
            {
                city = 2;
            }

            else if (t.City == "Banglore")
            {
                city = 3;
            }

            else if (t.City == "Pune")
            {
                city = 4;
            }

            else if (t.City == "Jabalpur")
            {
                city = 5;
            }
            else if (t.City == "Hyderabad")
            {
                city = 6;
            }
            else
            {
                city = 7;
            }

            AddTaskViewModel atvm = new AddTaskViewModel
            {
                taskName = t.TaskName,
                asigneeName = t.Assignee,
                description = t.Description,
                dueDate = (DateTime)t.DueDate,
                city = city,
                category = categoryName,
                taskId = t.Id
            };

            return atvm;
        }
    }
}
