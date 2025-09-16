using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;                   // Excel 匯出（EPPlus）
using StarProject.Models;
using StarProject.Services;
using StarProject.ViewModel;
using StarProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;                     // CSV 匯出
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public class ParticipantsController : Controller
    {
        private readonly StarProjectContext _context;

        // ✅ 改為注入介面 IEmailService（對應你的 EmailService.cs）
        private readonly IEmailService _email;

        // ✅ 建構子改成 IEmailService
        public ParticipantsController(StarProjectContext context, IEmailService email)
        {
            _context = context;
            _email = email;
        }

        [HttpGet]
		public async Task<IActionResult> CheckDuplicate(int? eventId, int? usersNo)
		{
			// 如果參數不完整，回傳不存在
			if (eventId == null || usersNo == null)
			{
				return Json(new { exists = false });
			}

			try
			{
				// 將 usersNo 轉為字串以符合資料庫欄位型別
				var usersNoStr = usersNo.Value.ToString();

				// 檢查是否已有成功報名的記錄
				var duplicate = await _context.Participants
					.AsNoTracking()
					.AnyAsync(p =>
						p.EventNo == eventId &&
						p.UsersNo == usersNoStr &&
						(p.Status == "報名成功" || p.Status == "Success")
					);

				return Json(new { exists = duplicate });
			}
			catch (Exception ex)
			{
				// 發生錯誤時回傳不存在（不阻擋用戶操作）
				return Json(new { exists = false, error = ex.Message });
			}
		}

		// =========================
		// Index：回傳卡片資料
		// =========================
		public async Task<IActionResult> Index()
        {
            // (原程式碼不變)
            var counts = await _context.Participants
                .AsNoTracking()
                .GroupBy(p => p.EventNo)
                .Select(g => new { EventNo = g.Key, Count = g.Count() })
                .ToListAsync();
            var map = counts.ToDictionary(x => x.EventNo, x => x.Count);

            var cards = await _context.Events
                .AsNoTracking()
                .OrderByDescending(e => e.StartDate)
                .Select(e => new EventCardVm
                {
                    No = e.No,
                    Category = e.Category,
                    Title = e.Title,
                    StartDate = e.StartDate,
                    Status = e.Status,
                    Location = e.Location,
                    CoverImageUrl = e.Image,
                    MaxParticipants = e.MaxParticipants,
                    CurrentCount = 0
                })
                .ToListAsync();

            foreach (var c in cards)
                if (map.TryGetValue(c.No, out var n)) c.CurrentCount = n;

            var vm = new ParticipantsIndexVm { Cards = cards };
            return View(vm);
        }

        // =========================
        // Details
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var participant = await _context.Participants
                .Include(p => p.EventNoNavigation)
                .Include(p => p.PaymentNoNavigation)
                .Include(p => p.UsersNoNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.No == id);

            if (participant == null) return NotFound();

            return View(participant);
        }

        // =========================
        // Create
        // =========================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var openEvents = await _context.Events
                .AsNoTracking()
                .Where(e => e.Status == "報名中")
                .OrderBy(e => e.StartDate)
                .Select(e => new { e.No, e.Title })
                .ToListAsync();
            ViewBag.EventNo = new SelectList(openEvents, "No", "Title");

            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Name)
                .Select(u => new { Value = u.No.ToString(), Text = u.Name })
                .ToListAsync();
            ViewBag.UsersNo = new SelectList(users, "Value", "Text");

            return View(new Participant { Status = "報名成功" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("No,EventNo,UsersNo,RegisteredDate,Status")] Participant participant)
        {
			ModelState.Remove(nameof(Participant.EventNoNavigation));
			ModelState.Remove(nameof(Participant.UsersNoNavigation));

			if (string.Equals(participant.Status, "Success", StringComparison.OrdinalIgnoreCase))
                participant.Status = "報名成功";

            participant.PaymentNo = null;

            if (!ModelState.IsValid)
            {
                await PopulateCreateDropdownsAsync(participant.EventNo, participant.UsersNo);
                return View(participant);
            }

            var duplicate = await _context.Participants
                .AsNoTracking()
                .AnyAsync(p =>
                    p.EventNo == participant.EventNo &&
                    p.UsersNo == participant.UsersNo &&
                    (p.Status == "報名成功" || p.Status == "Success")
                );

            if (duplicate)
            {
                ModelState.AddModelError(string.Empty, "此會員已報名該活動（狀態：報名成功），請勿重複建立。");
                await PopulateCreateDropdownsAsync(participant.EventNo, participant.UsersNo);
                return View(participant);
            }

            try
            {
                participant.Code = await GenerateNextParticipantCodeAsync();
                participant.RegisteredDate = DateTime.Now;
                participant.UpdatedAt = DateTime.Now;

                _context.Add(participant);
                await _context.SaveChangesAsync();

                // ✅ 改用 _email 呼叫你的 EmailService 方法
                if (IsSuccessStatus(participant.Status))
                    await SendSignupEmailAndRecordAsync(participant.No);

                TempData["Success"] = "新增成功。";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"建立失敗：{ex.Message}");
            }

            await PopulateCreateDropdownsAsync(participant.EventNo, participant.UsersNo);
            return View(participant);
        }

        private async Task PopulateCreateDropdownsAsync(int? selectedEventNo, object? selectedUsersNo)
        {
            ViewBag.EventNo = new SelectList(
                await _context.Events.AsNoTracking()
                    .Where(e => e.Status == "報名中")
                    .OrderBy(e => e.StartDate)
                    .Select(e => new { e.No, e.Title })
                    .ToListAsync(),
                "No", "Title", selectedEventNo
            );

            var userItems = await _context.Users.AsNoTracking()
                .OrderBy(u => u.Name)
                .Select(u => new { Value = u.No.ToString(), Text = u.Name })
                .ToListAsync();

            ViewBag.UsersNo = new SelectList(userItems, "Value", "Text", selectedUsersNo?.ToString());
        }

        // =========================
        // Edit
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var participant = await _context.Participants
                .Include(p => p.EventNoNavigation)
                .Include(p => p.UsersNoNavigation)
                .Include(p => p.PaymentNoNavigation)
                .FirstOrDefaultAsync(p => p.No == id);

            if (participant == null) return NotFound();

            return PartialView("_EditDrawer", participant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("No,Status,PaymentNo")] Participant participant)
        {
            if (id != participant.No) return NotFound();

            var existing = await _context.Participants.FirstOrDefaultAsync(p => p.No == id);
            if (existing == null) return NotFound();

            existing.Status = participant.Status;
            existing.PaymentNo = participant.PaymentNo;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // ✅ 狀態改成成功 → 仍用 _email
            if (IsSuccessStatus(existing.Status))
                await SendSignupEmailAndRecordAsync(existing.No);

            return Json(new { success = true });
		}


		[HttpGet]
		public async Task<IActionResult> ListByEvent(int eventId)
		{
			try
			{
				var list = await _context.Participants
					.Include(p => p.UsersNoNavigation)
					.Include(p => p.EventNoNavigation)
					.AsNoTracking()
					.Where(p => p.EventNo == eventId)
					.OrderByDescending(p => p.RegisteredDate)
					.ToListAsync();

				ViewBag.EventId = eventId;

				// 確保回傳部分檢視
				return PartialView("_ParticipantsTable", list);
			}
			catch (Exception ex)
			{
				// 錯誤時也要回傳部分檢視，避免整頁錯誤
				ViewBag.EventId = eventId;
				ViewBag.ErrorMessage = ex.Message;
				return PartialView("_ParticipantsTable", new List<Participant>());
			}
		}

		// =========================
		// 批次刪除
		// =========================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> BulkDelete([FromForm] int eventId, [FromForm] int[] ids)
		{
			try
			{
				// 檢查參數
				if (ids == null || ids.Length == 0)
				{
					return Json(new { success = false, message = "未收到要刪除的項目。" });
				}

				if (eventId <= 0)
				{
					return Json(new { success = false, message = "無效的活動編號。" });
				}

				// 查找要刪除的項目
				var toDel = await _context.Participants
					.Where(p => p.EventNo == eventId && ids.Contains(p.No))
					.ToListAsync();

				if (!toDel.Any())
				{
					return Json(new { success = false, message = "查無要刪除的項目。" });
				}

				// 執行刪除
				_context.Participants.RemoveRange(toDel);
				var deletedCount = await _context.SaveChangesAsync();

				return Json(new
				{
					success = true,
					message = $"成功刪除 {toDel.Count} 筆資料。",
					deletedCount = toDel.Count
				});
			}
			catch (Exception ex)
			{
				// 記錄錯誤（如果有日誌系統）
				// _logger?.LogError(ex, "批次刪除失敗: EventId={EventId}, Ids={Ids}", eventId, string.Join(",", ids ?? new int[0]));

				return Json(new
				{
					success = false,
					message = $"刪除失敗：{ex.Message}"
				});
			}
		}
		// =========================
		// 匯出 CSV
		// =========================
		[HttpGet]
public async Task<IActionResult> ExportCsv(int? eventId)
{
    var q = _context.Participants
        .Include(p => p.UsersNoNavigation)
        .Include(p => p.EventNoNavigation)
        .AsNoTracking()
        .AsQueryable();
    
    if (eventId.HasValue && eventId.Value > 0)
        q = q.Where(p => p.EventNo == eventId.Value);
    
    var rows = await q
        .OrderBy(p => p.EventNo)
        .ThenByDescending(p => p.RegisteredDate)
        .ToListAsync();
    
    var sb = new StringBuilder();
    sb.AppendLine("活動編號,活動名稱,會員姓名,狀態,報名日期,最後更新");
    
    foreach (var r in rows)
    {
        sb.AppendLine($"{r.EventNo}," +
                     $"\"{r.EventNoNavigation?.Title}\"," +
                     $"\"{r.UsersNoNavigation?.Name}\"," +
                     $"{r.Status}," +
                     $"{r.RegisteredDate:yyyy/MM/dd HH:mm}," +
                     $"{r.UpdatedAt:yyyy/MM/dd HH:mm}");
    }
    
    var bytes = Encoding.UTF8.GetBytes(sb.ToString());
    var fileName = $"Participants_{(eventId ?? 0)}_{DateTime.Now:yyyyMMddHHmm}.csv";
    
    return File(bytes, "text/csv;charset=utf-8", fileName);
}

// =========================
// 匯出 Excel
// =========================
[HttpGet]
public async Task<IActionResult> ExportExcel(int? eventId)
{
    var q = _context.Participants
        .Include(p => p.UsersNoNavigation)
        .Include(p => p.EventNoNavigation)
        .AsNoTracking()
        .AsQueryable();
    
    if (eventId.HasValue && eventId.Value > 0)
        q = q.Where(p => p.EventNo == eventId.Value);
    
    var rows = await q
        .OrderBy(p => p.EventNo)
        .ThenByDescending(p => p.RegisteredDate)
        .ToListAsync();
    
    using var pkg = new ExcelPackage();
    var ws = pkg.Workbook.Worksheets.Add("Participants");
    
    // 設定標題列
    ws.Cells[1, 1].Value = "活動編號";
    ws.Cells[1, 2].Value = "活動名稱";
    ws.Cells[1, 3].Value = "會員姓名";
    ws.Cells[1, 4].Value = "狀態";
    ws.Cells[1, 5].Value = "報名日期";
    ws.Cells[1, 6].Value = "最後更新";
    
    int r = 2;
    foreach (var p in rows)
    {
        ws.Cells[r, 1].Value = p.EventNo;
        ws.Cells[r, 2].Value = p.EventNoNavigation?.Title;
        ws.Cells[r, 3].Value = p.UsersNoNavigation?.Name;
        ws.Cells[r, 4].Value = p.Status;
        ws.Cells[r, 5].Value = p.RegisteredDate.ToString("yyyy/MM/dd HH:mm");
        ws.Cells[r, 6].Value = p.UpdatedAt.ToString("yyyy/MM/dd HH:mm");
        r++;
    }
    
    ws.Cells.AutoFitColumns();
    var bytes = pkg.GetAsByteArray();
    var fileName = $"Participants_{(eventId ?? 0)}_{DateTime.Now:yyyyMMddHHmm}.xlsx";
    
    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
}

// =========================
// 重新發送報名確認信
// =========================

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResendSignup(int id)
		{
			try
			{
				if (id <= 0)
				{
					return Json(new { success = false, message = "無效的參與者ID。" });
				}

				var (ok, reason) = await SendSignupEmailAndRecordAsync(id, forceResend: true);

				if (ok)
				{
					return Json(new
					{
						success = true,
						message = "報名確認信已重新發送。"
					});
				}
				else
				{
					var errorMessage = reason switch
					{
						"NoParticipant" => "找不到該參與者資料。",
						"NoEvent" => "找不到相關活動資料。",
						"NoEmail" => "該會員沒有設定電子郵件地址。",
						"InvalidEmail" => "會員的電子郵件地址格式不正確。",
						_ => $"發送失敗：{reason}"
					};

					return Json(new { success = false, message = errorMessage });
				}
			}
			catch (Exception ex)
			{
				// 記錄錯誤
				// _logger?.LogError(ex, "重送確認信失敗: ParticipantId={Id}", id);

				return Json(new
				{
					success = false,
					message = $"系統錯誤：{ex.Message}"
				});
			}
		}
	// =========================
	// Private methods
	// =========================
	private async Task<(bool ok, string reason)> SendSignupEmailAndRecordAsync(int participantId, bool forceResend = false)
    {
        var p = await _context.Participants
            .Include(x => x.EventNoNavigation)
            .Include(x => x.UsersNoNavigation)
            .FirstOrDefaultAsync(x => x.No == participantId);

        if (p == null) return (false, "NoParticipant");
        if (p.EventNoNavigation == null) return (false, "NoEvent");

        var to = p.UsersNoNavigation?.Email?.Trim();
        if (string.IsNullOrWhiteSpace(to)) return (false, "NoEmail");

        try { _ = MimeKit.MailboxAddress.Parse(to); }
        catch { return (false, "InvalidEmail"); }

        try
        {
            // ✅ 這裡改用 _email（介面）
            await _email.SendRegistrationSuccessEmail(
                to: to!,
                eventName: p.EventNoNavigation.Title,
                eventTime: p.EventNoNavigation.StartDate,
                qrPayload: $"SP|P={p.No}|E={p.EventNo}|T={DateTime.UtcNow:yyyyMMddHHmmss}|K={Guid.NewGuid():N}",
                recipientName: p.UsersNoNavigation?.Name
            );
            return (true, "OK");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    private static bool IsSuccessStatus(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        return s.Equals("Success", StringComparison.OrdinalIgnoreCase) || s == "報名成功";
    }


    private async Task<string> GenerateNextParticipantCodeAsync(CancellationToken ct = default)
    {
        var last5 = await _context.Participants
            .Where(x => x.Code != null && x.Code.StartsWith("EV") && x.Code.Length >= 7)
            .Select(x => x.Code.Substring(2, 5))
            .OrderByDescending(s => s)
            .FirstOrDefaultAsync(ct);

        var n = 0;
        if (!string.IsNullOrEmpty(last5) && int.TryParse(last5, out var parsed)) n = parsed;
        return "EV" + (n + 1).ToString("D5");
    }

    private static bool IsUniqueCodeViolation(DbUpdateException ex)
    {
        return ex.GetBaseException() is SqlException sql && (sql.Number == 2627 || sql.Number == 2601);
    }
    }
}
