using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.DataLayer.Models;

public partial class Physician
{
    public int PhysicianId { get; set; }

    public int? AspNetUserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string? Mobile { get; set; }

    public string? MedicalLicense { get; set; }

    public string? Photo { get; set; }

    public string? AdminNotes { get; set; }

    public BitArray? IsAgreementDoc { get; set; }

    public BitArray? IsBackgroundDoc { get; set; }

    public BitArray? IsTrainingDoc { get; set; }

    public BitArray? IsNonDisclosureDoc { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public int? RegionId { get; set; }

    public string? Zip { get; set; }

    public string? AltPhone { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public short? Status { get; set; }

    public string BusinessName { get; set; } = null!;

    public string BusinessWebsite { get; set; } = null!;

    public BitArray? IsDeleted { get; set; }

    public int? RoleId { get; set; }

    public string? Npinumber { get; set; }

    public BitArray? IsLicenseDoc { get; set; }

    public string? Signature { get; set; }

    public BitArray? IsCredentialDoc { get; set; }

    public BitArray? IsTokenGenerate { get; set; }

    public string? SyncEmailAddress { get; set; }

    public virtual AspNetUser? AspNetUser { get; set; }

    public virtual AspNetUser? CreatedByNavigation { get; set; }

    public virtual AspNetUser? ModifiedByNavigation { get; set; }

    public virtual ICollection<PhysicianNotification> PhysicianNotifications { get; set; } = new List<PhysicianNotification>();

    public virtual ICollection<PhysicianRegion> PhysicianRegions { get; set; } = new List<PhysicianRegion>();

    public virtual ICollection<RequestStatusLog> RequestStatusLogPhysicians { get; set; } = new List<RequestStatusLog>();

    public virtual ICollection<RequestStatusLog> RequestStatusLogTransToPhysicians { get; set; } = new List<RequestStatusLog>();

    public virtual ICollection<RequestWiseFile> RequestWiseFiles { get; set; } = new List<RequestWiseFile>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
