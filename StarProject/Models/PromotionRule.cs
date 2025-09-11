using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class PromotionRule
{
    public int PromotionNo { get; set; }

    public string? Rule { get; set; }

    public string? Action { get; set; }

    public string? Scope { get; set; }

    public virtual Promotion PromotionNoNavigation { get; set; } = null!;
}
