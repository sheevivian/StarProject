using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;
using StarProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public class TicketStockController : Controller
    {
        private readonly StarProjectContext _context;

        public TicketStockController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: TicketStock
        public async Task<IActionResult> Index()
        {
			// 1️⃣ 把每日庫存投影成同樣結構
			var dailyStocks = _context.TicketStocks
				.Select(s => new
				{
					TicketNo = s.TicketNo,
					Date = s.Date.Date,
					Stock = (int?)s.Stock,      // 用 int? 讓 null 可能存在
					TransQuantity = (int?)null  // 異動先為 null
				});

            // 2️⃣ 把異動表投影成同樣結構（Stock=null）
            var transStocks = _context.TicketTransStocks
                .Select(t => new
                {
                    TicketNo = t.TicketNo,
                    Date = t.Date.Date,
                    Stock = (int?)null,
                    // 判斷加減數：如果 TransQuantity 自己已正負，就不用再判斷
                    TransQuantity = (int?)t.TransQuantity
                });

			// 3️⃣ 合併兩個查詢
			var combined = dailyStocks.Concat(transStocks);

			// 4️⃣ GroupBy 票券+日期，計算合計
			var query = from c in combined
						group c by new { c.TicketNo, c.Date } into g
						select new TicketStockSumViewModel
						{
							TicketNo = g.Key.TicketNo,
							Date = g.Key.Date,
							TotalStock = g.Sum(x => x.Stock ?? 0)
									   + g.Sum(x => x.TransQuantity ?? 0)
						};

			var result = await query.ToListAsync();

			return View(result);
		}

        // GET: TicketStock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketStock = await _context.TicketStocks
                .Include(t => t.TicketNoNavigation)
                .FirstOrDefaultAsync(m => m.No == id);
            if (ticketStock == null)
            {
                return NotFound();
            }

            return View(ticketStock);
        }

        // GET: TicketStock/Create
        public IActionResult Create()
        {
            ViewData["TicketNo"] = new SelectList(_context.Tickets, "No", "No");
            return View();
        }

        // POST: TicketStock/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketNo,Date,Stock,No")] TicketStock ticketStock)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketStock);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketNo"] = new SelectList(_context.Tickets, "No", "No", ticketStock.TicketNo);
            return View(ticketStock);
        }


        private bool TicketStockExists(int id)
        {
            return _context.TicketStocks.Any(e => e.No == id);
        }
    }
}
