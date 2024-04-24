using Assignment.Models;
using Assignment.Models.ViewModels;
using Assignment.Repo.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Assignment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUser _user;
        public HomeController(ILogger<HomeController> logger, IUser user)
        {
            _logger = logger;
            _user = user;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IndexPartialView(int? page = 1, int? pageSize = 10, string? taskName = "")
        {
            IndexViewModel hm = _user.GetAllTasks(page, pageSize, taskName);
            return PartialView("IndexPagePartialView", hm);
        }

        public IActionResult DeleteTask(int id)
        {
            if(_user.DeleteTask(id))
            {
                TempData["success"] = "Task deleted successfully!!";
            }
            else
            {
                TempData["error"] = "Unable to delete the task!!";
            }
            return RedirectToAction("Index");
        }

        public IActionResult AddTask()
        {
            return PartialView("~/Views/Shared/_AddTaskModal.cshtml");
        }

        public IActionResult EditTask(int id)
        {
            AddTaskViewModel atvm = _user.GetTaskFromId(id);
            return PartialView("~/Views/Shared/_AddTaskModal.cshtml", atvm);
        }

        public IActionResult AddTaskSubmitAction(AddTaskViewModel model)
        {
            if (_user.AddNewTask(model) && model.taskId == null)
            {
                TempData["success"] = "New task added successfully";
            }
            else if (model.taskId != null)
            {
                TempData["success"] = "Task edited successfully";
            }
            else
            {
                TempData["error"] = "Unable to add new task";
            }
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}