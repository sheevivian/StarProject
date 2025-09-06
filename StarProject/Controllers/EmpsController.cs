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
    public class EmpsController : Controller
    {
        private readonly StarProjectContext _context;

        public EmpsController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: Emps
        public async Task<IActionResult> Index()
        {
            var starProjectContext = _context.Emps.Include(e => e.DeptNoNavigation).Include(e => e.RoleNoNavigation);
            return View(await starProjectContext.ToListAsync());
        }

        // GET: Emps/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emp = await _context.Emps
                .Include(e => e.DeptNoNavigation)
                .Include(e => e.RoleNoNavigation)
                .FirstOrDefaultAsync(m => m.No == id);
            if (emp == null)
            {
                return NotFound();
            }

            return View(emp);
        }

        // GET: Emps/Create
        public IActionResult Create()
        {
            ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "No");
            ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "No");
            return View();
        }

        // POST: Emps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("No,Name,RoleNo,DeptNo,HireDate,PasswordHash,PasswordSalt,EmpCode,Status")] Emp emp)
        {
            if (ModelState.IsValid)
            {
                _context.Add(emp);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "No", emp.DeptNo);
            ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "No", emp.RoleNo);
            return View(emp);
        }

        // GET: Emps/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emp = await _context.Emps.FindAsync(id);
            if (emp == null)
            {
                return NotFound();
            }
            ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "No", emp.DeptNo);
            ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "No", emp.RoleNo);
            return View(emp);
        }

        // POST: Emps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("No,Name,RoleNo,DeptNo,HireDate,PasswordHash,PasswordSalt,EmpCode,Status")] Emp emp)
        {
            if (id != emp.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(emp);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpExists(emp.No))
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
            ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "No", emp.DeptNo);
            ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "No", emp.RoleNo);
            return View(emp);
        }

        // GET: Emps/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emp = await _context.Emps
                .Include(e => e.DeptNoNavigation)
                .Include(e => e.RoleNoNavigation)
                .FirstOrDefaultAsync(m => m.No == id);
            if (emp == null)
            {
                return NotFound();
            }

            return View(emp);
        }

        // POST: Emps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var emp = await _context.Emps.FindAsync(id);
            if (emp != null)
            {
                _context.Emps.Remove(emp);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmpExists(string id)
        {
            return _context.Emps.Any(e => e.No == id);
        }
    }
}
