using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;

namespace StarProject.Controllers
{
    public class OrderDeliveryController : Controller
    {
        private readonly StarProjectContext _context;

        public OrderDeliveryController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: 出貨單列表
        public async Task<IActionResult> Index()
        {
            var deliveries = await _context.OrderDeliveries
                //.Include(d => d.Order) // 關聯 OrderMaster
                .Include(d => d.OrderStatuses) // 關聯狀態
                .ToListAsync();

            return View(deliveries);
        }

        // GET: 出貨單詳細
        public async Task<IActionResult> Details(int id)
        {
            var delivery = await _context.OrderDeliveries
                //.Include(d => d.Order)
                .Include(d => d.OrderStatuses)
                .FirstOrDefaultAsync(d => d.DeliveryId == id);

            if (delivery == null)
            {
                return NotFound();
            }

            return View(delivery);
        }

        // GET: 編輯收件資訊
        public async Task<IActionResult> Edit(int id)
        {
            var delivery = await _context.OrderDeliveries.FindAsync(id);
            if (delivery == null)
            {
                return NotFound();
            }
            return View(delivery);
        }

        // POST: 儲存收件資訊
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderDelivery delivery)
        {
            if (id != delivery.DeliveryId)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(delivery);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(delivery);
        }

        // POST: 刪除出貨單
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var delivery = await _context.OrderDeliveries.FindAsync(id);
            if (delivery != null)
            {
                _context.OrderDeliveries.Remove(delivery);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
