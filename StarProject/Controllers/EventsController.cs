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
using System.Data.SqlTypes;


namespace StarProject.Controllers
{
	public class EventsController : Controller
	{
		private static readonly DateTime SqlMin = SqlDateTime.MinValue.Value;
		private const int DefaultPageSize = 10;

		private readonly StarProjectContext _context;
		public EventsController(StarProjectContext context)
		{
			_context = context;
		}

		private IQueryable<Event> VisibleEventsQuery(string? status, string? keyword)
		{
			var now = DateTime.Now;

			// 只取在「列表」應顯示的活動
			var q = _context.Events.AsNoTracking()
				.Where(e =>
					// 沒有「未來才要發佈、且尚未執行」的排程
					!_context.Schedules.Any(s =>
						s.EventNo == e.No &&
						s.Executed == false &&
						s.ReleaseDate > now
					)
					&&
					// 沒有「已過期」的排程（SqlMin 視為無期限）
					!_context.Schedules.Any(s =>
						s.EventNo == e.No &&
						s.ExpirationDate != SqlMin &&
						s.ExpirationDate <= now
					)
				);

			// 關鍵字
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				var kw = keyword.Trim();
				q = q.Where(e => e.Title.Contains(kw));
			}

			// 狀態
			if (!string.IsNullOrWhiteSpace(status))
			{
				var keys = NormalizeStatus(status);
				q = q.Where(e => keys.Contains(e.Status));
			}

			return q;
		}


		// =================== 活動總覽 (Dashboard) ==================
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var now = DateTime.Now;
			var firstDay = new DateTime(now.Year, now.Month, 1);
			var nextMonthFirst = firstDay.AddMonths(1);

			var visible = VisibleEventsQuery(status: null, keyword: null);
			var allEvents = await visible.OrderByDescending(e => e.CreatedTime).ToListAsync();


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
		public async Task<IActionResult> List(string? status, string? keyword, int page = 1, int pageSize = DefaultPageSize)
		{
			ViewBag.Status = status;
			ViewBag.Keyword = keyword;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			var q = VisibleEventsQuery(status, keyword)
					.OrderByDescending(x => x.StartDate)
					.ThenByDescending(x => x.CreatedTime);

			var (events, total, totalPages) = await PaginationHelper.PaginateAsync(q, page, pageSize);

			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			return View(events);
		}
		// AJAX 分頁：只回傳 rows（_EventRows）
		
		[HttpGet]
		public async Task<IActionResult> GetEvents(string? status, string? keyword, int page = 1, int pageSize = DefaultPageSize)
		{
			ViewBag.Status = status;   // 若 _EventRows 需要知道目前狀態，可用這個
			ViewBag.Keyword = keyword;

			var now = DateTime.Now;

			// 只取「目前應該可見」的活動
			var q = _context.Events.AsNoTracking()
				.Where(e =>
					// 尚未到發佈時間且未執行 → 隱藏
					!_context.Schedules.Any(s =>
						s.EventNo == e.No &&
						s.Executed == false &&
						s.ReleaseDate > now
					)
					&&
					// 已過期 → 隱藏（SqlMin 視為無期限）
					!_context.Schedules.Any(s =>
						s.EventNo == e.No &&
						s.ExpirationDate != SqlMin &&
						s.ExpirationDate <= now
					)
				);

			// 關鍵字
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				var kw = keyword.Trim();
				q = q.Where(e => e.Title.Contains(kw));
			}

			// 狀態
			if (!string.IsNullOrWhiteSpace(status))
			{
				var keys = NormalizeStatus(status);
				q = q.Where(e => keys.Contains(e.Status));
			}

			// 排序 + 分頁
			q = q.OrderByDescending(x => x.StartDate)
				 .ThenByDescending(x => x.CreatedTime);

			var (events, _, _) = await PaginationHelper.PaginateAsync(q, page, pageSize);
			return PartialView("_EventRows", events);
		}

		// =================== 私有工具 ==================
		private static string[] NormalizeStatus(string raw)
		{
			var s = (raw ?? "").Trim();
			return s switch
			{
				"報名中" or "Open" or "開放報名" => new[] { "報名中", "Open", "開放報名" },
				"已結束" or "Ended" or "Closed" => new[] { "已結束", "Ended", "Closed" },
				"已取消" or "Cancelled" or "Canceled" => new[] { "已取消", "Cancelled", "Canceled" },
				"已額滿" or "Full" => new[] { "已額滿", "Full" },
				_ => new[] { s }
			};
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
					schedule.ExpirationDate = ExpirationDate.HasValue ? TrimToMinute(ExpirationDate.Value) : SqlMin;
					schedule.Executed = true;
				}
				else
				{
					var rel = ReleaseDate.HasValue ? TrimToMinute(ReleaseDate.Value) : nowMin;
					schedule.ReleaseDate = rel;
					schedule.ExpirationDate = ExpirationDate.HasValue ? TrimToMinute(ExpirationDate.Value) : SqlMin;
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
			ViewBag.ScheduleExpirationDate = (s != null && s.ExpirationDate > SqlMin)
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
					s.ExpirationDate = ExpirationDate.HasValue ? TrimToMinute(ExpirationDate.Value) : SqlMin;
					s.Executed = true;
				}
				else
				{
					var rel = ReleaseDate.HasValue ? TrimToMinute(ReleaseDate.Value) : nowMin;
					s.ReleaseDate = rel;
					s.ExpirationDate = ExpirationDate.HasValue ? TrimToMinute(ExpirationDate.Value) : SqlMin;
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
			if (ids == null || ids.Length == 0)
				return Ok(new { deleted = 0, message = "未選取任何資料" });

			// 以交易確保一致性
			using var tx = await _context.Database.BeginTransactionAsync();
			try
			{
				// 取出要刪的活動
				var events = await _context.Events
					.Where(e => ids.Contains(e.No))
					.ToListAsync();

				if (events.Count == 0)
					return Ok(new { deleted = 0, message = "找不到對應的活動" });

				// ==== 先刪相依資料（若你的專案有下列表，依實際名稱調整）====
				// Participants
				if (_context.Participants != null)
				{
					var participants = _context.Participants.Where(p => ids.Contains(p.EventNo));
					_context.Participants.RemoveRange(participants);
				}

				// Schedules
				if (_context.Schedules != null)
				{
					var schedules = _context.Schedules.Where(s => ids.Contains(s.EventNo));
					_context.Schedules.RemoveRange(schedules);
				}

				// EventNotif（排程/通知紀錄）
				// 將 _context.EventNotif 改為 _context.EventNotifs
				if (_context.EventNotifs != null)
				{
					var notifs = _context.EventNotifs.Where(n => ids.Contains(n.EventNo));
					_context.EventNotifs.RemoveRange(notifs);
				}
				



				// 最後刪主表 Events
				_context.Events.RemoveRange(events);

				var affected = await _context.SaveChangesAsync();
				await tx.CommitAsync();

				// 回覆刪掉了幾筆活動（不是受影響總列數）
				return Ok(new { deleted = events.Count, message = $"已刪除 {events.Count} 筆活動" });
			}
			catch (DbUpdateException dbEx)
			{
				await tx.RollbackAsync();
				// 常見：外鍵限制導致無法刪除
				return StatusCode(409, "刪除失敗：存在相依資料（報名/排程/通知/金流等），請先移除相依資料或啟用級聯刪除。");
			}
			catch (Exception ex)
			{
				await tx.RollbackAsync();
				return StatusCode(500, $"刪除失敗：{ex.Message}");
			}
		}


		// =================== 進階搜尋（清單頁） ==================
		// 使用你的 SearchFilterVM（含 keyword/Categories/Statuses/(Locations)/DateFrom/DateTo/Page/PageSize）
		
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> SearchSelect([FromBody] SearchFilterVM filters)
		{
			filters ??= new SearchFilterVM();

			int page = filters.Page > 0 ? filters.Page : 1;
			int pageSize = filters.PageSize > 0 ? filters.PageSize : DefaultPageSize;

			var q = _context.Events.AsNoTracking()
				.Where(e =>
					!_context.Schedules.Any(s => s.EventNo == e.No && s.Executed == false && s.ReleaseDate > DateTime.Now) &&
					!_context.Schedules.Any(s => s.EventNo == e.No && s.ExpirationDate != SqlMin && s.ExpirationDate <= DateTime.Now)
				);

			if (!string.IsNullOrWhiteSpace(filters.keyword))
			{
				var kw = filters.keyword.Trim();
				q = q.Where(e =>
					e.Title.Contains(kw) ||
					e.Category.Contains(kw) ||
					e.Location.Contains(kw) ||
					(e.Desc != null && e.Desc.Contains(kw)));
			}

			if (filters.Categories?.Any() == true)
				q = q.Where(e => filters.Categories.Contains(e.Category));

			// C. 狀態過濾（建議寫法）
			var statuses = filters.Statuses?
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => s.Trim())
				.ToList() ?? new List<string>();

			if (statuses.Any())
			{
				// 先把使用者輸入的每個狀態展開成你系統支援的同義值（NormalizeStatus）
				// e.g. "報名中" → {"報名中","Open","開放報名"}
				var keys = statuses
					.SelectMany(NormalizeStatus)
					.Where(s => !string.IsNullOrWhiteSpace(s))
					.Distinct()
					.ToArray();

				if (keys.Length > 0)
				{
					// ✅ 只在常數集合上做 Contains，讓 EF 轉成 SQL 的 IN (...)，索引友善
					q = q.Where(e => keys.Contains(e.Status));
				}
			}



			var ci = System.Globalization.CultureInfo.InvariantCulture;
			if (!string.IsNullOrWhiteSpace(filters.DateFrom) &&
				DateTime.TryParseExact(filters.DateFrom, "yyyy-MM-dd", ci, System.Globalization.DateTimeStyles.None, out var from))
			{
				q = q.Where(e => e.StartDate >= from);
			}
			if (!string.IsNullOrWhiteSpace(filters.DateTo) &&
				DateTime.TryParseExact(filters.DateTo, "yyyy-MM-dd", ci, System.Globalization.DateTimeStyles.None, out var to))
			{
				var end = to.Date.AddDays(1).AddTicks(-1);
				q = q.Where(e => e.StartDate <= end);
			}

			q = q.OrderByDescending(e => e.StartDate)
				 .ThenByDescending(e => e.CreatedTime);

			// 先算總數與頁數，再「夾住」page
			var total = await q.CountAsync();
			var totalPages = total == 0 ? 1 : (int)Math.Ceiling(total / (double)pageSize);
			if (page > totalPages) page = totalPages;
			if (page < 1) page = 1;

			var items = await q.Skip((page - 1) * pageSize)
							   .Take(pageSize)
							   .ToListAsync();

			// 這些給分頁 partial / 前端使用
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages; // 已保證至少 1
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			// 回填目前條件（讓前端重繪後仍保留）
			var firstStatus = filters.Statuses?.FirstOrDefault() ?? "";
			ViewBag.Status = firstStatus;
			ViewBag.Keyword = filters.keyword ?? "";

			var tableHtml = await RenderPartialViewToString("_EventRows", items);
			var paginationHtml = await RenderPartialViewToString("_EventPagination", null);

			return Json(new
			{
				tableHtml,
				paginationHtml,
				total,
				totalPages,
				page,
				pageSize,
				status = firstStatus,
				keyword = filters.keyword ?? ""
			});
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

		// =================== 檢視排程中的活動（Modal 會用） ==================
		[HttpGet]
		public async Task<IActionResult> GetScheduled(int withinDays = 180)
		{
			var now = DateTime.Now;
			var until = now.AddDays(withinDays);

			// 「目前排程中的活動」定義：
			// 1) 尚未到發布時間（未執行），或
			// 2) 已經到發布時間但仍在有效期限內（未過期，且標記未執行）
			//    —— 視你的排程器行為而定，這裡以「ReleaseDate >= 現在 且 未過期」為主，並忽略 Executed=true 的已處理案件
			var list = await (
				from s in _context.Schedules.AsNoTracking()
				join e in _context.Events.AsNoTracking() on s.EventNo equals e.No
				where
					// 發佈時間在未來（常見情境）
					s.ReleaseDate > now
					// 還沒過期（ExpirationDate==SqlMin 視為無期限）
					&& (s.ExpirationDate == SqlMin || s.ExpirationDate > now)
					// 只看尚未執行（若你的排程服務會把執行後改為 true）
					&& (s.Executed == false)
					// 可選：限制最遠查詢範圍，避免資料量太大
					&& s.ReleaseDate <= until
				orderby s.ReleaseDate ascending
				select new
				{
					e.No,
					e.Title,
					e.Category,
					e.Location,
					e.Status,
					e.Image,
					s.ReleaseDate,
					s.ExpirationDate
				}
			).ToListAsync();

			return Json(list);
		}

		[HttpGet]
		public async Task<IActionResult> GetScheduledCount()
		{
			var now = DateTime.Now;
			var count = await _context.Schedules
				.AsNoTracking()
				.Where(s =>
					s.ReleaseDate > now &&
					(s.ExpirationDate == SqlMin || s.ExpirationDate > now) &&
					(s.Executed == false))
				.CountAsync();

			return Json(new { count });
		}


		// =================== 私有工具 ==================
		private static DateTime TrimToMinute(DateTime dt)
			=> new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);

		// 供 AJAX 回傳 Partial HTML 用 
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
