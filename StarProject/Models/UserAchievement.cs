using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class UserAchievement
{
    public int No { get; set; }

    public string UserNo { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int AchievedLevel { get; set; }

    public DateTime AchievedDate { get; set; }

    public virtual User UserNoNavigation { get; set; } = null!;
}
