using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.ViewModels;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface IPatientRequest
    {
        public void InsertDataPatientRequest(PatientRequestModel model);
        public Region ValidateRegion(PatientRequestModel model);
        public BlockRequest ValidateBlockRequest(PatientRequestModel model);
        public AspNetUser ValidateAspNetUser(PatientRequestModel model);

    }
}
