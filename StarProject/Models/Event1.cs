using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Event1
{
    public int No { get; set; }

    public string Title { get; set; } = null!;

    public string? Desc { get; set; }

    public string Category { get; set; } = null!;

    public string Location { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime UpdatedTime { get; set; }

    public int MaxParticipants { get; set; }

    public string Status { get; set; } = null!;

    public int? Fee { get; set; }

    public int? Deposit { get; set; }

    public string Image { get; set; } = null!;
}
