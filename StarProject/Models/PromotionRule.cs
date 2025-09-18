using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarProject.Models;

// ✅ 修改原因：移除 Table 屬性（改在 DbContext 配置），簡化 Model
public partial class PromotionRule
{
    // ✅ 修改原因：只保留 Key 屬性，移除 Required（主鍵自動是必填）
    [Key]
    public int Promotion_No { get; set; }

    public string? RuleType { get; set; }
    public decimal? DiscountValue { get; set; }
    public string? ConditionType { get; set; }
    public int? ConditionAmount { get; set; }
    public string? MemberLevel { get; set; }
    public string? TargetCategory { get; set; }
    public string? Description { get; set; }

    // ✅ 修改原因：移除 ForeignKey 屬性（改在 DbContext 配置）
    public virtual Promotion PromotionNoNavigation { get; set; } = null!;
}