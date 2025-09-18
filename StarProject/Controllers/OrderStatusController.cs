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
            .Where(o => o.PaymentStatus == "已付款" && o.Status == "未出貨")
            .Include(o => o.UserNoNavigation)   // 加這個，載入會員資料
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

        var order = await _context.OrderMasters
            .Include(o => o.OrderItems)
            .Include(o => o.UserNoNavigation) // ✅ 撈會員資料
            .FirstOrDefaultAsync(o => o.No == orderNo);

        if (order == null)
            return RedirectToAction("Index");

        var delivery = new OrderDelivery
        {
            OrderNo = order.No,
            UserNo = order.UserNo,
            RecipientName = order.UserNoNavigation?.Name,        // ✅ 預設帶會員姓名
            RecipientAddress = order.UserNoNavigation?.Address,  // ✅ 預設帶會員地址
            RecipientPhone = order.UserNoNavigation?.Phone       // ✅ 預設帶會員電話
        };

        ViewBag.Order = order; // ✅ 傳去 CSHTML
        return View(delivery);
    }




    [HttpPost]

    //public async Task<IActionResult> SubmitShippingOrder(OrderDelivery delivery)
    //{

    //    ModelState.Remove("OrderNoNavigation");
    //    ModelState.Remove("UserNoNavigation");
    //    if (!ModelState.IsValid)
    //        return View("ShippingOrder", delivery);

    //    // 新增出貨單
    //    _context.OrderDeliveries.Add(delivery);
    //    await _context.SaveChangesAsync(); // EF Core 會自動生成 DeliveryId

    //    // 新增初始出貨狀態
    //    _context.OrderStatuses.Add(new OrderStatus
    //    {
    //        DeliveryId = delivery.DeliveryId,
    //        StatusType = "準備中",
    //        StatusTime = DateTime.Now,
    //        Notes = "出貨單剛建立"
    //    });
    //    await _context.SaveChangesAsync();

    //    return RedirectToAction("EditDelivery", new { id = delivery.DeliveryId });
    //}
    // 1️⃣ Submit 出貨單
    [HttpPost]
    public async Task<IActionResult> SubmitShippingOrder(OrderDelivery delivery)
    {
        ModelState.Remove("OrderNoNavigation");
        ModelState.Remove("UserNoNavigation");

        if (!ModelState.IsValid)
            return View("ShippingOrder", delivery);

        // 新增出貨單
        _context.OrderDeliveries.Add(delivery);
        await _context.SaveChangesAsync();

        // 新增初始出貨狀態
        _context.OrderStatuses.Add(new OrderStatus
        {
            DeliveryId = delivery.DeliveryId,
            StatusType = "準備中",
            StatusTime = DateTime.Now,
            Notes = "出貨單剛建立"
        });

        // 更新 OrderMaster 狀態
        var order = await _context.OrderMasters
            .FirstOrDefaultAsync(o => o.No == delivery.OrderNo);
        if (order != null)
        {
            order.Status = "準備中";
        }

        await _context.SaveChangesAsync();

        // Redirect 到 EditDelivery
        return RedirectToAction("EditDelivery", new { id = delivery.DeliveryId });
    }

    // 2️⃣ EditDelivery
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
    [HttpPost]
    public async Task<IActionResult> UpdateDelivery(OrderDelivery delivery)
    {
        _context.Attach(delivery);
        _context.Entry(delivery).Property(d => d.RecipientName).IsModified = true;
        _context.Entry(delivery).Property(d => d.RecipientAddress).IsModified = true;
        _context.Entry(delivery).Property(d => d.RecipientPhone).IsModified = true;
        _context.Entry(delivery).Property(d => d.Notes).IsModified = true;

        await _context.SaveChangesAsync();

        // 撈最新資料顯示
        var updated = await _context.OrderDeliveries
            .Include(d => d.OrderStatuses)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DeliveryId == delivery.DeliveryId);

        return View("EditDelivery", updated);

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
        var delivery = await _context.OrderDeliveries
            .FirstOrDefaultAsync(d => d.DeliveryId == id);

        if (delivery != null)
        {
            // 先刪除對應的出貨狀態紀錄
            var statuses = _context.OrderStatuses
                .Where(s => s.DeliveryId == delivery.DeliveryId);
            _context.OrderStatuses.RemoveRange(statuses);

            // 🟣 更新 OrderMaster.Status = "未出貨"
            var order = await _context.OrderMasters
                .FirstOrDefaultAsync(o => o.No == delivery.OrderNo);
            if (order != null)
            {
                order.Status = "未出貨";
                _context.OrderMasters.Update(order);
            }

            // 再刪除出貨單
            _context.OrderDeliveries.Remove(delivery);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}

