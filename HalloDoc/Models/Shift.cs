using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class Shift
{
    public int ShiftId { get; set; }

    public int PhysicianId { get; set; }

    public DateOnly StartDate { get; set; }

    public BitArray IsRepeat { get; set; } = null!;

    public string? WeekDays { get; set; }

    public int? RepeatUpto { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? Ip { get; set; }

    public virtual AspNetUser CreatedByNavigation { get; set; } = null!;

    public virtual Physician Physician { get; set; } = null!;

    public virtual ICollection<ShiftDetail> ShiftDetails { get; set; } = new List<ShiftDetail>();
}
