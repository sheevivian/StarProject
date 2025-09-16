namespace StarProject.DTOs
{
    public class PromotionListDto
    {
        public int No { get; set; }
        public string Name { get; set; }
        public string CouponCode { get; set; }
        public string Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public int? Limit { get; set; }
        public bool Reuse { get; set; }  // 新增
        public int? UsesTime { get; set; } // 新增

        // 計算屬性
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
            "Active" => "啟用中",
            "Inactive" => "停用",
            "NotStarted" => "未開始",
            "Expired" => "已過期",
            _ => ActualStatus
        };

        public string StatusClass => ActualStatus switch
        {
            "Active" => "success",
            "Inactive" => "warning",
            "NotStarted" => "info",
            "Expired" => "danger",
            _ => "secondary"
        };

        // 新增：使用限制的顯示文字
        public string UsageRestriction
        {
            get
            {
                var restrictions = new List<string>();

                if (Limit.HasValue)
                    restrictions.Add($"限{Limit}張");

                if (Reuse)
                    restrictions.Add("可重複");
                else
                    restrictions.Add("不可重複");

                if (UsesTime.HasValue)
                    restrictions.Add($"每人限{UsesTime}次");

                return restrictions.Any() ? string.Join("、", restrictions) : "無限制";
            }
        }
    }
}