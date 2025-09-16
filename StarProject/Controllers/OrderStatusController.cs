using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;

public class OrderStatusController : Controller
{
    private readonly StarProjectContext _context;

    public OrderStatusController(StarProjectContext context)
    {
        _context = context;
    }

    // ================================
    // Index：已付款且未出貨或準備中
    // ================================
    public async Task<IActionResult> Index()
    {
        var orders = await _context.OrderMasters
            .Where(o => o.Category == "商品")
            .Where(o => o.PaymentStatus == "已付款" && o.Status == "未出貨" )

            .Include(o => o.OrderItems)
            .ToListAsync();

        return View(orders);
    }

    // ================================
    // 建立出貨單頁面
    // ================================
    public async Task<IActionResult> ShippingOrder(string orderNo)
    {
        if (string.IsNullOrEmpty(orderNo))
            return RedirectToAction("Index");

        //var order = await _context.OrderMasters
        //        .Include(o => o.OrderItems) // ✅ 一定要把明細撈出來
        //    .FirstOrDefaultAsync(o => o.No == orderNo);
        var order = await _context.OrderMasters
    .Include(o => o.OrderItems)
    .Include(o => o.UserNoNavigation) // User 導覽屬性
    .FirstOrDefaultAsync(o => o.No == orderNo);

        ViewBag.UserName = order.UserNoNavigation?.Name ?? "未知會員";

        if (order == null)
            return RedirectToAction("Index");
        ViewBag.OrderItems = order.OrderItems.ToList();

        var delivery = new OrderDelivery
        {
            OrderNo = order.No,
            UserNo = order.UserNo
        };

        return View(delivery);
    }

    // ================================
    // Submit 出貨單
    // ================================


    [HttpPost]

    public async Task<IActionResult> SubmitShippingOrder(OrderDelivery delivery)
    {

        ModelState.Remove("OrderNoNavigation");
        ModelState.Remove("UserNoNavigation");
        if (!ModelState.IsValid)
            return View("ShippingOrder", delivery);

        // 新增出貨單
        _context.OrderDeliveries.Add(delivery);
        await _context.SaveChangesAsync(); // EF Core 會自動生成 DeliveryId

        // 新增初始出貨狀態
        _context.OrderStatuses.Add(new OrderStatus
        {
            DeliveryId = delivery.DeliveryId,
            StatusType = "準備中",
            StatusTime = DateTime.Now,
            Notes = "出貨單剛建立"
        });
        await _context.SaveChangesAsync();

        return RedirectToAction("EditDelivery", new { id = delivery.DeliveryId });
    }

    // ================================
    // 查看 / 修改出貨單
    // ================================
    public async Task<IActionResult> EditDelivery(int id)
    {
        var delivery = await _context.OrderDeliveries
            .Include(d => d.OrderStatuses)
            .FirstOrDefaultAsync(d => d.DeliveryId == id);

        if (delivery == null)
            return RedirectToAction("Index");

        return View(delivery);
    }

    // ================================
    // 更新收件資訊
    // ================================
    [HttpPost]
    public async Task<IActionResult> UpdateDelivery(OrderDelivery delivery)
    {
        if (!ModelState.IsValid)
            return View("EditDelivery", delivery);

        var existing = await _context.OrderDeliveries.FindAsync(delivery.DeliveryId);
        if (existing != null)
        {
            existing.RecipientName = delivery.RecipientName;
            existing.RecipientAddress = delivery.RecipientAddress;
            existing.RecipientPhone = delivery.RecipientPhone;
            existing.Notes = delivery.Notes;

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("EditDelivery", new { id = delivery.DeliveryId });
    }

    // ================================
    // 更新出貨狀態（準備中 → 未出貨 → 已出貨 → 已送達）
    // ================================
    [HttpPost]
    public async Task<IActionResult> AdvanceStatus(int deliveryId)
    {
        var delivery = await _context.OrderDeliveries
            .Include(d => d.OrderStatuses)
            .FirstOrDefaultAsync(d => d.DeliveryId == deliveryId);

        if (delivery == null) return RedirectToAction("Index");

        var lastStatus = delivery.OrderStatuses
            .OrderByDescending(s => s.StatusTime)
            .FirstOrDefault()?.StatusType ?? "準備中";

        var sequence = new[] { "準備中", "未出貨", "已出貨", "已送達" };
        var idx = Array.IndexOf(sequence, lastStatus);

        if (idx < sequence.Length - 1)
        {
            var next = sequence[idx + 1];
            _context.OrderStatuses.Add(new OrderStatus
            {
                DeliveryId = deliveryId,
                StatusType = next,
                StatusTime = DateTime.Now,
                Notes = $"狀態更新為 {next}"
            });

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("EditDelivery", new { id = deliveryId });
    }

    // ================================
    // 刪除出貨單
    // ================================
    [HttpPost]
    public async Task<IActionResult> DeleteDelivery(int id)
    {
        var delivery = await _context.OrderDeliveries.FindAsync(id);
        if (delivery != null)
        {
            // 先刪除相關出貨狀態
            var statuses = _context.OrderStatuses.Where(s => s.DeliveryId == id);
            _context.OrderStatuses.RemoveRange(statuses);

            // 再刪除出貨單
            _context.OrderDeliveries.Remove(delivery);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}
