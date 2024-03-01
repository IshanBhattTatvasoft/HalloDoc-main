﻿using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDocMvc.Entity.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface IAdminInterface 
    {
        public AdminDashboardTableView ModelOfAdminDashboard(string? status);
        public Request ValidateRequest(int requestId);
        public RequestClient ValidateRequestClient(int requestClientId);
        public void EditViewCaseAction(ViewCaseModel userProfile, RequestClient userToUpdate);
        public RequestNote FetchRequestNote(int requestId);
        public RequestStatusLog FetchRequestStatusLogs(int requestId);
        public Physician FetchPhysician(int id);
        public void EditViewNotesAction(RequestNote rn, ViewNotes model);
        public CaseTag FetchCaseTag(int caseTagId);
        public void AddRequestStatusLogFromCancelCase(RequestStatusLog rs);
        public List<Physician> FetchPhysicianByRegion(int RegionId);
        public void AddBlockRequestData(BlockRequest br);
        public void UpdateRequest(Request r);




    }
}