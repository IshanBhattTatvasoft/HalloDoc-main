using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HalloDoc.Models;

public class ViewDocumentModel
{
    public string patient_name { get; set; }
    public string uploader_name { get; set; }
    public string Username { get; set; }
    public string name { get; set; }
    public string confirmation_number { get; set; }
    public List<RequestWiseFile> requestWiseFiles { get; set; }
    public IFormFile? ImageContent { get; set; }
    public int requestId { get; set; }

}