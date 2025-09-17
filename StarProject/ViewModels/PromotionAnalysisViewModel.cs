namespace StarProject.ViewModels
{
    public class PromotionAnalysisViewModel
    {
        public int PromotionNo { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public int UsesTime { get; set; }
        public int UsageCount { get; set; }   // 實際使用次數
        public int UniqueUsers { get; set; }  // 不重複使用人數
        public double UsageRate { get; set; } // 使用率 = UsageCount / UsesTime
    }

}
