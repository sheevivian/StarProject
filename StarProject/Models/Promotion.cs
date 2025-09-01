using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Promotion
{
    public int No { get; set; }

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = null!;

    public int Limit { get; set; }

    public string CouponCode { get; set; } = null!;

    public bool Reuse { get; set; }

    public string? UsesTime { get; set; }
}
