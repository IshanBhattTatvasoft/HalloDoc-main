using System;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class Region
{
    public int RegionId { get; set; }

    public string Name { get; set; } = null!;

    public string? Abbreviation { get; set; }

    public virtual ICollection<AdminRegion> AdminRegions { get; set; } = new List<AdminRegion>();

    public virtual ICollection<Business> Businesses { get; set; } = new List<Business>();

    public virtual ICollection<Concierge> Concierges { get; set; } = new List<Concierge>();

    public virtual ICollection<PhysicianRegion> PhysicianRegions { get; set; } = new List<PhysicianRegion>();

    public virtual ICollection<RequestClient> RequestClients { get; set; } = new List<RequestClient>();

    public virtual ICollection<ShiftDetailRegion> ShiftDetailRegions { get; set; } = new List<ShiftDetailRegion>();
}
