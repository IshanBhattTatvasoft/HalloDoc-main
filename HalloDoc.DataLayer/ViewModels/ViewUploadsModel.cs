using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataLayer.ViewModels
{
    public class ViewUploadsModel
    {
        public string confirmation_number { get; set; }
        public List<Models.RequestWiseFile> requestWiseFiles { get; set; }
        public IFormFile? ImageContent { get; set; }
        public int requestId { get; set; }
        public User user { get; set; }

    }
}
