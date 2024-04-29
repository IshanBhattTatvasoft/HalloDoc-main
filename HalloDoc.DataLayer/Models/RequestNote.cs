using System;
using System.Collections.Generic;

namespace HalloDoc.DataLayer.Models;

public partial class RequestNote
{
    public int RequestNotesId { get; set; }

    public int RequestId { get; set; }

    public string? StrMonth { get; set; }

    public int? IntYear { get; set; }

    public int? IntDate { get; set; }

    public string? PhysicianNotes { get; set; }

    public string? AdminNotes { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Ip { get; set; }

    public string? AdministrativeNotes { get; set; }

    public virtual AspNetUser CreatedByNavigation { get; set; } = null!;

    public virtual AspNetUser? ModifiedByNavigation { get; set; }

    public virtual Request Request { get; set; } = null!;
}
