using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class LeaveType
{
    public int No { get; set; }

    public string TypeName { get; set; } = null!;

    public string TypeCode { get; set; } = null!;

    public virtual ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();
}
