using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.DataLayer.Models;

public partial class Timesheet
{
    public int TimesheetId { get; set; }

    public int PhysicianId { get; set; }

    public DateTime? Startdate { get; set; }

    public DateTime? Enddate { get; set; }

    public string? Status { get; set; }

    public BitArray? IsFinalized { get; set; }

    public virtual Physician Physician { get; set; } = null!;

    public virtual ICollection<TimesheetDetail> TimesheetDetails { get; set; } = new List<TimesheetDetail>();

    public virtual ICollection<TimesheetReimbursement> TimesheetReimbursements { get; set; } = new List<TimesheetReimbursement>();
}
