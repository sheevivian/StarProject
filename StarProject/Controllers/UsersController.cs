using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarProject.Attributes;
using StarProject.DTOs.UsersDTOs;
using StarProject.Helpers;
using StarProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public class UsersController : Controller
    {
		[Permission("emp")]
		private readonly StarProjectContext _context;

        public UsersController(StarProjectContext context)
        {
            _context = context;
        }
		[Permission("emp")]
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

			var dto = new UserEditDTO
			{
				No = user.No,
				Account = user.Account,
				Name = user.Name,
				Phone = user.Phone,
				Email = user.Email,
				Address = user.Address,
				Status = (UsersStatus)user.Status
			};

			return View(dto);
		}

		public async Task<IActionResult> Edit(string id)
		{
			var user = await _context.Users.FindAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			var dto = new UserEditDTO
			{
				No = user.No,
				Account = user.Account,
				Name = user.Name,
				Phone = user.Phone,
				Email = user.Email,
				Address = user.Address,
				Status = (UsersStatus)user.Status
			};

			ViewData["StatusList"] = Enum.GetValues(typeof(UsersStatus))
				.Cast<UsersStatus>()
				.Select(s => new SelectListItem
				{
					Value = ((int)s).ToString(),
					Text = s.GetDisplayName(),
					Selected = (int)s == user.Status
				}).ToList();

			return View(dto);
		}



		// POST: Users/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, UserEditDTO dto)
		{
			if (id != dto.No)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				var user = await _context.Users.FindAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				// 只更新允許的欄位
				user.Account = dto.Account;
				user.Name = dto.Name;
				user.Phone = dto.Phone;
				user.Email = dto.Email;
				user.Address = dto.Address;
				user.Status = (byte)dto.Status;

				_context.Update(user);
				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(Index));
			}

			return View(dto);
		}


		private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.No == id);
        }
    }
}
