using System;
using System.Collections.Generic;

namespace HalloDoc.DataLayer.Models;

public partial class TimesheetReimbursement
{
    public int TimesheetReimbursementId { get; set; }

    public int TimesheetId { get; set; }

    public string Item { get; set; } = null!;

    public int Amount { get; set; }

    public string? Bill { get; set; }

    public DateTime Date { get; set; }

    public virtual Timesheet Timesheet { get; set; } = null!;
}
