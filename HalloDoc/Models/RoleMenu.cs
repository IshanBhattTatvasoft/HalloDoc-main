using System;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class RoleMenu
{
    public int RoleMenuId { get; set; }

    public int RoleId { get; set; }

    public int MenuId { get; set; }

    public virtual Menu Menu { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
