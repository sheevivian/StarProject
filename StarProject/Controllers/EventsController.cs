using Microsoft.AspNetCore.Http; // IFormFile
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StarProject.Helpers;
using StarProject.Models;
using StarProject.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
	public class EventsController : Controller
	{
		private const int DefaultPageSize = 10;

		private readonly StarProjectContext _context;
		public EventsController(StarProjectContext context)
		{
			_context = context;
		}

		// =================== 活動總覽 (Dashboard) ==================
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var now = DateTime.Now;
			var firstDay = new DateTime(now.Year, now.Month, 1);
			var nextMonthFirst = firstDay.AddMonths(1);

			var allEvents = await _context.Events
				.AsNoTracking()
				.OrderByDescending(e => e.CreatedTime)
				.ToListAsync();

			// 統計
			ViewBag.TotalEvents = allEvents.Count;
			ViewBag.TotalParticipants = await _context.Participants.CountAsync();
			ViewBag.TotalEndedEvents = allEvents.Count(e => e.Status == "已結束");
			ViewBag.TotalOpenEvents = allEvents.Count(e => e.Status == "報名中");
			ViewBag.TotalCancelledEvents = allEvents.Count(e => e.Status == "已取消");

			// 本月活動（給白底表格）
			ViewBag.ThisMonthEvents = allEvents
				.Where(e => e.StartDate >= firstDay && e.StartDate < nextMonthFirst)
				.OrderBy(e => e.StartDate)
				.Select(e => new { e.No, e.Title, e.StartDate, e.Status, e.MaxParticipants })
				.ToList();

			// 7 天內即將到來（上方提醒）
			ViewBag.UpcomingEvents = allEvents
				.Where(e => e.StartDate > now && e.StartDate <= now.AddDays(7))
				.OrderBy(e => e.StartDate)
				.Take(5)
				.Select(e => new { e.No, e.Title, e.StartDate })
				.ToList();

			// 報名成功人數（進度條用）
			ViewBag.RegsByEvent = await _context.Participants
				.AsNoTracking()
				.Where(p => p.Status == "報名成功" || p.Status == "Success")
				.GroupBy(p => p.EventNo)
				.Select(g => new { EventNo = g.Key, Count = g.Count() })
				.ToDictionaryAsync(x => x.EventNo, x => x.Count);

			return View(allEvents);
		}

		// =================== 活動清單 (List + 分頁) ==================
		[HttpGet]
		public async Task<IActionResult> List(int page = 1, int pageSize = DefaultPageSize)
		{
			var query = _context.Events
								.OrderByDescending(x => x.StartDate)
								.ThenByDescending(x => x.CreatedTime);

			var (events, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			return View(events);
		}

		// AJAX 分頁：只回傳 rows（_EventRows）
		[HttpGet]
		public async Task<IActionResult> GetEvents(int page = 1, int pageSize = DefaultPageSize)
		{
			var query = _context.Events
								.OrderByDescending(x => x.StartDate)
								.ThenByDescending(x => x.CreatedTime);

			var (events, _, _) = await PaginationHelper.PaginateAsync(query, page, pageSize);
			return PartialView("_EventRows", events);
		}

		// =================== 詳情 ==================
		[HttpGet]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();
			var ev = await _context.Events.FirstOrDefaultAsync(e => e.No == id);
			if (ev == null) return NotFound();
			return View(ev);
		}

		// =================== 新增 ==================
		[HttpGet]
		public IActionResult Create()
		{
			var vm = new EventInfoVM
			{
				StartDate = DateTime.Now,
				EndDate = DateTime.Now,
				Image = "/img/logo.png"
			};

			ViewBag.ScheduleReleaseDate = "";
			ViewBag.ScheduleExpirationDate = "";
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			EventInfoVM vm,
			string PublishMode,
			DateTime? ReleaseDate,
			DateTime? ExpirationDate)
		{
			if (!ModelState.IsValid) return View(vm);

			try
			{
				string imageUrl = "/img/logo.png";
				if (vm.ImageFile != null && vm.ImageFile.Length > 0)
				{
					imageUrl = await ImgUploadHelper.UploadToImgBB(vm.ImageFile);
					if (string.IsNullOrWhiteSpace(imageUrl))
						throw new InvalidOperationException("圖片上傳失敗（回傳空 URL）。");
				}

				var start = TrimToMinute(vm.StartDate);
				var end = vm.EndDate.HasValue ? TrimToMinute(vm.EndDate.Value) : (DateTime?)null;

				var entity = new Event
				{
					Title = vm.Title,
					Category = vm.Category,
					Desc = vm.Desc,
					Status = vm.Status,
					StartDate = start,
					EndDate = end,
					CreatedTime = DateTime.Now,
					UpdatedTime = DateTime.Now,
					Location = vm.Location,
					MaxParticipants = vm.MaxParticipants,
					Fee = vm.Fee,
					Deposit = vm.Deposit,
					Image = imageUrl
				};

				_context.Events.Add(entity);
				await _context.SaveChangesAsync();

				// 排程
				var nowMin = TrimToMinute(DateTime.Now);
				var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.EventNo == entity.No)
							   ?? new Schedule { EventNo = entity.No };

				if (string.Equals(PublishMode, "now", StringComparison.OrdinalIgnoreCase))
				{
					schedule.ReleaseDate = nowMin;
					schedule.ExpirationDate = ExpirationDate.HasValue ? TrimToMinute(ExpirationDate.Value) : DateTime.MinValue;
					schedule.Executed = true;
				}
				else
				{
					var rel = ReleaseDate.HasValue ? TrimToMinute(ReleaseDate.Value) : nowMin;
					schedule.ReleaseDate = rel;
					schedule.ExpirationDate = ExpirationDate.HasValue ? TrimToMinute(ExpirationDate.Value) : DateTime.MinValue;
					schedule.Executed = rel <= nowMin;
				}

				if (_context.Entry(schedule).State == EntityState.Detached)
					_context.Schedules.Add(schedule);

				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(List));
			}
			catch (DbUpdateException dbx)
			{
				ModelState.AddModelError("", "資料庫寫入失敗：" + (dbx.InnerException?.Message ?? dbx.Message));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "上傳圖片或儲存失敗：" + ex.Message);
			}

			ViewBag.ScheduleReleaseDate = ReleaseDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
			ViewBag.ScheduleExpirationDate = ExpirationDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
			return View(vm);
		}

		// =================== 編輯 ==================
		[HttpGet]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var entity = await _context.Events.FindAsync(id);
			if (entity == null) return NotFound();

			var evm = new EventInfoVM
			{
				No = entity.No,
				Title = entity.Title,
				Category = entity.Category,
				Location = entity.Location,
				Desc = entity.Desc,
				StartDate = entity.StartDate,
				EndDate = entity.EndDate,
				CreatedTime = entity.CreatedTime,
				UpdatedTime = entity.UpdatedTime,
				MaxParticipants = entity.MaxParticipants,
				Status = entity.Status,
				Fee = entity.Fee,
				Deposit = entity.Deposit,
				Image = entity.Image,
			};

			var s = await _context.Schedules.AsNoTracking().FirstOrDefaultAsync(x => x.EventNo == entity.No);
			ViewBag.ScheduleReleaseDate = s?.ReleaseDate.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
			ViewBag.ScheduleExpirationDate = (s != null && s.ExpirationDate > DateTime.MinValue)
				? s.ExpirationDate.ToString("yyyy-MM-ddTHH\\:mm")
				: "";

			return View(evm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(
			int id,
			[Bind("No,Title,Category,Desc,Location,StartDate,EndDate,MaxParticipants,Fee,Deposit,Status,ImageFile")]
			EventInfoVM evm,
			string PublishMode,
			DateTime? ReleaseDate,
			DateTime? ExpirationDate)
		{
			if (id != evm.No) return NotFound();

			var original = await _context.Events.AsNoTracking().FirstOrDefaultAsync(x => x.No == id);
			if (original == null) return NotFound();

			if (!ModelState.IsValid)
			{
				ViewBag.ScheduleReleaseDate = ReleaseDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
				ViewBag.ScheduleExpirationDate = ExpirationDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
				return View(evm);
			}

			try
			{
				var entity = await _context.Events.FindAsync(id);
				if (entity == null) return NotFound();

				// 圖片
				string imageUrl = original.Image ?? "/img/logo.png";
				if (evm.ImageFile != null && evm.ImageFile.Length > 0)
				{
					var uploadedUrl = await ImgUploadHelper.UploadToImgBB(evm.ImageFile);
					if (string.IsNullOrWhiteSpace(uploadedUrl))
					{
						ModelState.AddModelError("ImageFile", "圖片上傳失敗，請改用較小的圖片或稍後再試。");
						ViewBag.ScheduleReleaseDate = ReleaseDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
						ViewBag.ScheduleExpirationDate = ExpirationDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
						return View(evm);
					}
					imageUrl = uploadedUrl;
				}

				// 欄位
				entity.Title = evm.Title;
				entity.Category = evm.Category;
				entity.Location = evm.Location;
				entity.Desc = evm.Desc;
				entity.StartDate = TrimToMinute(evm.StartDate);
				entity.EndDate = evm.EndDate.HasValue ? TrimToMinute(evm.EndDate.Value) : (DateTime?)null;
				entity.CreatedTime = original.CreatedTime;
				entity.UpdatedTime = DateTime.Now;
				entity.MaxParticipants = evm.MaxParticipants;
				entity.Status = evm.Status;
				entity.Fee = evm.Fee;
				entity.Deposit = evm.Deposit;
				entity.Image = imageUrl;

				await _context.SaveChangesAsync();

				// 排程
				var nowMin = TrimToMinute(DateTime.Now);
				var s = await _context.Schedules.FirstOrDefaultAsync(x => x.EventNo == id)
						?? new Schedule { EventNo = id };

				if (string.Equals(PublishMode, "now", StringComparison.OrdinalIgnoreCase))
				{
					s.ReleaseDate = nowMin;
					s.ExpirationDate = ExpirationDate.HasValue ? TrimToMinute(ExpirationDate.Value) : DateTime.MinValue;
					s.Executed = true;
				}
				else
				{
					var rel = ReleaseDate.HasValue ? TrimToMinute(ReleaseDate.Value) : nowMin;
					s.ReleaseDate = rel;
					s.ExpirationDate = ExpirationDate.HasValue ? TrimToMinute(ExpirationDate.Value) : DateTime.MinValue;
					s.Executed = rel <= nowMin;
				}

				if (_context.Entry(s).State == EntityState.Detached)
					_context.Schedules.Add(s);

				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(List));
			}
			catch (DbUpdateException dbx)
			{
				ModelState.AddModelError("", "資料庫更新失敗：" + (dbx.InnerException?.Message ?? dbx.Message));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "更新失敗：" + ex.Message);
			}

			ViewBag.ScheduleReleaseDate = ReleaseDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
			ViewBag.ScheduleExpirationDate = ExpirationDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
			return View(evm);
		}

		// =================== 刪除 ==================
		[HttpGet]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();
			var ev = await _context.Events.FirstOrDefaultAsync(e => e.No == id);
			if (ev == null) return NotFound();
			return View(ev);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var ev = await _context.Events.FindAsync(id);
			if (ev != null)
			{
				_context.Events.Remove(ev);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(List));
		}

		// 多筆刪除（清單右上角垃圾桶）
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteMultiple([FromForm] int[] ids)
		{
			if (ids == null || ids.Length == 0) return Ok();
			var events = await _context.Events.Where(e => ids.Contains(e.No)).ToListAsync();
			if (events.Any())
			{
				_context.Events.RemoveRange(events);
				await _context.SaveChangesAsync();
			}
			return Ok();
		}

		// =================== 進階搜尋（清單頁） ==================
		// 使用你的 SearchFilterVM（含 keyword/Categories/Statuses/(Locations)/DateFrom/DateTo/Page/PageSize）
		[HttpPost]
		[IgnoreAntiforgeryToken] // 若要驗證 CSRF：改成 [ValidateAntiForgeryToken] 並在前端加 RequestVerificationToken header
		public async Task<IActionResult> SearchSelect([FromBody] SearchFilterVM filters)
		{
			filters ??= new SearchFilterVM();

			int page = filters.Page > 0 ? filters.Page : 1;
			int pageSize = filters.PageSize > 0 ? filters.PageSize : DefaultPageSize;

			var q = _context.Events.AsQueryable();

			// 排序（與 List 一致）
			q = q.OrderByDescending(e => e.StartDate)
				 .ThenByDescending(e => e.CreatedTime);

			// 關鍵字
			if (!string.IsNullOrWhiteSpace(filters.keyword))
			{
				var kw = filters.keyword.Trim();
				q = q.Where(e =>
					e.Title.Contains(kw) ||
					e.Category.Contains(kw) ||
					e.Location.Contains(kw) ||
					(e.Desc != null && e.Desc.Contains(kw)));
			}

			// 類別
			if (filters.Categories != null && filters.Categories.Any())
				q = q.Where(e => filters.Categories.Contains(e.Category));

			// 狀態
			if (filters.Statuses != null && filters.Statuses.Any())
				q = q.Where(e => filters.Statuses.Contains(e.Status));


			// 日期區間（StartDate）
			if (!string.IsNullOrWhiteSpace(filters.DateFrom) && DateTime.TryParse(filters.DateFrom, out var from))
				q = q.Where(e => e.StartDate >= from.Date);
			if (!string.IsNullOrWhiteSpace(filters.DateTo) && DateTime.TryParse(filters.DateTo, out var to))
				q = q.Where(e => e.StartDate <= to.Date.AddDays(1).AddTicks(-1));

			// ✅ 呼叫分頁工具（與隊友一致）
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(q, page, pageSize);

			// 分頁資訊給 Partial 用
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			// 轉成 rows 與 pagination 的 HTML
			var tableHtml = await RenderPartialViewToString("_EventRows", items);
			var paginationHtml = await RenderPartialViewToString("_EventPagination", null);

			return Json(new { tableHtml, paginationHtml });
		}

		// =================== 報名管理 ==================
		[HttpGet]
		[HttpGet]
		public IActionResult Participants(int? selectedEventId, string searchUserNo)
		{
			// 直接交回 ParticipantsController.Index，由它統一組 ParticipantsIndexVm
			return RedirectToAction(
				actionName: "Index",
				controllerName: "Participants",
				routeValues: new
				{
					selectedEventId = selectedEventId ?? 0,
					searchEventKeyword = "",                 // 若未使用可留空字串
					searchUserKeyword = searchUserNo ?? ""   // 對應 ParticipantsController.Index 的參數名稱
				});
		}

		// =================== 私有工具 ==================
		private static DateTime TrimToMinute(DateTime dt)
			=> new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);

		// 供 AJAX 回傳 Partial HTML 用（和隊友相同做法）
		public async Task<string> RenderPartialViewToString(string viewName, object model)
		{
			if (string.IsNullOrEmpty(viewName))
				viewName = ControllerContext.ActionDescriptor.ActionName;

			ViewData.Model = model;

			using var sw = new StringWriter();
			var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
			var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

			if (!viewResult.Success)
				throw new ArgumentNullException($"View {viewName} not found.");

			var viewContext = new ViewContext(
				ControllerContext,
				viewResult.View,
				ViewData,
				TempData,
				sw,
				new HtmlHelperOptions()
			);

			await viewResult.View.RenderAsync(viewContext);
			return sw.GetStringBuilder().ToString();
		}
	}
}
