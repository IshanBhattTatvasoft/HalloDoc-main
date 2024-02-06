using System;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class RequestBusiness
{
    public int RequestBusinessId { get; set; }

    public int RequestId { get; set; }

    public int BusinessId { get; set; }

    public string? Ip { get; set; }

    public virtual Business Business { get; set; } = null!;

    public virtual Request Request { get; set; } = null!;
}
