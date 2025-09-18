using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;
using System.Globalization;

namespace StarProject.Controllers
{
    public class PromotionAnalysisController : Controller
    {
        private readonly StarProjectContext _context;

        public PromotionAnalysisController(StarProjectContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ✅ 綜合分析資料 API
        [HttpGet]
        public async Task<IActionResult> GetAnalysisData()
        {
            var monthlyTrend = await GetMonthlyTrend();
            var usageRanking = await GetUsageRanking();
            var kpiData = await GetKPIData();

            return Json(new
            {
                monthlyTrend,
                usageRanking,
                kpiData
            });
        }

        // ✅ 月度使用趨勢分析
        private async Task<object> GetMonthlyTrend()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var monthlyData = await _context.OrderMasters
                .Where(o => o.Date >= sixMonthsAgo && !string.IsNullOrEmpty(o.CouponCode))
                .GroupBy(o => new { o.Date.Year, o.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    TotalDiscount = g.Sum(o => o.DiscountAmount ?? 0)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var labels = monthlyData
                .Select(x => new DateTime(x.Year, x.Month, 1)
                .ToString("yyyy年MM月", CultureInfo.InvariantCulture))
                .ToList();

            var counts = monthlyData.Select(x => x.Count).ToList();
            var discounts = monthlyData.Select(x => x.TotalDiscount).ToList();

            return new { labels, counts, discounts };
        }

        // ✅ 優惠券使用排名
        private async Task<object> GetUsageRanking()
        {
            var ranking = await _context.OrderMasters
                .Where(o => !string.IsNullOrEmpty(o.CouponCode))
                .GroupBy(o => o.CouponCode)
                .Select(g => new
                {
                    CouponCode = g.Key,
                    UsageCount = g.Count(),
                    TotalDiscount = g.Sum(o => o.DiscountAmount ?? 0)
                })
                .OrderByDescending(x => x.UsageCount)
                .Take(10)
                .ToListAsync();

            return ranking;
        }

        // ✅ 關鍵績效指標 (KPI)
        private async Task<object> GetKPIData()
        {
            var totalOrders = await _context.OrderMasters.CountAsync();

            var couponOrders = await _context.OrderMasters
                .Where(o => !string.IsNullOrEmpty(o.CouponCode))
                .CountAsync();

            var totalRevenue = await _context.OrderMasters.SumAsync(o => o.AllTotal);

            var totalDiscount = await _context.OrderMasters.SumAsync(o => o.DiscountAmount ?? 0);

            var couponRevenue = await _context.OrderMasters
                .Where(o => !string.IsNullOrEmpty(o.CouponCode))
                .SumAsync(o => o.AllTotal);

            var nonCouponRevenue = await _context.OrderMasters
                .Where(o => string.IsNullOrEmpty(o.CouponCode))
                .SumAsync(o => o.AllTotal);

            var couponROI = totalDiscount > 0
                ? Math.Round((couponRevenue - totalDiscount) / totalDiscount * 100, 2)
                : 0;

            var couponUsageRate = totalOrders > 0
                ? Math.Round((double)couponOrders / totalOrders * 100, 2)
                : 0;

            var avgDiscountRate = totalRevenue > 0
                ? Math.Round(totalDiscount / totalRevenue * 100, 2)
                : 0;

            return new
            {
                totalOrders,
                couponOrders,
                totalRevenue,
                totalDiscount,
                couponRevenue,
                nonCouponRevenue,
                couponROI,
                couponUsageRate,
                avgDiscountRate
            };
        }

        // ✅ 分類分析
        [HttpGet]
        public async Task<IActionResult> GetCategoryAnalysis()
        {
            var categoryData = await _context.OrderMasters
                .Where(o => !string.IsNullOrEmpty(o.CouponCode))
                .GroupBy(o => o.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    OrderCount = g.Count(),
                    TotalDiscount = g.Sum(o => o.DiscountAmount ?? 0),
                    AvgDiscount = g.Average(o => o.DiscountAmount ?? 0)
                })
                .ToListAsync();

            return Json(categoryData);
        }
    }
}
