using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using StarProject.Models; // ★ 新增：為了 ToEntity / FromEntity

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

        // ★ 修改：改成 get;set;，讓 Controller 可以注入 DB 取回的選項
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
        public DateTime StartDate { get; set; }

        [Display(Name = "優惠結束時間")]
        [Required(ErrorMessage = "結束時間為必填")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Display(Name = "優惠券狀態")]
        [Required(ErrorMessage = "狀態為必填")]
        public string Status { get; set; } = null!;

        // ★ 修改：改成 get;set; 以便擴充
        public List<SelectListItem> StatusOptions { get; set; } = new()
        {
            new SelectListItem { Value = "Active", Text = "啟用" },
            new SelectListItem { Value = "Inactive", Text = "停用" },
            // new SelectListItem { Value = "Expired", Text = "已過期" }
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

        [Display(Name = "每人限用次數")]
        [Range(0, int.MaxValue, ErrorMessage = "限用次數不可為負數")]
        [RegularExpression("^\\d*$", ErrorMessage = "僅能輸入數字")]
        public int? UsesTime { get; set; }

        [Display(Name = "折扣類型")]
        [Required(ErrorMessage = "折扣類型為必選")]
        public string RuleType { get; set; } = null!;

        // ★ 修改：改成 get;set; 以便擴充
        public List<SelectListItem> RuleTypeOptions { get; set; } = new()
        {
            new SelectListItem { Value = "FixedAmount", Text = "金額折扣" },
            new SelectListItem { Value = "Percentage", Text = "百分比折扣" },
            new SelectListItem { Value = "BuyXGetY", Text = "附帶優惠" },
            new SelectListItem { Value = "FreeShipping", Text = "免運" }
        };

        [Display(Name = "門檻類型")]
        public string ConditionType { get; set; } = "Amount";

        // ★ 修改：改成 get;set; 以便擴充
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
        public string? MemberLevel { get; set; }

        [Display(Name = "折扣數值")]
        [Range(0.0, double.MaxValue, ErrorMessage = "折扣數值不可為負數")]
        [RegularExpression("^\\d+(\\.\\d{1,2})?$", ErrorMessage = "請輸入正確數字格式")]
        public decimal? DiscountValue { get; set; }

        [HiddenInput]
        public string TargetCategory => Category;

        [Display(Name = "優惠說明")]
        [Required(ErrorMessage = "請輸入優惠說明")]
        public string Description { get; set; } = null!;

        // ========= ★ 新增：Controller 呼叫這些方法，讓 Controller 變輕 =========

        /// <summary>
        /// 依據 LimitMode / UsesTimeMode 正規化數值
        /// </summary>
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

        /// <summary>
        /// VM → Entity
        /// </summary>
        public Promotion ToEntity(Promotion? existing = null)
        {
            var e = existing ?? new Promotion();
            e.No = this.No;
            e.Name = this.Name;
            e.CouponCode = this.CouponCode;
            e.Category = this.Category;
            e.StartDate = this.StartDate;
            e.EndDate = this.EndDate;
            e.Status = this.Status;
            e.Limit = this.Limit ?? 0;     // 若 DB 欄位是 int 非 nullable，用 0 表示無限制
            e.Reuse = this.Reuse;
            e.UsesTime = this.UsesTime ?? 0;
            // 其他 PromotionRule 相關欄位，看你的實體是否存在後續再補
            return e;
        }

        /// <summary>
        /// Entity → VM
        /// </summary>
        public static PromotionFormViewModel FromEntity(Promotion e)
        {
            return new PromotionFormViewModel
            {
                No = e.No,
                Name = e.Name,
                CouponCode = e.CouponCode,
                Category = e.Category,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Status = e.Status,
                Limit = (e.Limit == 0) ? null : e.Limit,
                Reuse = e.Reuse,
                UsesTime = (e.UsesTime == 0) ? null : e.UsesTime,
                LimitMode = (e.Limit == 0) ? "unlimited" : "limited",
                UsesTimeMode = (e.UsesTime == 0) ? "unlimited" : "limited",
                // 其他規則欄位請依你的資料表再映射
            };
        }

        /// <summary>
        /// 由 Controller 提供 DB 類別清單後，灌入選項（避免 VM 依賴 DbContext）
        /// </summary>
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
    }
}
