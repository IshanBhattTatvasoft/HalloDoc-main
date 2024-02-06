using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class RequestStatusLog
{
    public int RequestStatusLogId { get; set; }

    public int RequestId { get; set; }

    public short Status { get; set; }

    public int? PhysicianId { get; set; }

    public int? AdminId { get; set; }

    public int? TransToPhysicianId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? Ip { get; set; }

    public BitArray? TransToAdmin { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual Physician? Physician { get; set; }

    public virtual Request Request { get; set; } = null!;

    public virtual ICollection<RequestClosed> RequestCloseds { get; set; } = new List<RequestClosed>();

    public virtual Physician? TransToPhysician { get; set; }
}
