using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class OrderStatus
{
    public string OrderNo { get; set; } = null!;

    public string Motion { get; set; } = null!;

    public DateTime Update { get; set; }

    public string? EmpNo { get; set; }

    public virtual Emp? EmpNoNavigation { get; set; }

    public virtual OrderMaster OrderNoNavigation { get; set; } = null!;
}
