using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class PromotionRule
{
    public int No { get; set; }

    public int PromotionNo { get; set; }

    public string RuleType { get; set; } = null!;

    public int? ConditionAmount { get; set; }

    public decimal? DiscountValue { get; set; }

    public string TargetCategory { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? ConditionType { get; set; }

    public string? MemberLevel { get; set; }
}
