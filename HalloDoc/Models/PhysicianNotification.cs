using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class PhysicianNotification
{
    public int Id { get; set; }

    public int PhysicianId { get; set; }

    public BitArray IsNotificationStopped { get; set; } = null!;

    public virtual Physician Physician { get; set; } = null!;
}
