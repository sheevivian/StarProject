using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class AuditLog
{
    public long No { get; set; }

    public string EmpNo { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string TableName { get; set; } = null!;

    public int RecordId { get; set; }

    public string OldValue { get; set; } = null!;

    public string NewValue { get; set; } = null!;

    public DateTime ActionTime { get; set; }

    public virtual Emp EmpNoNavigation { get; set; } = null!;
}
