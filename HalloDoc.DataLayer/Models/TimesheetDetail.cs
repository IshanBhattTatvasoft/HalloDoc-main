using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.DataLayer.Models;

public partial class TimesheetDetail
{
    public int TimesheetDetailId { get; set; }

    public int TimesheetId { get; set; }

    public DateTime? Shiftdate { get; set; }

    public int? ShiftHours { get; set; }

    public int? Housecall { get; set; }

    public int? PhoneConsult { get; set; }

    public BitArray? IsWeekend { get; set; }

    public virtual Timesheet Timesheet { get; set; } = null!;
}
