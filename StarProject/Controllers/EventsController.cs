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

		// 在 EventsController 類別裡新增
		private async Task<string> UploadWithTimeoutOrFallbackAsync(IFormFile? file, string fallbackUrl, int timeoutSeconds = 15)
		{
			if (file == null || file.Length == 0) return fallbackUrl;

			// 可選：基本檔案檢查（避免怪檔案卡超久）
			if (file.ContentType is not ("image/jpeg" or "image/png" or "image/webp"))
				return fallbackUrl; // 或者 ModelState.AddModelError 後 return fallbackUrl

			// 逾時保護：ImgBB 若沒回應，改用 fallbackUrl（不中斷整個儲存流程）
			var uploadTask = ImgUploadHelper.UploadToImgBB(file);
			var completed = await Task.WhenAny(uploadTask, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)));
			if (completed != uploadTask) return fallbackUrl;           // 逾時 → 保底
			var url = await uploadTask;                                // 成功
			return string.IsNullOrWhiteSpace(url) ? fallbackUrl : url; // 空字串也保底
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

			//1-1. 活動總數
			ViewBag.TotalEvents = allEvents.Count;
			//1-2. 總報名人數
			ViewBag.TotalParticipants = totalParticipants;
			//1-3. 狀態統計
			ViewBag.TotalEndedEvents = allEvents.Count(e => e.Status == "已結束");
			ViewBag.TotalOpenEvents = allEvents.Count(e => e.Status == "報名中");
			ViewBag.TotalCancelledEvents = allEvents.Count(e => e.Status == "已取消");

			//1-4. 本月活動
			var now = DateTime.Now;
			ViewBag.ThisMonthEvents = allEvents
									  .Where(e => e.StartDate != null &&
												  e.StartDate.Month == now.Month &&
												  e.StartDate.Year == now.Year)
									  .OrderBy(e => e.StartDate)
									  .ToList();

			//1-5. 即將開始的活動（未來 7 天）
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

			// 取得 IQueryable，延遲查詢
			IQueryable<Models.Event> events = _context.Events.AsQueryable();

			// 篩選類別
			if (!string.IsNullOrEmpty(category))
				events = events.Where(e => e.Category == category);

			// 篩選狀態
			if (!string.IsNullOrEmpty(status))
				events = events.Where(e => e.Status == status);

			// 活動類別清單 (給下拉選單)
			var categories = await _context.Events
				.Select(e => e.Category)
				.Distinct()
				.OrderBy(c => c)
				.ToListAsync();

			// 狀態清單 (給下拉選單)
			ViewBag.StatusList = await _context.Events
				.Select(e => e.Status)
				.Distinct()
				.OrderBy(s => s)
				.ToListAsync();

			// 保留目前選取的篩選值，方便前端顯示
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

			return View(vm);
			
		}

		// POST: LostInfo/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(EventInfoVM vm)
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

				var entity = new Event
				{
					Title = vm.Title,
					Category = vm.Category,
					Desc = vm.Desc,
					Status = vm.Status,
					StartDate = vm.StartDate,
					EndDate = vm.EndDate,           // ✅ 允許 null
					CreatedTime = DateTime.Now,
					UpdatedTime = DateTime.Now,
					Location = vm.Location,
					MaxParticipants = vm.MaxParticipants,
					Fee = vm.Fee,                   // ✅ 允許 null
					Deposit = vm.Deposit,           // ✅ 允許 null
					Image = imageUrl
				};

				_context.Events.Add(entity);
				await _context.SaveChangesAsync();
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

			return View(evm);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(
	int id,
	[Bind("No,Title,Category,Desc,Location,StartDate,EndDate,MaxParticipants,Fee,Deposit,Status,ImageFile")]
	EventInfoVM evm)
		{
			if (id != evm.No) return NotFound();

			var original = await _context.Events.AsNoTracking().FirstOrDefaultAsync(x => x.No == id);
			if (original == null) return NotFound();

			if (!ModelState.IsValid) return View(evm);

			try
			{
				var entity = await _context.Events.FindAsync(id);
				if (entity == null) return NotFound();

				// 1) 圖片處理：有新檔→上傳；失敗留在頁面顯示錯誤，不寫庫
				string imageUrl = original.Image ?? "/img/logo.png";
				if (evm.ImageFile != null && evm.ImageFile.Length > 0)
				{
					var uploadedUrl = await ImgUploadHelper.UploadToImgBB(evm.ImageFile);
					if (string.IsNullOrWhiteSpace(uploadedUrl))
					{
						ModelState.AddModelError("ImageFile", "圖片上傳失敗，請改用較小的圖片或稍後再試。");
						return View(evm);
					}
					imageUrl = uploadedUrl;
				}

				// 2) 更新欄位（不需要改主鍵 No）
				entity.Title = evm.Title;
				entity.Category = evm.Category;
				entity.Location = evm.Location;
				entity.Desc = evm.Desc;
				entity.StartDate = evm.StartDate;
				entity.EndDate = evm.EndDate;              // 可 null
				entity.CreatedTime = original.CreatedTime;     // 保留舊值
				entity.UpdatedTime = DateTime.Now;
				entity.MaxParticipants = evm.MaxParticipants;
				entity.Status = evm.Status;
				entity.Fee = evm.Fee;
				entity.Deposit = evm.Deposit;
				entity.Image = imageUrl;                 // ★ 修正重點：用 imageUrl

				await _context.SaveChangesAsync();

				// 3) 依你的需求導回 List，而不是 Index
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
			// 取得所有活動
			var allEvents = await _context.Events
										  .OrderBy(e => e.CreatedTime)
										  .ToListAsync();
			ViewBag.AllEvents = allEvents;

			// 取得報名資料
			var participantQuery = _context.Participants.AsQueryable();

			// 篩選活動
			if (selectedEventId != null && selectedEventId != 0)
			{
				participantQuery = participantQuery.Where(p => p.EventNo == selectedEventId);
			}

			// 篩選會員編號
			if (!string.IsNullOrEmpty(searchUserNo))
			{
				participantQuery = participantQuery.Where(p => p.UsersNo.Contains(searchUserNo));
			}

			// Include 對應活動資料
			var participants = await participantQuery
									.Include(p => p.EventNoNavigation)
									.Include(p => p.PaymentNoNavigation)
									.Include(p => p.UsersNoNavigation)
									.ToListAsync();

			ViewBag.SelectedEventId = selectedEventId ?? 0;
			ViewBag.SearchUserNo = searchUserNo;

			return View("~/Views/Participants/Index.cshtml", participants);
		}



		// 匯出 CSV
		//public async Task<IActionResult> ExportCsv(int? selectedEventId, string searchUserNo)
		//{
		//	var participantQuery = _context.Participants.AsQueryable();

		//	// 篩選活動
		//	if (selectedEventId != null && selectedEventId != 0)
		//	{
		//		participantQuery = participantQuery.Where(p => p.EventNo == selectedEventId);
		//	}

		//	// 篩選會員編號
		//	if (!string.IsNullOrEmpty(searchUserNo))
		//	{
		//		participantQuery = participantQuery.Where(p => p.UsersNo.Contains(searchUserNo));
		//	}

		//	var participants = await participantQuery
		//							.Include(p => p.EventNoNavigation)
		//							.Include(p => p.PaymentNoNavigation)
		//							.Include(p => p.UsersNoNavigation)
		//							.ToListAsync();

		//	//// 建立 CSV 內容
		//	//var sb = new StringBuilder();
		//	//sb.AppendLine("會員編號,活動編號,報名時間,狀態,付款編號");

		//	//foreach (var p in participants)
		//	//{
		//	//	sb.AppendLine($"{p.UsersNoNavigation?.No},{p.EventNoNavigation?.No},{p.RegisteredDate:yyyy-MM-dd HH:mm},{p.Status},{p.PaymentNoNavigation?.No}");
		//	//}

		//	//// 下載 CSV
		//	//return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Participants.csv");
		//}

		// =================== 列表搜尋篩選 (SearchSelect) ==================
		[HttpPost]
		[ValidateAntiForgeryToken] // 前端用 @Html.AntiForgeryToken() 產生
		public async Task<IActionResult> SearchSelect([FromBody] EventSearchFilter filter)
		{
			// 可選：容錯
			filter ??= new EventSearchFilter();

			var q = _context.Events.AsQueryable();

			// 關鍵字（名稱/種類/地點/說明）
			if (!string.IsNullOrWhiteSpace(filter.Keyword))
			{
				var kw = filter.Keyword.Trim();
				q = q.Where(e =>
					e.Title.Contains(kw) ||
					e.Category.Contains(kw) ||
					e.Location.Contains(kw) ||
					(e.Desc != null && e.Desc.Contains(kw)));
			}

			// 種類
			if (filter.Categories?.Any() == true)
				q = q.Where(e => filter.Categories.Contains(e.Category));

			// 狀態
			if (filter.Statuses?.Any() == true)
				q = q.Where(e => filter.Statuses.Contains(e.Status));

			// 地點
			if (filter.Locations?.Any() == true)
				q = q.Where(e => filter.Locations.Contains(e.Location));

			// 開始日期區間
			if (DateTime.TryParse(filter.DateFrom, out var from))
				q = q.Where(e => e.StartDate >= from);
			if (DateTime.TryParse(filter.DateTo, out var to))
			{
				// 若你希望包含當天最後一秒：
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

		// =================== 列表批次刪除 (DeleteMultiple) ==================
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
