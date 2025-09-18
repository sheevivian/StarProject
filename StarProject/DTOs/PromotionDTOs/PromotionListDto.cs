using System.Linq.Expressions;
using StarProject.Models;

namespace StarProject.DTOs.PromotionDTOs
{
    public class PromotionListDto
    {
        public int No { get; set; }
        public string Name { get; set; } = "";
        public string CouponCode { get; set; } = "";
        public string Category { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "";
        public int? Limit { get; set; }
        public bool Reuse { get; set; }
        public int? UsesTime { get; set; }

        // ★ 新增：EF 可翻譯的投影（只投影欄位，不含計算屬性）
        public static Expression<Func<Promotion, PromotionListDto>> Projection => p => new PromotionListDto
        {
            No = p.No,
            Name = p.Name,
            CouponCode = p.CouponCode,
            Category = p.Category,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Status = p.Status,
            Limit = p.Limit,
            Reuse = p.Reuse,
            UsesTime = p.UsesTime
        };

        public string ActualStatus
        {
            get
            {
                if (EndDate < DateTime.Now) return "Expired";
                if (StartDate > DateTime.Now) return "NotStarted";
                return Status;
            }
        }

        public string CategoryDisplay => Category switch
        {
            "ALL" => "全館",
            "Product" => "商品",
            "Activity" => "活動",
            "Ticket" => "票券",
            _ => Category
        };

        public string StatusDisplay => ActualStatus switch
        {
            "Active" => "status-active",
            "Inactive" => "停用",
            "NotStarted" => "未開始",
            "Expired" => "已過期",
            _ =>""
        };

        public string StatusClass => ActualStatus switch
        {
            "Active" => "success",
            "Inactive" => "warning",
            "NotStarted" => "info",
            "Expired" => "danger",
            _ => "secondary"
        };

        // ★ 新增：對應你 CSS 的類別（status-badge status-active 等）
        public string UiStatusClass => ActualStatus switch
        {
            "Active" => "status-active",
            "Inactive" => "status-inactive",
            "NotStarted" => "status-upcoming",
            "Expired" => "status-expired",
            _ => ""
        };

        public string UsageRestriction
        {
            get
            {
                var restrictions = new List<string>();
                if (Limit.HasValue) restrictions.Add($"限{Limit}張");
                restrictions.Add(Reuse ? "可重複" : "不可重複");
                if (UsesTime.HasValue) restrictions.Add($"每人限{UsesTime}次");
                return restrictions.Any() ? string.Join("、", restrictions) : "無限制";
            }
        }
    }
}
