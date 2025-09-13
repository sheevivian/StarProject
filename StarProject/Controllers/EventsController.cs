using Microsoft.AspNetCore.Http; // IFormFile
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarProject.Helpers;
using StarProject.Models;
using StarProject.ViewModel;  // 確認 namespace 名稱正確
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
	public class EventsController : Controller
	{
		private readonly StarProjectContext _context;

		// 只保留到「分鐘」的工具方法
		private static DateTime TrimToMinute(DateTime dt)
			=> new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);

		// 在 EventsController 類別裡新增
		private async Task<string> UploadWithTimeoutOrFallbackAsync(IFormFile? file, string fallbackUrl, int timeoutSeconds = 15)
		{
			if (file == null || file.Length == 0) return fallbackUrl;

			if (file.ContentType is not ("image/jpeg" or "image/png" or "image/webp"))
				return fallbackUrl;

			var uploadTask = ImgUploadHelper.UploadToImgBB(file);
			var completed = await Task.WhenAny(uploadTask, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)));
			if (completed != uploadTask) return fallbackUrl;
			var url = await uploadTask;
			return string.IsNullOrWhiteSpace(url) ? fallbackUrl : url;
		}

		public EventsController(StarProjectContext context)
		{
			_context = context;
		}

		// =================== 活動總覽 (Index) ==================
		public async Task<IActionResult> Index()
		{
			var allEvents = await _context.Events
										  .OrderByDescending(e => e.CreatedTime)
										  .ToListAsync();

			var totalParticipants = await _context.Participants.CountAsync();

			ViewBag.TotalEvents = allEvents.Count;
			ViewBag.TotalParticipants = totalParticipants;
			ViewBag.TotalEndedEvents = allEvents.Count(e => e.Status == "已結束");
			ViewBag.TotalOpenEvents = allEvents.Count(e => e.Status == "報名中");
			ViewBag.TotalCancelledEvents = allEvents.Count(e => e.Status == "已取消");

			var now = DateTime.Now;
			ViewBag.ThisMonthEvents = allEvents
									  .Where(e => e.StartDate != null &&
												  e.StartDate.Month == now.Month &&
												  e.StartDate.Year == now.Year)
									  .OrderBy(e => e.StartDate)
									  .ToList();

			ViewBag.UpcomingEvents = allEvents
				.Where(e => e.StartDate != null && e.StartDate > now && e.StartDate <= now.AddDays(7))
				.OrderBy(e => e.StartDate)
				.Take(5)
				.ToList();

			return View(allEvents);
		}

		// =================== 活動列表 (List) ==================
		public async Task<IActionResult> List(string category, string status, int page = 1)
		{
			int pageSize = 10;

			IQueryable<Models.Event> events = _context.Events.AsQueryable();

			if (!string.IsNullOrEmpty(category))
				events = events.Where(e => e.Category == category);

			if (!string.IsNullOrEmpty(status))
				events = events.Where(e => e.Status == status);

			var categories = await _context.Events
				.Select(e => e.Category)
				.Distinct()
				.OrderBy(c => c)
				.ToListAsync();

			ViewBag.StatusList = await _context.Events
				.Select(e => e.Status)
				.Distinct()
				.OrderBy(s => s)
				.ToListAsync();

			ViewBag.CurrentCategory = category;
			ViewBag.CurrentStatus = status;
			ViewBag.Categories = categories;
			ViewBag.CategoriesSelect = new SelectList(categories, category);

			var eventList = await events
				.OrderByDescending(e => e.StartDate)
				.ToListAsync();

			return View(eventList);
		}

		// =================== 活動詳情 (Details) ==================
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();

			var ev = await _context.Events.FirstOrDefaultAsync(e => e.No == id);
			if (ev == null)
				return NotFound();

			return View(ev);
		}

		// =================== 新增活動 (Create) ==================
		[HttpGet]
		public IActionResult Create()
		{
			var vm = new EventInfoVM
			{
				StartDate = DateTime.Now,
				EndDate = DateTime.Now,
				Image = "/img/logo.png"
			};

			// 排程：初次建立預設空白（對應 View 的 value 綁定）
			ViewBag.ScheduleReleaseDate = "";
			ViewBag.ScheduleExpirationDate = "";

			return View(vm);
		}

		// POST: Events/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(EventInfoVM vm, string PublishMode, DateTime? ReleaseDate, DateTime? ExpirationDate)
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

				// ▼ 時間截到「分鐘」
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
				await _context.SaveChangesAsync(); // 取得 entity.No

				// ===== 排程：Upsert Schedule（時間也截到分鐘） =====
				var nowMin = TrimToMinute(DateTime.Now);
				var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.EventNo == entity.No)
							   ?? new Schedule { EventNo = entity.No };

				if (string.Equals(PublishMode, "now", StringComparison.OrdinalIgnoreCase))
				{
					schedule.ReleaseDate = nowMin;
					schedule.ExpirationDate = ExpirationDate.HasValue ? TrimToMinute(ExpirationDate.Value) : DateTime.MinValue; // 不下架用 MinValue
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
				// ================================

				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateException dbx)
			{
				ModelState.AddModelError("", "資料庫寫入失敗：" + (dbx.InnerException?.Message ?? dbx.Message));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "上傳圖片或儲存失敗：" + ex.Message);
			}

			// 失敗回填（維持原有排程輸入）
			ViewBag.ScheduleReleaseDate = ReleaseDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
			ViewBag.ScheduleExpirationDate = ExpirationDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
			return View(vm);
		}

		// =================== 編輯活動 (Edit) ==================
		[HttpGet]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var EventInfo = await _context.Events.FindAsync(id);
			if (EventInfo == null)
				return NotFound();

			// 將 Models.Event 轉成 ViewModel
			var evm = new EventInfoVM
			{
				No = EventInfo.No,
				Title = EventInfo.Title,
				Category = EventInfo.Category,
				Location = EventInfo.Location,
				Desc = EventInfo.Desc,
				StartDate = EventInfo.StartDate,
				EndDate = EventInfo.EndDate,
				CreatedTime = EventInfo.CreatedTime,
				UpdatedTime = EventInfo.UpdatedTime,
				MaxParticipants = EventInfo.MaxParticipants,
				Status = EventInfo.Status,
				Fee = EventInfo.Fee,
				Deposit = EventInfo.Deposit,
				Image = EventInfo.Image,
			};

			// 排程：載入現有 Schedule 值供畫面顯示
			var s = await _context.Schedules.AsNoTracking().FirstOrDefaultAsync(x => x.EventNo == EventInfo.No);
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
				// 回填排程欄位
				ViewBag.ScheduleReleaseDate = ReleaseDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
				ViewBag.ScheduleExpirationDate = ExpirationDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
				return View(evm);
			}

			try
			{
				var entity = await _context.Events.FindAsync(id);
				if (entity == null) return NotFound();

				// 1) 圖片處理
				string imageUrl = original.Image ?? "/img/logo.png";
				if (evm.ImageFile != null && evm.ImageFile.Length > 0)
				{
					var uploadedUrl = await ImgUploadHelper.UploadToImgBB(evm.ImageFile);
					if (string.IsNullOrWhiteSpace(uploadedUrl))
					{
						ModelState.AddModelError("ImageFile", "圖片上傳失敗，請改用較小的圖片或稍後再試。");
						// 回填排程欄位
						ViewBag.ScheduleReleaseDate = ReleaseDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
						ViewBag.ScheduleExpirationDate = ExpirationDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
						return View(evm);
					}
					imageUrl = uploadedUrl;
				}

				// 2) 更新活動欄位（時間截到分鐘）
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

				// ===== 排程：Upsert Schedule（時間也截到分鐘） =====
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
				// ================================

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

			// 回填排程欄位（失敗時）
			ViewBag.ScheduleReleaseDate = ReleaseDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
			ViewBag.ScheduleExpirationDate = ExpirationDate?.ToString("yyyy-MM-ddTHH\\:mm") ?? "";
			return View(evm);
		}

		private bool EventExists(int id)
		{
			return _context.Events.Any(e => e.No == id);
		}

		// =================== 刪除活動 (Delete) ==================
		[HttpGet]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
				return NotFound();

			var EventInfo = await _context.Events.FirstOrDefaultAsync(e => e.No == id);
			if (EventInfo == null)
				return NotFound();

			return View(EventInfo);
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

		// =================== 報名管理 (Participants) ==================
		public async Task<IActionResult> Participants(int? selectedEventId, string searchUserNo)
		{
			var allEvents = await _context.Events
										  .OrderBy(e => e.CreatedTime)
										  .ToListAsync();
			ViewBag.AllEvents = allEvents;

			var participantQuery = _context.Participants.AsQueryable();

			if (selectedEventId != null && selectedEventId != 0)
			{
				participantQuery = participantQuery.Where(p => p.EventNo == selectedEventId);
			}

			if (!string.IsNullOrEmpty(searchUserNo))
			{
				participantQuery = participantQuery.Where(p => p.UsersNo.Contains(searchUserNo));
			}

			var participants = await participantQuery
									.Include(p => p.EventNoNavigation)
									.Include(p => p.PaymentNoNavigation)
									.Include(p => p.UsersNoNavigation)
									.ToListAsync();

			ViewBag.SelectedEventId = selectedEventId ?? 0;
			ViewBag.SearchUserNo = searchUserNo;

			return View("~/Views/Participants/Index.cshtml", participants);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SearchSelect([FromBody] EventSearchFilter filter)
		{
			filter ??= new EventSearchFilter();

			var q = _context.Events.AsQueryable();

			if (!string.IsNullOrWhiteSpace(filter.Keyword))
			{
				var kw = filter.Keyword.Trim();
				q = q.Where(e =>
					e.Title.Contains(kw) ||
					e.Category.Contains(kw) ||
					e.Location.Contains(kw) ||
					(e.Desc != null && e.Desc.Contains(kw)));
			}

			if (filter.Categories?.Any() == true)
				q = q.Where(e => filter.Categories.Contains(e.Category));

			if (filter.Statuses?.Any() == true)
				q = q.Where(e => filter.Statuses.Contains(e.Status));

			if (filter.Locations?.Any() == true)
				q = q.Where(e => filter.Locations.Contains(e.Location));

			if (DateTime.TryParse(filter.DateFrom, out var from))
				q = q.Where(e => e.StartDate >= from);
			if (DateTime.TryParse(filter.DateTo, out var to))
			{
				to = to.Date.AddDays(1).AddTicks(-1);
				q = q.Where(e => e.StartDate <= to);
			}

			var list = await q.OrderByDescending(e => e.StartDate).ToListAsync();
			return PartialView("_EventRows", list);
		}

		public class EventSearchFilter
		{
			public string? Keyword { get; set; }
			public List<string>? Categories { get; set; }
			public List<string>? Statuses { get; set; }
			public List<string>? Locations { get; set; }
			public string? DateFrom { get; set; } // yyyy-MM-dd
			public string? DateTo { get; set; }   // yyyy-MM-dd
		}

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
	}
}
