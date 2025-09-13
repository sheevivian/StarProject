using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StarProject.Models;

public class OrdersController : Controller
{
    private readonly StarProjectContext _context;

    public OrdersController(StarProjectContext context)
    {
        _context = context;
    }

    // 訂單列表 + 查詢 + 分頁
    public IActionResult Index(string category, string paymentStatus, string userName, int? page)
    {
        // 下拉選單清單
        ViewBag.Categories = new List<SelectListItem>
        {
            new SelectListItem { Text = "全部", Value = "", Selected = string.IsNullOrEmpty(category) },
            new SelectListItem { Text = "商品", Value = "商品", Selected = category == "商品" },
            new SelectListItem { Text = "票券", Value = "票券", Selected = category == "票券" }
        };
        ViewBag.PaymentStatuses = new List<SelectListItem>
        {
            new SelectListItem { Text = "全部", Value = "", Selected = string.IsNullOrEmpty(paymentStatus) },
            new SelectListItem { Text = "已付款", Value = "已付款", Selected = paymentStatus == "已付款" },
            new SelectListItem { Text = "待確認", Value = "待確認", Selected = paymentStatus == "待確認" },
            new SelectListItem { Text = "未付款", Value = "未付款", Selected = paymentStatus == "未付款" }
        };

        ViewBag.SelectedUserName = userName ?? "";

        // 查詢
        var query = _context.OrderMasters.Include(o => o.UserNoNavigation).AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(o => o.Category == category);
        if (!string.IsNullOrEmpty(paymentStatus))
            query = query.Where(o => o.PaymentStatus == paymentStatus);
        if (!string.IsNullOrEmpty(userName))
            query = query.Where(o => o.UserNoNavigation.Name.Contains(userName));

        // 分頁
        int pageSize = 10;
        int pageNumber = page ?? 1;
        int totalItems = query.Count();
        var items = query.OrderBy(o => o.No).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var result = new PagedResult<OrderMaster>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        return View(result);
    }

    // 訂單明細
    public IActionResult Details(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var order = _context.OrderMasters.Include(o => o.UserNoNavigation)
                                         .FirstOrDefault(o => o.No == id);
        if (order == null) return NotFound();

        var items = _context.OrderItems.Where(oi => oi.OrderNo == id).ToList();

        ViewBag.Order = order;
        ViewBag.Items = items;

        return View();
    }

    // 匯出 Excel (非商業用)
    public IActionResult ExportExcel(string category, string paymentStatus, string userName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var query = _context.OrderMasters.Include(o => o.UserNoNavigation).AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(o => o.Category == category);
        if (!string.IsNullOrEmpty(paymentStatus))
            query = query.Where(o => o.PaymentStatus == paymentStatus);
        if (!string.IsNullOrEmpty(userName))
            query = query.Where(o => o.UserNoNavigation.Name.Contains(userName));

        var orders = query.ToList();

        using (var package = new ExcelPackage())
        {
            var ws = package.Workbook.Worksheets.Add("訂單報表");
            ws.Cells[1, 1].Value = "訂單編號";
            ws.Cells[1, 2].Value = "會員姓名";
            ws.Cells[1, 3].Value = "商品種類";
            ws.Cells[1, 4].Value = "總金額";
            ws.Cells[1, 5].Value = "付款狀態";
            ws.Cells[1, 6].Value = "日期";

            int row = 2;
            foreach (var o in orders)
            {
                ws.Cells[row, 1].Value = o.No;
                ws.Cells[row, 2].Value = o.UserNoNavigation.Name;
                ws.Cells[row, 3].Value = o.Category;
                ws.Cells[row, 4].Value = o.AllTotal;
                ws.Cells[row, 5].Value = o.PaymentStatus;
                ws.Cells[row, 6].Value = o.Date.ToString("yyyy-MM-dd");
                row++;
            }

            ws.Cells[1, 1, row - 1, 6].AutoFitColumns();

            var fileBytes = package.GetAsByteArray();
            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "OrderReport.xlsx");
        }
    }
}

