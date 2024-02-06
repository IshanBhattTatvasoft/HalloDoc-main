using System;
using System.Collections;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class Smslog
{
    public decimal SmslogId { get; set; }

    public string Smstemplate { get; set; } = null!;

    public string MobileNumber { get; set; } = null!;

    public string? ConfirmationNumber { get; set; }

    public int? RoleId { get; set; }

    public int? AdminId { get; set; }

    public int? RequestId { get; set; }

    public int? PhysicianId { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime? SentDate { get; set; }

    public BitArray? IsSmssent { get; set; }

    public int SentTries { get; set; }

    public int? Action { get; set; }
}
