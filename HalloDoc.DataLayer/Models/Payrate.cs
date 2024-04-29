using System;
using System.Collections.Generic;

namespace HalloDoc.DataLayer.Models;

public partial class Payrate
{
    public int PhysicianId { get; set; }

    public int PayrateId { get; set; }

    public decimal? NightShiftWeekend { get; set; }

    public decimal? Shift { get; set; }

    public decimal? HousecallNightWeekend { get; set; }

    public decimal? Phoneconsult { get; set; }

    public decimal? PhoneconsultNightWeekend { get; set; }

    public decimal? BatchTesting { get; set; }

    public decimal? Housecall { get; set; }

    public virtual Physician Physician { get; set; } = null!;
}
