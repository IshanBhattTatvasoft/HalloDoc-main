using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class RequestWiseFile
{
    public int RequestWiseFileId { get; set; }

    public int RequestId { get; set; }

    public string FileName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int? PhysicianId { get; set; }

    public int? AdminId { get; set; }

    public short? DocType { get; set; }

    public BitArray? IsFrontSide { get; set; }

    public BitArray? IsCompensation { get; set; }

    public string? Ip { get; set; }

    public BitArray? IsFinalize { get; set; }

    public BitArray? IsDeleted { get; set; }

    public BitArray? IsPatientRecords { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual Physician? Physician { get; set; }

    public virtual Request Request { get; set; } = null!;
}
