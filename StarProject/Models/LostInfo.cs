using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class LostInfo
{
    public int No { get; set; }

    public string Name { get; set; } = null!;

    public string Desc { get; set; } = null!;

    public string Image { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime FoundDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? OwnerName { get; set; }

    public string? OwnerPhone { get; set; }

    public string Category { get; set; } = null!;
}
