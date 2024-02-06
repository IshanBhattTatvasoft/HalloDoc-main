using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class HealthProfessionalType
{
    public int HealthProfessionalId { get; set; }

    public string ProfessionName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public BitArray? IsActive { get; set; }

    public BitArray? IsDeleted { get; set; }

    public virtual ICollection<HealthProfessional> HealthProfessionals { get; set; } = new List<HealthProfessional>();
}
