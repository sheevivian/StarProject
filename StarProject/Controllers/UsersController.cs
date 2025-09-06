using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarProject.Helpers;
using StarProject.Models;

namespace StarProject.Controllers
{
    public class UsersController : Controller
    {
        private readonly StarProjectContext _context;

        public UsersController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
			var result = await _context.Users
		.Select(u => new UsersDTO
		{
			Id = u.No,   // 對應資料表主鍵
			Account = u.Account,
			Name = u.Name,
			Phone = u.Phone,
			Email = u.Email,
			Address = u.Address,
			Status = (UsersStatus)u.Status,          // 轉成 Enum
			StatusText = ((UsersStatus)u.Status).GetDisplayName()
		})
		.ToListAsync();

			return View(result);
		}

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.No == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("No,Account,PasswordHash,PasswordSalt,Name,Phone,Email,Address,IdNumber,Status")] User user)
        {
            if (id != user.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.No))
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
            return View(user);
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.No == id);
        }
    }
}
