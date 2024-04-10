﻿using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;
using HalloDocMvc.Entity.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface IAdminInterface 
    {
        //public AdminDashboardTableView ModelOfAdminDashboard(int page = 1, int pageSize = 10, string status, int id, string? search, string? requestor, int? region)
        public AdminDashboardTableView ModelOfAdminDashboard(string status, int id, string? search, string? requestor, int? region, int page = 1, int pageSize = 10);
        public PatientHistoryViewModel PatientHistoryFilteredData(AdminNavbarModel an, string fname, string lname, string pno, string email, int page = 1, int pageSize = 10);
        public PatientHistoryViewModel PatientRecordsData(int userid, AdminNavbarModel an, int page = 1, int pageSize = 10);
        public ProviderMenuViewModel ProviderMenuFilteredData(AdminNavbarModel an, int? region, int page = 1, int pageSize = 10);
        public PatientHistoryViewModel PatientRecordsFilteredData(int userid, AdminNavbarModel an, int page = 1, int pageSize = 10);
        public SearchRecordsViewModel SearchRecordsFilteredData(AdminNavbarModel an, int? page = 1, int? pageSize = 10, int? requestStatus = -1, string? patientName = "", int? requestType = -1, DateTime? fromDate = null, DateTime? toDate = null, string? providerName = "", string? email = "", string? phoneNo = null);
        public SmsLogsViewModel SmsLogsFilteredData(AdminNavbarModel an, int page = 1, int pageSize = 10, int? role = 0, string? recipientName = "", string? phoneNumber = "", DateTime? createdDate = null, DateTime? sentDate = null);
        public Request ValidateRequest(int requestId);
        public RequestClient ValidateRequestClient(int requestClientId);
        public void EditViewCaseAction(ViewCaseModel userProfile, RequestClient userToUpdate);
        public RequestNote FetchRequestNote(int requestId);
        public RequestStatusLog FetchRequestStatusLogs(int requestId);
        public Physician FetchPhysician(int id);
        public void EditViewNotesAction(ViewNotes model);
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
        public Admin ValidateUser(string email);
        public User ValidateUserByRequestId(Request r);
        public List<RequestWiseFile> GetFileData(int requestid);
        public Request GetRequestWithUser(int requestid);
        public void AddFile(RequestWiseFile requestWiseFile);
        public AspNetUser ValidAspNetUser(string email);
        public bool FindAdminFromAspNetUser(int id);
        public Admin GetAdminFromAspNetUser(string email)
        public Physician GetPhysicianFromAspNetUser(string email)
        public bool FindPhysicianFromAspNetUser(int id);
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
        public List<Request> GetPatientRecordsData(int userId);
        public List<Physician> GetAllPhysicians();
        public List<RequestWiseFile> GetAllFiles();
        public RequestClient ValidatePatientEmail(string email);
        public List<Menu> GetAllMenus();
        public void CreateNewRole2(string name, string acType, string adminName, List<int> menuIds);
        public List<Role> GetAllRoles();
        public void DeleteRoleFromId(int roleId);
        public string GetNameFromRoleId(int id);
        public int GetAccountTypeFromId(int id);
        public List<RoleMenu> GetAllRoleMenu(int id);
        public void EditRoleSubmitAction(int roleid, List<int> menuIds);
        public EditProviderAccountViewModel ProviderEditAccount(int id, AdminNavbarModel an);
        public void SavePasswordOfPhysician(EditProviderAccountViewModel model);
        public void EditProviderBillingInfo(EditProviderAccountViewModel model);
        public void SaveProviderProfile(EditProviderAccountViewModel model, string selectedRegionsList);
        public void SetContentOfPhysician(IFormFile file, int id, bool IsSignature);
        public void SetAllDocOfPhysician(IFormFile file, int id, int num);
        public void PhysicianProfileUpdate(EditProviderAccountViewModel model);
        public void ChangeNotificationValue(int id);
        public void DeletePhysicianAccount(int id);
        public void CreateNewProviderAccount(EditProviderAccountViewModel model, List<int> regionNames, int userId);
        public List<RequestStatusLog> GetAllRslData(int requestId);
        public List<Role> GetSpecifiedAdminRoles();
        public List<Role> GetSpecifiedProviderRoles();
        public void CreateNewAdminAccount(EditProviderAccountViewModel model, List<int> regionNames, int userId);
        public UserAccessViewModel UserAccessFilteredData(AdminNavbarModel an, int accountType);
        public List<string> GetAllMenus(string roleId);
        public List<BlockedHistoryData> GetBlockedHistoryData();
        public void UnblockRequest(int id);
        public List<ShiftDetail> GetScheduleData(int RegionId);
        public List<SchedulingViewModel> GetProviderInformation(int Region);
        public bool CreateNewShift(SchedulingViewModel model, List<int> RepeatedDays, int id);
        public EditViewShiftModel GetViewShift(int ShiftDetailId);
        public bool ReturnViewShift(int ShiftDetailId);
        public bool EditViewShift(EditViewShiftModel Shift);
        public bool DeleteViewShift(int ShiftDetailId);
        public BlockedHistoryViewModel BlockedHistoryFilteredData(AdminNavbarModel an, string name, DateOnly date, string email, string phoneNo);
        public List<RequestedShiftsData> GetRequestedShiftsData(int? regionId = -1);
        public void ApproveSelectedShifts(string shiftDetailIdString);
        public void DeleteSelectedShifts(string shiftDetailIdString);
        public MdsOnCallViewModel GetMdsData(AdminNavbarModel an);
        public string GetPhysicianNameFromId(int id, int shiftId);
        public List<PhysicianLocation> GetPhysicianLocation();
        public VendorsViewModel VendorsFilteredData(AdminNavbarModel an, string? name = "", int? professionalId = -1, int page = 1, int pageSize = 10);
        public bool AddNewVendor(AddVendorViewModel model);
        public AddVendorViewModel GetVendorDataFromId(int id, AdminNavbarModel an);
        public bool SaveEditedBusinessInfo(AddVendorViewModel model, int id);
        public bool DeleteBusinessProfile(int id);
        public bool DeleteSearchRecord(int id);
        public void AddSmsLogFromSendLink(string body, string number, int? adminId, DateTime temp, int count, bool isSMSSent);
        public void AddSmsLogFromSendOrder(string body, string number, int? adminId, DateTime temp, int count, bool isSMSSent);
        public void AddSmsLogFromContactProvider(string body, string number, int? adminId, int phyId, DateTime temp, int count, bool isSMSSent);
        public void AddEmailLog(string body, string subject, string email, int RoleId, string? filePath, string? ConfirmationNumber, int? RequestId, int? AdminId, int? PhysicianId, DateTime? createdDate, bool isEmailSent, int emailSentCount);

    }
}
