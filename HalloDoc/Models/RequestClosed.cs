using System;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class RequestClosed
{
    public int RequestClosedId { get; set; }

    public int RequestId { get; set; }

    public int RequestStatusLogId { get; set; }

    public string? PhyNotes { get; set; }

    public string? ClientNotes { get; set; }

    public string? Ip { get; set; }

    public virtual Request Request { get; set; } = null!;

    public virtual RequestStatusLog RequestStatusLog { get; set; } = null!;
}
