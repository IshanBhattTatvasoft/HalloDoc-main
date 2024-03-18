using HalloDoc.DataLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataLayer.ViewModels
{
    public class Enums
    {
        public enum Status
        {
            Unassigned = 1, Accepted = 2, Cancelled = 3, MDEnRoute = 4, MDONSite = 5, Conclude = 6, CancelledByPatient = 7, Closed = 8, Unpaid = 9, Clear = 10, Blocked = 11
        }
    }
}
