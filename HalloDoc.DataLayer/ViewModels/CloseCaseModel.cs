using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataLayer.ViewModels
{
    public class CloseCaseModel
    {
        public int reqId {  get; set; }
        public string conf_no { get; set; }
        public List<Models.RequestWiseFile> requestWiseFiles { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string fullName { get; set; }
        public DateOnly DOB { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public AdminNavbarModel? an { get; set; }

    }
}
