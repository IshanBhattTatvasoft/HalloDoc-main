using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class HealthProfessional
{
    public int VendorId { get; set; }

    public string VendorName { get; set; } = null!;

    public int? Profession { get; set; }

    public string FaxNumber { get; set; } = null!;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public int? RegionId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? PhoneNumber { get; set; }

    public BitArray? IsDeleted { get; set; }

    public string? Ip { get; set; }

    public string? Email { get; set; }

    public string? BusinessContact { get; set; }

    public virtual HealthProfessionalType? ProfessionNavigation { get; set; }
}
