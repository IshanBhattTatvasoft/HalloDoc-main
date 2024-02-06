using System;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class RequestType
{
    public int RequestTypeId { get; set; }

    public string Name { get; set; } = null!;
}
