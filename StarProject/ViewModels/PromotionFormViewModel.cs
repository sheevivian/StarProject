using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace StarProject.ViewModels
{
    public class PromotionFormViewModel
    {
        // 主鍵，資料庫自動產生，View中隱藏顯示
        [HiddenInput]
        public int No { get; set; }

        // 優惠名稱（必填）
        [Display(Name = "優惠名稱")]
        [Required(ErrorMessage = "優惠名稱為必填欄位")]
        public string Name { get; set; } = null!;

        // 優惠代碼（必填）
        [Display(Name = "優惠代碼")]
        [Required(ErrorMessage = "優惠代碼為必填欄位")]
        public string CouponCode { get; set; } = null!;

        // 適用類別（下拉選單）
        [Display(Name = "適用類別")]
        [Required(ErrorMessage = "適用類別為必選")]
        public string Category { get; set; } = null!;
        public List<SelectListItem> CategoryOptions { get; } = new()
        {
            new SelectListItem { Value = "ALL", Text = "全館" },
            new SelectListItem { Value = "Product", Text = "商品" },
            new SelectListItem { Value = "Activity", Text = "活動" },
            new SelectListItem { Value = "Ticket", Text = "票券" }
        };

        // 開始時間（使用者可選年/月/日/時/分）
        [Display(Name = "優惠開始時間")]
        [Required(ErrorMessage = "開始時間為必填")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        // 結束時間（使用者可選年/月/日/時/分）
        [Display(Name = "優惠結束時間")]
        [Required(ErrorMessage = "結束時間為必填")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        // 啟用/停用（單選）→ 實際狀態還需由後端時間判斷補上 Expired
        [Display(Name = "優惠券狀態")]
        [Required(ErrorMessage = "狀態為必填")]
        public string Status { get; set; } = null!;
        public List<SelectListItem> StatusOptions { get; } = new()
        {
            new SelectListItem { Value = "Active", Text = "啟用" },
            new SelectListItem { Value = "Inactive", Text = "停用" },
            //new SelectListItem { Value = "Expired", Text = "已過期" }
        };

        // 數量限制模式（Controller 需要根據 LimitMode 決定是否存值，unlimited會變成NULL）
        [Display(Name = "數量限制模式")]
        public string LimitMode { get; set; } = "unlimited";

        // 數量限制（輸入框，僅接受數字）
        [Display(Name = "數量限制")]
        [RegularExpression("^\\d*$", ErrorMessage = "僅能輸入數字")]
        public int? Limit { get; set; }

        // 是否可重複使用（預設為 false）
        [Display(Name = "可重複使用")]
        public bool Reuse { get; set; } = false;

        // 限用次數模式（Controller 需要根據 UsesTimeMode 決定是否存值，unlimited會變成NULL）
        [Display(Name = "限用次數模式")]
        public string UsesTimeMode { get; set; } = "unlimited";

        // 每人限用次數（輸入框，僅接受數字）
        [Display(Name = "每人限用次數")]
        [RegularExpression("^\\d*$", ErrorMessage = "僅能輸入數字")]
        public int? UsesTime { get; set; }

        // PromotionRule欄位
        // 折扣規則類型（下拉選單）
        [Display(Name = "折扣類型")]
        [Required(ErrorMessage = "折扣類型為必選")]
        public string RuleType { get; set; } = null!;
        public List<SelectListItem> RuleTypeOptions { get; } = new()
        {
            new SelectListItem { Value = "FixedAmount", Text = "金額折扣" },
            new SelectListItem { Value = "Percentage", Text = "百分比折扣" },
            new SelectListItem { Value = "BuyXGetY", Text = "附帶優惠" },
            new SelectListItem { Value = "FreeShipping", Text = "免運" }
        };

        // 門檻條件類型（Amount or MemberLevel）
        [Display(Name = "門檻類型")]
        public string ConditionType { get; set; } = "Amount";
        public List<SelectListItem> ConditionTypeOptions { get; } = new()
        {
            new SelectListItem { Value = "Amount", Text = "金額門檻" },
            new SelectListItem { Value = "MemberLevel", Text = "會員等級" }
        };

        // 金額門檻（限制折扣發動的最低消費）
        [Display(Name = "金額門檻")]
        [RegularExpression("^\\d*$", ErrorMessage = "僅能輸入數字")]
        public int? ConditionAmount { get; set; }

        // 指定會員等級（依 ConditionType 決定是否顯示）
        [Display(Name = "會員等級")]
        public string? MemberLevel { get; set; } 

        // 折扣金額或比例，允許小數點（針對不同折扣類型）
        [Display(Name = "折扣數值")]
        [RegularExpression("^\\d+(\\.\\d{1,2})?$", ErrorMessage = "請輸入正確數字格式")]
        public decimal? DiscountValue { get; set; }

        // 僅後端使用：因 PromotionRule 需關聯分類，透過此欄位將主表的 Category 傳入，無需用戶輸入
        [HiddenInput]
        public string TargetCategory => Category;

        // 優惠說明欄位（必填，避免使用者誤會）
        [Display(Name = "優惠說明")]
        [Required(ErrorMessage = "請輸入優惠說明")]
        public string Description { get; set; } = null!;
    }
}
