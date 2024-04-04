﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.DataLayer.ViewModels
{
    public class SearchRecordsTableData
    {
        public string? patientName {  get; set; }
        public string? requestor { get; set; }
        public DateOnly? dateOfService { get; set; }
        public DateOnly? closeCaseDate { get; set; }
        public string? email { get; set; }
        public string? phoneNumber { get; set; }
        public string? address { get; set; }
        public string? zipcode { get; set; }
        public string? requestStatus { get; set; }
        public string? physician { get; set; }
        public string? physicianState { get; set; }
        public string? cancelledByProviderNote { get; set; }
        public string? adminNote { get; set; }
        public string? patientNote { get; set; }
    }
}