using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using StarProject.Models;

namespace StarProject.ViewModels
{
    public class PromotionFormViewModel
    {
        [HiddenInput]
        public int No { get; set; }

        [Display(Name = "優惠名稱")]
        [Required(ErrorMessage = "優惠名稱為必填欄位")]
        public string Name { get; set; } = null!;

        [Display(Name = "優惠代碼")]
        [Required(ErrorMessage = "優惠代碼為必填欄位")]
        public string CouponCode { get; set; } = null!;

        [Display(Name = "適用類別")]
        [Required(ErrorMessage = "適用類別為必選")]
        public string Category { get; set; } = null!;

        public List<SelectListItem> CategoryOptions { get; set; } = new()
        {
            new SelectListItem { Value = "ALL", Text = "全館" },
            new SelectListItem { Value = "Product", Text = "商品" },
            new SelectListItem { Value = "Activity", Text = "活動" },
            new SelectListItem { Value = "Ticket", Text = "票券" }
        };

        [Display(Name = "優惠開始時間")]
        [Required(ErrorMessage = "開始時間為必填")]
        [DataType(DataType.DateTime)]
        // ✅ 修改：輸出 yyyy-MM-ddTHH:mm，對應 input[type=datetime-local]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "優惠結束時間")]
        [Required(ErrorMessage = "結束時間為必填")]
        [DataType(DataType.DateTime)]
        // ✅ 修改：輸出 yyyy-MM-ddTHH:mm
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Display(Name = "優惠券狀態")]
        [Required(ErrorMessage = "狀態為必填")]
        public string Status { get; set; } = null!;

        public List<SelectListItem> StatusOptions { get; set; } = new()
        {
            new SelectListItem { Value = "Active", Text = "啟用" },
            new SelectListItem { Value = "Inactive", Text = "停用" },
        };

        [Display(Name = "數量限制模式")]
        public string LimitMode { get; set; } = "unlimited";

        [Display(Name = "數量限制")]
        [Range(0, int.MaxValue, ErrorMessage = "數量限制不可為負數")]
        [RegularExpression("^\\d*$", ErrorMessage = "僅能輸入數字")]
        public int? Limit { get; set; }

        [Display(Name = "可重複使用")]
        public bool Reuse { get; set; } = false;

        [Display(Name = "限用次數模式")]
        public string UsesTimeMode { get; set; } = "unlimited";

        [Display(Name = "每人使用次數")]
        [Range(0, int.MaxValue, ErrorMessage = "限用次數不可為負數")]
        [RegularExpression("^\\d*$", ErrorMessage = "僅能輸入數字")]
        public int? UsesTime { get; set; }

        [Display(Name = "折扣類型")]
        [Required(ErrorMessage = "折扣類型為必選")]
        public string RuleType { get; set; } = null!;

        public List<SelectListItem> RuleTypeOptions { get; set; } = new()
        {
            new SelectListItem { Value = "FixedAmount", Text = "金額折扣" },
            new SelectListItem { Value = "Percentage", Text = "百分比優惠" }, // ✅ 修改：文案
            new SelectListItem { Value = "BuyXGetY", Text = "附帶優惠" },
            new SelectListItem { Value = "FreeShipping", Text = "免運" }
        };

        [Display(Name = "門檻類型")]
        public string ConditionType { get; set; } = "Amount";

        public List<SelectListItem> ConditionTypeOptions { get; set; } = new()
        {
            new SelectListItem { Value = "Amount", Text = "金額門檻" },
            new SelectListItem { Value = "MemberLevel", Text = "會員等級" }
        };

        [Display(Name = "金額門檻")]
        [Range(0, int.MaxValue, ErrorMessage = "金額門檻不可為負數")]
        [RegularExpression("^\\d*$", ErrorMessage = "僅能輸入數字")]
        public int? ConditionAmount { get; set; }

        [Display(Name = "會員等級")]
        public string? MemberLevel { get; set; } // ✅ 已在 VM，View 乾淨

        [Display(Name = "折扣數值")]
        [Range(0.0, double.MaxValue, ErrorMessage = "折扣數值不可為負數")]
        [RegularExpression("^\\d+(\\.\\d{1,2})?$", ErrorMessage = "請輸入正確數字格式")]
        public decimal? DiscountValue { get; set; }

        [HiddenInput]
        public string TargetCategory => Category;

        [Display(Name = "優惠說明")]
        [Required(ErrorMessage = "請輸入優惠說明")]
        public string Description { get; set; } = null!;

        // ======== 封裝：Controller 變乾淨 ========

        public void NormalizeModes()
        {
            if (string.Equals(LimitMode, "unlimited", StringComparison.OrdinalIgnoreCase))
                Limit = null;
            if (string.Equals(UsesTimeMode, "unlimited", StringComparison.OrdinalIgnoreCase))
                UsesTime = null;

            CouponCode = CouponCode?.ToUpper()?.Trim() ?? string.Empty;
            Name = Name?.Trim() ?? string.Empty;
            Category = Category?.Trim() ?? string.Empty;
            Status = Status?.Trim() ?? string.Empty;
        }

        public Promotion ToEntity(Promotion? existing = null)
        {
            var e = existing ?? new Promotion();
            e.No = this.No;
            e.Name = this.Name;
            e.CouponCode = this.CouponCode;
            e.Category = this.Category;
            e.StartDate = TrimToMinute(this.StartDate);
            e.EndDate = TrimToMinute(this.EndDate);
            e.Status = this.Status;
            e.Limit = this.Limit ?? 0;
            e.Reuse = this.Reuse;
            e.UsesTime = this.UsesTime ?? 0;
            return e;
        }

        public static PromotionFormViewModel FromEntity(Promotion e)
        {
            return new PromotionFormViewModel
            {
                No = e.No,
                Name = e.Name,
                CouponCode = e.CouponCode,
                Category = e.Category,
                StartDate = TrimToMinute(e.StartDate),
                EndDate = TrimToMinute(e.EndDate),
                Status = e.Status,
                Limit = (e.Limit == 0) ? (int?)null : e.Limit,
                Reuse = e.Reuse,
                UsesTime = (e.UsesTime == 0) ? (int?)null : e.UsesTime,
                LimitMode = (e.Limit == 0) ? "unlimited" : "limited",
                UsesTimeMode = (e.UsesTime == 0) ? "unlimited" : "limited",
            };
        }

        // 修正 ToRuleEntity 方法，將 Promotion_No 設定為傳入參數 promotionNo
        public PromotionRule ToRuleEntity(int promotionNo, PromotionRule? existing = null)
        {
            var r = existing ?? new PromotionRule();
            r.PromotionNo = promotionNo;
            r.RuleType = this.RuleType;
            r.DiscountValue = this.DiscountValue;
            r.ConditionType = this.ConditionType;
            r.ConditionAmount = string.Equals(this.ConditionType, "Amount", StringComparison.OrdinalIgnoreCase)
                                ? this.ConditionAmount : null;
            r.MemberLevel = string.Equals(this.ConditionType, "MemberLevel", StringComparison.OrdinalIgnoreCase)
                                ? this.MemberLevel : null;
            r.TargetCategory = this.TargetCategory;
            r.Description = this.Description;
            return r;
        }

        // ✅ 新增：Rule Entity → VM（Edit GET 載入）
        public void ApplyRule(PromotionRule? r)
        {
            if (r == null) return;
            RuleType = r.RuleType ?? RuleType;
            DiscountValue = r.DiscountValue;
            ConditionType = r.ConditionType ?? "Amount";
            ConditionAmount = string.Equals(ConditionType, "Amount", StringComparison.OrdinalIgnoreCase) ? r.ConditionAmount : null;
            MemberLevel = string.Equals(ConditionType, "MemberLevel", StringComparison.OrdinalIgnoreCase) ? r.MemberLevel : null;
            // TargetCategory 不覆蓋 Category（以主表為準）
            Description = r.Description ?? Description;
        }

        // ✅ 新增：商業驗證，Controller 呼叫
        public List<string> ValidateBusinessRules(DateTime now)
        {
            var errors = new List<string>();

            if (string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                if (now < StartDate || now > EndDate)
                {
                    errors.Add("優惠期間有誤，無法啟用\n請重新選擇優惠期間");
                }
            }

            if (Reuse)
            {
                var u = UsesTime ?? 0;
                if (u < 2)
                {
                    errors.Add("會員可重複使用時，「每人使用次數」需為 2 次以上。");
                }
            }

            return errors;
        }

        public void FillCategoryOptions(IEnumerable<string> categories)
        {
            var set = new HashSet<string>(CategoryOptions.Select(x => x.Value));
            foreach (var c in categories.Where(c => !string.IsNullOrWhiteSpace(c)))
            {
                if (set.Add(c!))
                    CategoryOptions.Add(new SelectListItem { Text = c!, Value = c! });
            }
            CategoryOptions = CategoryOptions.OrderBy(x => x.Text).ToList();
        }

        private static DateTime TrimToMinute(DateTime dt)
            => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);
    }
}
