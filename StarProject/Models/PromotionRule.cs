using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarProject.Models;

public partial class PromotionRule
{
    [Key]  // 🔴 重要：加入主鍵標記
    [ForeignKey("PromotionNoNavigation")]  // 🔴 指定外鍵關聯
    public int Promotion_No { get; set; }

    public string RuleType { get; set; } = null!;

    public int? ConditionAmount { get; set; }

    public decimal? DiscountValue { get; set; }

    public string TargetCategory { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? ConditionType { get; set; }

    public string? MemberLevel { get; set; }

    public virtual Promotion PromotionNoNavigation { get; set; } = null!;
}