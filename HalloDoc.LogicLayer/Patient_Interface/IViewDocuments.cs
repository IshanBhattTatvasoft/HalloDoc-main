using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface IViewDocuments
    {
        public Request? GetRequestWithClient(int requestId);
        public List<RequestWiseFile>? ValidateFile(int requestid);

        public User ValidateUser(int user_id);
        public string UserFirstName(int user_id);
        public Request? GetRequestWithUser(int requestId);
        public void AddFile(RequestWiseFile requestWiseFile);



    }
}
