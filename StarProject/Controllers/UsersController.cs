using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarProject.Attributes;
using StarProject.DTOs.UsersDTOs;
using StarProject.Helpers;
using StarProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
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
		public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
		{
			var query = _context.Users.AsQueryable();

			// 總筆數
			var totalCount = await query.CountAsync();

			// 分頁資料
			var result = await query
				.OrderBy(u => u.Account)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(u => new UsersDTO
				{
					Id = u.No,
					Account = u.Account,
					Name = u.Name,
					Phone = u.Phone,
					Email = u.Email,
					Address = u.Address,
					Status = (UsersStatus)u.Status,
					StatusText = ((UsersStatus)u.Status).GetDisplayName()
				})
				.ToListAsync();

			// 確保設定 ViewBag.Page
			ViewBag.Page = page;  // 這行很重要
			ViewBag.Total = totalCount;
			ViewBag.CurrentPage = page;
			ViewBag.PageSize = pageSize;
			ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
			ViewBag.HasPrevious = page > 1;
			ViewBag.HasNext = page < ViewBag.TotalPages;

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
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Suspend(string id)
		{
			var user = await _context.Users.FindAsync(id);
			if (user != null)
			{
				user.Status = (byte)UsersStatus.Suspended;
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Block(string id)
		{
			var user = await _context.Users.FindAsync(id);
			if (user != null)
			{
				user.Status = (byte)UsersStatus.Blocked;
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Activate(string id)
		{
			var user = await _context.Users.FindAsync(id);
			if (user != null)
			{
				user.Status = (byte)UsersStatus.Normal;
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> BatchSuspend(string[] ids)
		{
			if (ids == null || ids.Length == 0)
				return Json(new { success = false, message = "未選擇任何項目" });

			var users = await _context.Users.Where(u => ids.Contains(u.No)).ToListAsync();
			foreach (var user in users)
			{
				user.Status = (byte)UsersStatus.Suspended;
			}
			await _context.SaveChangesAsync();
			return Json(new { success = true, message = $"已成功停用 {users.Count} 個會員" });
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> BatchBlock(string[] ids)
		{
			if (ids == null || ids.Length == 0)
				return Json(new { success = false, message = "未選擇任何項目" });

			var users = await _context.Users.Where(u => ids.Contains(u.No)).ToListAsync();
			foreach (var user in users)
			{
				user.Status = (byte)UsersStatus.Blocked;
			}
			await _context.SaveChangesAsync();
			return Json(new { success = true, message = $"已成功封鎖 {users.Count} 個會員" });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> BatchActivate(string[] ids)
		{
			if (ids == null || ids.Length == 0)
				return Json(new { success = false, message = "未選擇任何項目" });

			var users = await _context.Users.Where(u => ids.Contains(u.No)).ToListAsync();
			foreach (var user in users)
			{
				user.Status = (byte)UsersStatus.Normal;
			}
			await _context.SaveChangesAsync();
			return Json(new { success = true, message = $"已成功啟用 {users.Count} 個會員" });
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

		// 新增支援自訂 ViewData 的方法
		private async Task<string> RenderPartialViewToStringWithViewData(string viewName, object model, ViewDataDictionary viewData)
		{
			if (string.IsNullOrEmpty(viewName))
			{
				viewName = ControllerContext.ActionDescriptor.ActionName;
			}

			using (var writer = new StringWriter())
			{
				IViewEngine viewEngine = HttpContext.RequestServices.GetService<ICompositeViewEngine>();
				ViewEngineResult viewResult = viewEngine.FindView(ControllerContext, viewName, false);

				if (viewResult.Success == false)
				{
					return $"找不到視圖 {viewName}";
				}

				// 使用傳入的 viewData，如果是 null 則使用現有的
				var contextViewData = viewData ?? ViewData;
				contextViewData.Model = model;

				ViewContext viewContext = new ViewContext(
					ControllerContext,
					viewResult.View,
					contextViewData,
					TempData,
					writer,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync(viewContext);

				return writer.GetStringBuilder().ToString();
			}
		}
		[HttpPost]
		public async Task<IActionResult> SearchSelect([FromBody] UsersFilterDTO filters)
		{
			var query = _context.Users.AsQueryable();

			// 關鍵字搜尋
			if (!string.IsNullOrEmpty(filters.Keyword))
			{
				query = query.Where(u => u.Account.Contains(filters.Keyword) ||
										u.Name.Contains(filters.Keyword) ||
										u.Phone.Contains(filters.Keyword) ||
										u.Email.Contains(filters.Keyword));
			}

			// 狀態篩選
			if (filters.Categories != null && filters.Categories.Any())
			{
				var statusValues = new List<byte>();
				foreach (var category in filters.Categories)
				{
					switch (category)
					{
						case "正常":
							statusValues.Add((byte)UsersStatus.Normal);
							break;
						case "停用":
							statusValues.Add((byte)UsersStatus.Suspended);
							break;
						case "封鎖":
							statusValues.Add((byte)UsersStatus.Blocked);
							break;
					}
				}
				if (statusValues.Any())
				{
					query = query.Where(u => statusValues.Contains(u.Status));
				}
			}

			// 日期篩選 (假設您有創建日期或其他日期欄位)
			// 如果您的 User 模型沒有日期欄位，可以移除這段
			/*
			if (filters.DateFrom.HasValue)
			{
				query = query.Where(u => u.CreateDate >= filters.DateFrom.Value);
			}
			if (filters.DateTo.HasValue)
			{
				query = query.Where(u => u.CreateDate <= filters.DateTo.Value.Date.AddDays(1).AddSeconds(-1));
			}
			*/

			var totalCount = await query.CountAsync();

			var result = await query
				.OrderBy(u => u.Account)
				.Skip((filters.Page - 1) * filters.PageSize)
				.Take(filters.PageSize)
				.Select(u => new UsersDTO
				{
					Id = u.No,
					Account = u.Account,
					Name = u.Name,
					Phone = u.Phone,
					Email = u.Email,
					Address = u.Address,
					Status = (UsersStatus)u.Status,
					StatusText = ((UsersStatus)u.Status).GetDisplayName()
				})
				.ToListAsync();

			// 設定所有必要的 ViewBag
			ViewBag.Total = totalCount;
			ViewBag.Page = filters.Page;
			ViewBag.CurrentPage = filters.Page;
			ViewBag.PageSize = filters.PageSize;
			ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize);
			ViewBag.HasPrevious = filters.Page > 1;
			ViewBag.HasNext = filters.Page < ViewBag.TotalPages;

			try
			{
				// 渲染部分視圖
				var tableHtml = await RenderPartialViewToString("_UsersRows", result);
				var paginationHtml = await RenderPartialViewToString("_Pagination", result);

				return Json(new
				{
					success = true,
					tableHtml = tableHtml,
					paginationHtml = paginationHtml,
					totalCount = totalCount
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "渲染視圖時發生錯誤: " + ex.Message
				});
			}
		}
		// 保留原來的方法以維持相容性
		private async Task<string> RenderPartialViewToString(string viewName, object model)
		{
			return await RenderPartialViewToStringWithViewData(viewName, model, null);
		}
		private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.No == id);
        }
    }
}
