using System;
using System.Collections.Generic;

namespace HalloDoc.DataLayer.Models;

public partial class PasswordReset
{
    public string Token { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public bool IsModified { get; set; }

    public virtual AspNetUser EmailNavigation { get; set; } = null!;
}
