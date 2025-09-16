using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class AttendanceRecord
{
    public int No { get; set; }

    public string EmpNo { get; set; } = null!;

    public DateOnly ClockDate { get; set; }

    public DateTime? ClockInTime { get; set; }

    public DateTime? ClockOutTime { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Emp EmpNoNavigation { get; set; } = null!;
}
