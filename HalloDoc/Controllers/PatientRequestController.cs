using HalloDoc.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HalloDoc.Controllers
{
    public class PatientRequestController : Controller
    {
        private readonly ILogger<PatientRequestController> _logger;

        public PatientRequestController(ILogger<PatientRequestController> logger)
        {
            _logger = logger;
        }

        public IActionResult CreatePatientRequest()
        {
            return View();
        }

        public IActionResult CreateFamilyFriendRequest()
        {
            return View();
        }

        public IActionResult CreateBusinessRequest()
        {
            return View();
        }

        public IActionResult CreateConciergeRequest()
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