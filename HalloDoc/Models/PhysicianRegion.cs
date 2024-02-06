using System;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class PhysicianRegion
{
    public int PhysicianRegionId { get; set; }

    public int PhysicianId { get; set; }

    public int RegionId { get; set; }

    public virtual Physician Physician { get; set; } = null!;

    public virtual Region Region { get; set; } = null!;
}
