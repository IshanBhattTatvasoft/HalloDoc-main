using System;
using System.Collections.Generic;

namespace HalloDoc.DataLayer.Models;

public partial class Family
{
    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public string? Mobile { get; set; }

    public string? Email { get; set; }

    public string? Relation { get; set; }

    public int Familyid { get; set; }

    public DateTime? Createddate { get; set; }

    public virtual ICollection<RequestFamily> RequestFamilies { get; set; } = new List<RequestFamily>();
}
