using HalloDoc.DataLayer.Models;
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
        public AdminDashboardTableView ModelOfAdminDashboard(string status, int id, string? search, string? requestor, int? region);
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
        public DataLayer.Models.Region ValidateRegion(AdminCreateRequestModel model);
        public BlockRequest ValidateBlockRequest(AdminCreateRequestModel model);
        public AspNetUser ValidateAspNetUser(AdminCreateRequestModel model);
        public void InsertDataOfRequest(AdminCreateRequestModel model);
        public bool VerifyLocation(string state);
        public AspNetUser ValidateAspNetUser(LoginViewModel model);
        public Admin ValidateUser(LoginViewModel model);
        public User ValidateUserByRequestId(Request r);
        public List<RequestWiseFile> GetFileData(int requestid);
        public Request GetRequestWithUser(int requestid);
        public void AddFile(RequestWiseFile requestWiseFile);
        public AspNetUser ValidAspNetUser(string email);
        public List<HealthProfessional> getBusinessData(int professionId);
        public PasswordReset ValidateToken(string token);
        public AspNetUser ValidateUserForResetPassword(ResetPasswordViewModel model, string useremail);
        public void SetPasswordForResetPassword(AspNetUser user, ResetPasswordViewModel model);
        public List<Request> GetRequestDataInList();
        public int SingleDelete(int id);
        public List<DataLayer.Models.Region> GetAllRegion();
        public List<CaseTag> GetAllCaseTags();
        public Request GetReqFromReqType(int ReqId);
        public Request GetReqFromModel(AdminDashboardTableView model);
        public void MultipleDelete(int requestid, string fileId);
        public List<HealthProfessionalType> GetHealthProfessionalType();
        public List<HealthProfessional> GetHealthProfessional();
        public List<HealthProfessional> GetBusinessDataFromProfession(int professionId);
        public HealthProfessional GetOtherDataFromBId(int businessId);
        public void AddOrderDetails(OrderDetail orderDetail);
        public RequestClient GetPatientData(int id);
        public string GetMailToSentAgreement(int reqId);
        public RequestClient GetRequestClientFromId(int id);
        public Request GetReqFromReqClient(int id);
        public RequestStatusLog GetLogFromReqId(int reqId);
        public void AddRequestStatusLogFromAgreement(RequestStatusLog rsl);
        //AdminDashboardTableView ModelOfAdminDashboard(string? status, int userId);
        public EncounterForm GetEncounterFormData(int reqId);
        public void UpdateEncounterFormData(EncounterFormModel model, RequestClient rc);
        public void AddRequestClosedData(RequestClosed rc);
        public void UpdateRequestClient(RequestClient rc);
        public List<HalloDoc.DataLayer.Models.Region> GetAllRegions();
        public Admin GetAdminFromId(int id);
        public AspNetUser GetAdminDataFromId(int id);
        public HalloDoc.DataLayer.Models.Region GetRegFromId(int id);
        public AspNetUser GetAspNetFromAdminId(int id);
        public void AdminResetPassword(AspNetUser anur, string pass);
        public void UpdateAdminDataFromId(AdminProfile model, int id, string selectedRegion);
        public List<AdminRegion> GetAdminRegionFromId(int id);
        public List<AdminRegion> GetAvailableRegionOfAdmin(int id);
        public void UpdateMailingInfo(AdminProfile model, int aid);


    }
}
