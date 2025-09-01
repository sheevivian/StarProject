using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Participant
{
    public int No { get; set; }

    public int EventNo { get; set; }

    public string UsersNo { get; set; } = null!;

    public DateTime RegisterdDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Status { get; set; } = null!;

    public int PaymentNo { get; set; }

    public virtual Event EventNoNavigation { get; set; } = null!;

    public virtual PaymentTransaction PaymentNoNavigation { get; set; } = null!;

    public virtual User UsersNoNavigation { get; set; } = null!;
}
