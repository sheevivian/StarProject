using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class OrderC
{
    public int No { get; set; }

    public string UserNo { get; set; } = null!;

    public string OrderNo { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Content { get; set; } = null!;

    public byte[]? Image { get; set; }

    public DateTime Date { get; set; }

    public string? Reply { get; set; }

    public DateTime? ReplyDate { get; set; }

    public string? EmpNo { get; set; }

    public string Status { get; set; } = null!;

    public virtual Emp? EmpNoNavigation { get; set; }

    public virtual OrderMaster OrderNoNavigation { get; set; } = null!;

    public virtual User UserNoNavigation { get; set; } = null!;
}
