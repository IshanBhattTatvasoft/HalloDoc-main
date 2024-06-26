﻿using HalloDoc.DataLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDocMvc.Entity.ViewModel
{
    public class ViewNotes
    {
        public string? AdminNotes { get; set; }
        public string? cancelledByAdminNotes { get; set; }
        public string? PhysicianNotes { get; set; }
        public string? cancelledByPatientNotes { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; }


        public string? PhyName { get; set; }

        public int RequestId { get; set; }
        public AdminNavbarModel? an { get; set; }



    }
}
