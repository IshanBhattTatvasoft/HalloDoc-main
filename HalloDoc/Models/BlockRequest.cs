using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class BlockRequest
{
    public int BlockRequestId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public BitArray? IsActive { get; set; }

    public string? Reason { get; set; }

    public string RequestId { get; set; } = null!;

    public string? Ip { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
