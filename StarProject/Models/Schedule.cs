using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Schedule
{
    public int EventNo { get; set; }

    public DateTime ReleaseDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    public bool? Executed { get; set; }

    public virtual Event EventNoNavigation { get; set; } = null!;
}
