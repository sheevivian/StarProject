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
  //      public async Task<IActionResult> Index()
  //      {
		//	// Step 1: 先在 DB 層做 GroupBy，算好 Sum / Max
		//	var stockSummaryQuery = _context.TicketStock
		//		.GroupBy(ss => ss.ProductNo)
		//		.Select(g => new
		//		{
		//			ProductNo = g.Key,
		//			SumQuantity = g.Sum(x => x.TransQuantity),
		//			UpdateDate = g.Max(x => x.Date)
		//		});

		//	// Step 2: 再用上面的結果去 Join Products
		//	var query = from s in stockSummaryQuery
		//				join p in _context.Products.Include(p => p.ProCategoryNoNavigation)
		//					on s.ProductNo equals p.No
		//				select new ProductStockSumViewModel
		//				{
		//					ProductNo = s.ProductNo,
		//					ProductName = p.Name,
		//					ProCategoryName = p.ProCategoryNoNavigation.Name,
		//					SumQuantity = s.SumQuantity,
		//					UpdateDate = s.UpdateDate,
		//				};

  //          return View();
		//}

        // GET: TicketStock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketStock = await _context.TicketStock
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
            return _context.TicketStock.Any(e => e.No == id);
        }
    }
}
