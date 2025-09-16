using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class LeaveApplication
{
    public int No { get; set; }

    public string EmpNo { get; set; } = null!;

    public int LeaveTypeId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal TotalDays { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime AppliedAt { get; set; }

    public virtual Emp EmpNoNavigation { get; set; } = null!;

    public virtual LeaveType LeaveType { get; set; } = null!;
}
