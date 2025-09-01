using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class UserSecurity
{
    public string UserNo { get; set; } = null!;

    public string TwoFaenabled { get; set; } = null!;

    public string TwoFasecret { get; set; } = null!;

    public int LastPasswordChange { get; set; }

    public virtual User UserNoNavigation { get; set; } = null!;
}
