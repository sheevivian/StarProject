using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;

namespace StarProject.Controllers
{
    public class TicketStocksController : Controller
    {
        private readonly StarProjectContext _context;

        public TicketStocksController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: TicketStocks
        public async Task<IActionResult> Index()
        {
            var starProjectContext = _context.TicketStock.Include(t => t.TicketNoNavigation);
            return View(await starProjectContext.ToListAsync());
        }

        // GET: TicketStocks/Details/5
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

        // GET: TicketStocks/Create
        public IActionResult Create()
        {
            ViewData["TicketNo"] = new SelectList(_context.Tickets, "No", "No");
            return View();
        }

        // POST: TicketStocks/Create
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

        // GET: TicketStocks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketStock = await _context.TicketStock.FindAsync(id);
            if (ticketStock == null)
            {
                return NotFound();
            }
            ViewData["TicketNo"] = new SelectList(_context.Tickets, "No", "No", ticketStock.TicketNo);
            return View(ticketStock);
        }

        // POST: TicketStocks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketNo,Date,Stock,No")] TicketStock ticketStock)
        {
            if (id != ticketStock.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketStock);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketStockExists(ticketStock.No))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketNo"] = new SelectList(_context.Tickets, "No", "No", ticketStock.TicketNo);
            return View(ticketStock);
        }

        // GET: TicketStocks/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: TicketStocks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketStock = await _context.TicketStock.FindAsync(id);
            if (ticketStock != null)
            {
                _context.TicketStock.Remove(ticketStock);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketStockExists(int id)
        {
            return _context.TicketStock.Any(e => e.No == id);
        }
    }
}
