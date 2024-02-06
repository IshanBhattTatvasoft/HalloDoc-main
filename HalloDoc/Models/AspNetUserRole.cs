using System;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class AspNetUserRole
{
    public string UserId { get; set; } = null!;

    public string RoleId { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
