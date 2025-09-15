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
			var query =
				from s in _context.TicketStocks
				join t in _context.TicketTransStocks
					on new { s.Ticket_No, Date = s.Date.Date }
					equals new { t.Ticket_No, Date = t.Date.Date } into gj
				from t in gj.DefaultIfEmpty() // left join
				group t by new { s.Ticket_No, s.Date, s.Stock } into g
				select new
				{
					TicketNo = g.Key.Ticket_No,
					Date = g.Key.Date,
					// 異動數量要判斷正負
					TotalStock = g.Key.Stock + g.Sum(x =>
						x == null ? 0 :
						(x.Type == "Sale" ? -x.TransQuantity : x.TransQuantity))
				};

			var result = await query.ToListAsync();

			return View();
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
