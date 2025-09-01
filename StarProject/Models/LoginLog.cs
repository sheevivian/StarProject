using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class LoginLog
{
    public long No { get; set; }

    public string UserNo { get; set; } = null!;

    public DateTime LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    public string Ipaddress { get; set; } = null!;

    public string? DeviceInfo { get; set; }

    public bool Status { get; set; }

    public virtual User UserNoNavigation { get; set; } = null!;
}
