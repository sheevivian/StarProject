using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Participant
{
    public int No { get; set; }

    public int EventNo { get; set; }

    public string UsersNo { get; set; } = null!;

    public DateTime RegisteredDate { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Status { get; set; } = null!;

    public string? PaymentNo { get; set; }

    public string? Code { get; set; }

    public virtual User UsersNoNavigation { get; set; } = null!;
}
