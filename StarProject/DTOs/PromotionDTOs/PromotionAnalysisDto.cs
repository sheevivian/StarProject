using StarProject.Models;

namespace StarProject.DTOs.PromotionDTOs
{
    public class PromotionAnalysisDto
    {
        public int PromotionNo { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "";
        public int UsesTime { get; set; }          // 已做非 null 化
        public int UsageCount { get; set; }        // 使用次數
        public int UniqueUsers { get; set; }       // 不重複使用人數

        // ✅ 計算屬性：前端可直接使用
        public double UsageRate =>
            UsesTime <= 0 ? 0 : Math.Round((double)UsageCount / UsesTime * 100, 2);

        /// <summary>
        /// ✅ 提供 EF 可翻譯的查詢組裝（Join PromotionUsage 並彙總）
        /// </summary>
        public static IQueryable<PromotionAnalysisDto> BuildQuery(
            IQueryable<Promotion> promotions,
            IQueryable<PromotionUsage> usages,
            string? search)
        {
            // ✅ 先在 Promotion 端套搜尋（可翻譯到 SQL）
            if (!string.IsNullOrWhiteSpace(search))
            {
                var key = search.Trim();
                promotions = promotions.Where(p =>
                    (p.Name ?? "").Contains(key) ||
                    (p.Category ?? "").Contains(key));
            }

            // ✅ Group Join + 彙總（Count / Distinct Count）
            var query =
                from p in promotions
                join u in usages on p.No equals u.PromotionNo into ug
                select new PromotionAnalysisDto
                {
                    PromotionNo = p.No,
                    Name = p.Name,
                    Category = p.Category,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    UsesTime = p.UsesTime ?? 0,
                    UsageCount = ug.Count(),
                    UniqueUsers = ug.Select(x => x.UserNo).Distinct().Count()
                };

            return query;
        }
    }
}
