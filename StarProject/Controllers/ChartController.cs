using Microsoft.AspNetCore.Mvc;
using StarProject.Models;

namespace StarProject.Controllers
{
    // OrdersController.cs
    public class ChartController : Controller
    {
        private readonly StarProjectContext _context;

        public ChartController(StarProjectContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(); // 對應 Views/Chart/Index.cshtml
        }

        // 如果你要提供 Chart 資料的 API
        public IActionResult GetCategoryNestedData()
        {
            var categories = new List<string> {
        "卡片書籤", "服包飾品", "益智桌遊", "地球儀",
        "生活雜貨", "書籍刊物", "餐廚用品", "交通卡",
        "設計文具", "望遠鏡"
    };


            var counts = _context.OrderItems
                .Where(o => categories.Contains(o.Category))
                .GroupBy(o => o.Category)
                .Select(g => new { name = g.Key, value = g.Count() })
                .ToList();



            var revenues = _context.OrderItems // 如果票券也是在 OrderItem
                        .Where(o => categories.Contains(o.Category))
                        .GroupBy(o => o.Category)
                        .Select(g => new {
                            name = g.Key,
                            value = g.Sum(x => x.UnitPrice * x.Quantity) // 改成營收總和
                        })
                   .ToList();

            return Json(new { counts, revenues });
        }

        public IActionResult GetTicketCategoryData()
        {
            var ticketCategories = new List<string> {
        "星際探險", "常設展覽", "星空劇院",
        "優惠套票", "特別展覽", "立體劇場"
    };

            var counts = _context.OrderItems
                            .Where(o => ticketCategories.Contains(o.Category))
                            .GroupBy(o => o.Category)
                            .Select(g => new { name = g.Key, value = g.Count() })
                            .ToList();



            var revenues = _context.OrderItems // 如果票券也是在 OrderItem
                        .Where(o => ticketCategories.Contains(o.Category))
                        .GroupBy(o => o.Category)
                        .Select(g => new {
                            name = g.Key,
                            value = g.Sum(x => x.UnitPrice * x.Quantity) // 改成營收總和
                        })
                   .ToList();




            return Json(new { counts, revenues });
        }

    }
}
    

