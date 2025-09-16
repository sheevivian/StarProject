namespace StarProject.Services
{
    public interface IPromotionService
    {
        decimal CalculateDiscount(decimal originalAmount, string ruleType, decimal? discountValue, int? conditionAmount);
        bool IsPromotionValid(DateTime startDate, DateTime endDate);
        bool CheckPromotionCondition(decimal orderAmount, int? conditionAmount);
    }

    public class PromotionService : IPromotionService
    {
        public decimal CalculateDiscount(decimal originalAmount, string ruleType, decimal? discountValue, int? conditionAmount)
        {
            if (!discountValue.HasValue) return 0;

            return ruleType switch
            {
                "FixedAmount" => discountValue.Value,  // 固定金額折扣
                "Percentage" => originalAmount * (discountValue.Value / 100),  // 百分比折扣
                "FreeShipping" => 60,  // 免運費（假設運費60元）
                _ => 0
            };
        }

        public bool IsPromotionValid(DateTime startDate, DateTime endDate)
        {
            var now = DateTime.Now;
            return now >= startDate && now <= endDate;
        }

        public bool CheckPromotionCondition(decimal orderAmount, int? conditionAmount)
        {
            if (!conditionAmount.HasValue) return true;
            return orderAmount >= conditionAmount.Value;
        }
    }
}