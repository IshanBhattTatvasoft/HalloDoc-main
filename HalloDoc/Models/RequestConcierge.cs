using System;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class RequestConcierge
{
    public int Id { get; set; }

    public int RequestId { get; set; }

    public int ConciergeId { get; set; }

    public string? Ip { get; set; }

    public virtual Concierge Concierge { get; set; } = null!;

    public virtual Request Request { get; set; } = null!;
}
