using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class EventNotif
{
    public int EventNo { get; set; }

    public string Category { get; set; } = null!;

    public int ParticipantNo { get; set; }

    public DateTime Senttime { get; set; }

    public string Status { get; set; } = null!;

    public virtual Event EventNoNavigation { get; set; } = null!;
}
