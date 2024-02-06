using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class Admin
{
    public int AdminId { get; set; }

    public string AspNetUserId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string? Mobile { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public int? RegionId { get; set; }

    public string? Zip { get; set; }

    public string? AltPhone { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public short? Status { get; set; }

    public BitArray? IsDeleted { get; set; }

    public int? RoleId { get; set; }

    public virtual ICollection<AdminRegion> AdminRegions { get; set; } = new List<AdminRegion>();

    public virtual AspNetUser AspNetUser { get; set; } = null!;

    public virtual AspNetUser? ModifiedByNavigation { get; set; }

    public virtual ICollection<RequestStatusLog> RequestStatusLogs { get; set; } = new List<RequestStatusLog>();

    public virtual ICollection<RequestWiseFile> RequestWiseFiles { get; set; } = new List<RequestWiseFile>();
}
