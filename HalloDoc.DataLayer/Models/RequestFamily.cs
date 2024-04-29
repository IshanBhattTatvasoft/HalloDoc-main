using System;
using System.Collections.Generic;

namespace HalloDoc.DataLayer.Models;

public partial class RequestFamily
{
    public int Id { get; set; }

    public int? Requestid { get; set; }

    public int? Familyid { get; set; }

    public virtual Family? Family { get; set; }

    public virtual Request? Request { get; set; }
}
