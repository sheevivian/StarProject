using Microsoft.AspNetCore.Mvc;
using StarProject.Models;
using System.Linq;

public class AnalysisController : Controller
{
    private readonly StarProjectContext _context;

    public AnalysisController(StarProjectContext context)
    {
        _context = context;
    }

    // 首頁 View
    public IActionResult Index()
    {
        return View(); // Views/Chart/analysis.cshtml
    }

    // 商品類別 API
    [HttpGet]
    public IActionResult GetMonthlyProductData(int year, int month)
    {
        var productCategories = new List<string> { "卡片書籤", "服包飾品", "益智桌遊", "地球儀", "生活雜貨", "書籍刊物", "餐廚用品", "交通卡", "設計文具", "望遠鏡" };
        var query = from item in _context.OrderItems
                    join order in _context.OrderMasters
                    on item.OrderNo equals order.No  // <-- 改這裡
                    where productCategories.Contains(item.Category)
                          && order.Date.Year == year
                          && order.Date.Month == month
                    select item;


        var counts = query
            .GroupBy(o => o.Category)
            .Select(g => new { name = g.Key, value = g.Sum(x => x.Quantity) })
            .ToList();

        var revenues = query
            .GroupBy(o => o.Category)
            .Select(g => new { name = g.Key, value = g.Sum(x => x.Quantity * x.UnitPrice) })
            .ToList();

        return Json(new { counts, revenues });
    }


    // 票券類別 API
    [HttpGet]
    public IActionResult GetMonthlyTicketData(int year, int month)
    {
        var ticketCategories = new List<string> { "星際探險", "常設展覽", "星空劇院", "優惠套票", "特別展覽", "立體劇場" };

        var query = from item in _context.OrderItems
                    join order in _context.OrderMasters
                    on item.OrderNo equals order.No
                    where ticketCategories.Contains(item.Category)
                          && order.Date.Year == year
                          && order.Date.Month == month
                    select item;

        var counts = query
            .GroupBy(o => o.Category)
            .Select(g => new { name = g.Key, value = g.Sum(x => x.Quantity) })
            .ToList();

        var revenues = query
            .GroupBy(o => o.Category)
            .Select(g => new { name = g.Key, value = g.Sum(x => x.Quantity * x.UnitPrice) })
            .ToList();

        return Json(new { counts, revenues });
    }
    public IActionResult RevenueByMonth()
    {
        var query = _context.OrderMasters
            .GroupBy(o => new { o.Category, o.Date.Year, o.Date.Month })
            .Select(g => new
            {
                Category = g.Key.Category,
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(x => x.AllTotal)
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        return Json(query);
    }
    [HttpGet]
    public IActionResult GetDiscountRevenueData(string type) // type=Product / Ticket
    {
        var productCategories = new List<string> { "卡片書籤", "服包飾品", "益智桌遊", "地球儀", "生活雜貨", "書籍刊物", "餐廚用品", "交通卡", "設計文具", "望遠鏡" };
        var ticketCategories = new List<string> { "星際探險", "常設展覽", "星空劇院", "優惠套票", "特別展覽", "立體劇場" };
        var categories = type == "Product" ? productCategories : ticketCategories;

        var data = _context.OrderItems
            .Where(o => categories.Contains(o.Category))
            .Select(o => new
            {
                discount = o.Discount,
                revenue = o.Quantity * o.UnitPrice
            })
            .ToList();

        return Json(data);
    }

    // 依類別抓出前三名商品
    // 🟢 1. 商品類別 → 該類別前三名商品 (依 Quantity 排序)
    [HttpGet]
    public IActionResult GetTopProductsByCategory(string category)
    {
        // 商品類別清單（非票券）
        var productCategories = new List<string> {
        "卡片書籤","服包飾品","益智桌遊","地球儀","生活雜貨",
        "書籍刊物","餐廚用品","交通卡","設計文具","望遠鏡"
    };

        if (!productCategories.Contains(category))
            return Json(new List<object>());

        var data = _context.OrderItems
            .Where(o => o.Category == category)
            .GroupBy(o => o.Name)
            .Select(g => new {
                name = g.Key,
                value = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.value)
            .Take(3)
            .ToList();

        return Json(data);
    }

    // 🟢 2. 年月 → 票券 Type 總數量
    [HttpGet]
    public IActionResult GetTicketTypeCountsByMonth(int year, int month)
    {
        var ticketCategories = new List<string> {
        "星際探險","常設展覽","星空劇院","優惠套票","特別展覽","立體劇場"
    };

        var data = (from item in _context.OrderItems
                    join order in _context.OrderMasters
                    on item.OrderNo equals order.No
                    where ticketCategories.Contains(item.Category)
                          && order.Date.Year == year
                          && order.Date.Month == month
                    group item by item.Type into g
                    select new
                    {
                        name = g.Key,
                        value = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.value)
                    .ToList();

        return Json(data);
    }

}




