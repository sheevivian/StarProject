using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;
using StarProject.Services;
using Microsoft.Data.SqlClient;

namespace StarProject.Controllers
{
	public class ParticipantsController : Controller
	{
		private readonly StarProjectContext _context;
		private readonly MailService _mail;

		public ParticipantsController(StarProjectContext context, MailService mail)
		{
			_context = context;
			_mail = mail;
		}

		// GET: Participants
		public async Task<IActionResult> Index(int selectedEventId = 0, string searchEventKeyword = "", string searchUserKeyword = "")
		{
			var participants = _context.Participants
				.Include(p => p.EventNoNavigation)
				.Include(p => p.PaymentNoNavigation)
				.Include(p => p.UsersNoNavigation)
				.AsNoTracking()
				.AsQueryable();

			if (selectedEventId > 0)
			{
				participants = participants.Where(p => p.EventNo == selectedEventId);
			}

			if (!string.IsNullOrEmpty(searchEventKeyword))
			{
				participants = participants.Where(p => p.EventNoNavigation != null &&
					p.EventNoNavigation.Title.Contains(searchEventKeyword));
			}

			if (!string.IsNullOrEmpty(searchUserKeyword))
			{
				participants = participants.Where(p => p.UsersNoNavigation != null &&
					p.UsersNoNavigation.Name.Contains(searchUserKeyword));
			}

			participants = participants
				.OrderByDescending(p => p.RegisteredDate)
				.ThenByDescending(p => p.No);

			ViewBag.AllEvents = await _context.Events.AsNoTracking().ToListAsync();
			ViewBag.SelectedEventId = selectedEventId;
			ViewBag.SearchEventKeyword = searchEventKeyword;
			ViewBag.SearchUserKeyword = searchUserKeyword;

			return View(await participants.ToListAsync());
		}

		// GET: Participants/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();

			var participant = await _context.Participants
				.Include(p => p.EventNoNavigation)
				.Include(p => p.PaymentNoNavigation)
				.Include(p => p.UsersNoNavigation)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.No == id);

			if (participant == null)
				return NotFound();

			return View(participant);
		}

		// GET: Participants/Create
		public IActionResult Create()
		{
			ViewData["EventNo"] = new SelectList(_context.Events, "No", "Title");
			ViewData["PaymentNo"] = new SelectList(_context.PaymentTransactions, "No", "No");
			ViewData["UsersNo"] = new SelectList(_context.Users, "No", "Name");
			return View(); // Views/Participants/Create.cshtml
		}

		// POST: Participants/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("No,EventNo,UsersNo,RegisteredDate,Status,PaymentNo")] Participant participant)
		{
			if (!ModelState.IsValid)
			{
				ViewData["EventNo"] = new SelectList(_context.Events, "No", "Title", participant.EventNo);
				ViewData["PaymentNo"] = new SelectList(_context.PaymentTransactions, "No", "No", participant.PaymentNo);
				ViewData["UsersNo"] = new SelectList(_context.Users, "No", "Name", participant.UsersNo);
				return View(participant);
			}

			try
			{
				participant.Code = await GenerateNextParticipantCodeAsync();

				participant.RegisteredDate = DateTime.Now;
				participant.UpdatedAt = DateTime.Now;

				const int maxAttempts = 3;
				for (int attempt = 1; attempt <= maxAttempts; attempt++)
				{
					try
					{
						_context.Add(participant);
						await _context.SaveChangesAsync();
						break; // OK
					}
					catch (DbUpdateException ex) when (IsUniqueCodeViolation(ex) && attempt < maxAttempts)
					{
						_context.Entry(participant).State = EntityState.Detached;
						participant.Code = await GenerateNextParticipantCodeAsync();
						participant.RegisteredDate = DateTime.Now;
						participant.UpdatedAt = DateTime.Now;
						_context.Add(participant);
						continue;
					}
				}

				// 報名成功才寄信（支援 "Success" 與「報名成功」）
				bool sent = false;
				if (IsSuccessStatus(participant.Status))
				{
					sent = await SendSignupEmailAndRecordAsync(participant.No);
				}

				TempData["Success"] = sent ? "新增成功，已寄出報名成功通知。" : "新增成功。";
				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateException ex)
			{
				var root = ex.GetBaseException()?.Message ?? ex.Message;
				ModelState.AddModelError(string.Empty, $"建立失敗（DB）：{root}");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"建立失敗：{ex.Message}");
			}

			// 失敗回表單
			ViewData["EventNo"] = new SelectList(_context.Events, "No", "Title", participant.EventNo);
			ViewData["PaymentNo"] = new SelectList(_context.PaymentTransactions, "No", "No", participant.PaymentNo);
			ViewData["UsersNo"] = new SelectList(_context.Users, "No", "Name", participant.UsersNo);
			return View(participant);
		}

		//「重寄通知mail」按鈕
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResendSignup(int id)
		{
			var ok = await SendSignupEmailAndRecordAsync(id);
			TempData["Success"] = ok ? "已重寄報名成功通知。" : "重寄失敗，請檢查收件者 Email 或 SMTP 設定。";
			return RedirectToAction(nameof(Index));
		}

		//Get: edit
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

		// POST: Participants/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("No,Status,PaymentNo")] Participant participant)
		{
			if (id != participant.No) return NotFound();

			if (!ModelState.IsValid)
			{
				var fullParticipantInvalid = await _context.Participants
					.Include(p => p.EventNoNavigation)
					.Include(p => p.UsersNoNavigation)
					.Include(p => p.PaymentNoNavigation)
					.FirstOrDefaultAsync(p => p.No == id);

				if (fullParticipantInvalid == null) return NotFound();
				return PartialView("_EditPartial", fullParticipantInvalid);
			}

			var existing = await _context.Participants
				.Include(p => p.EventNoNavigation)
				.Include(p => p.UsersNoNavigation)
				.FirstOrDefaultAsync(p => p.No == id);

			if (existing == null) return NotFound();

			// 由非 Success → Success 時，補寄成功信
			bool willSendSignup =
				!IsSuccessStatus(existing.Status) &&
				 IsSuccessStatus(participant.Status);

			existing.Status = participant.Status;
			existing.PaymentNo = participant.PaymentNo;
			existing.UpdatedAt = DateTime.Now;

			await _context.SaveChangesAsync();

			if (willSendSignup)
			{
				await SendSignupEmailAndRecordAsync(existing.No);
			}

			return Json(new { success = true });
		}

		// GET: Participants/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
				return NotFound();

			var participant = await _context.Participants
				.Include(p => p.EventNoNavigation)
				.Include(p => p.PaymentNoNavigation)
				.Include(p => p.UsersNoNavigation)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.No == id);

			if (participant == null)
				return NotFound();

			return View(participant);
		}

		// POST: Participants/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var participant = await _context.Participants.FindAsync(id);
			if (participant != null)
				_context.Participants.Remove(participant);

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool ParticipantExists(int id)
		{
			return _context.Participants.Any(e => e.No == id);
		}

		// ===========================
		// 共用：報名成功信 + EventNotif 防重寄（永遠先落 Pending）
		// ===========================
		private async Task<bool> SendSignupEmailAndRecordAsync(int participantId)
		{
			var p = await _context.Participants
				.Include(x => x.EventNoNavigation)
				.Include(x => x.UsersNoNavigation)
				.FirstOrDefaultAsync(x => x.No == participantId);

			if (p == null || p.EventNoNavigation == null)
				return false;

			// 收件者：優先 Users.Email；若你有 Participant.Email 可在此退而求其次
			var to = p.UsersNoNavigation?.Email;
			// if (string.IsNullOrWhiteSpace(to)) to = p.Email;

			// 先登記 Pending（NOT NULL 的 Senttime 以登記時間落帳）
			var notif = new EventNotif
			{
				EventNo = p.EventNo,
				ParticipantNo = p.No,
				Category = "Signup",
				Status = "Pending",
				Senttime = DateTime.UtcNow
			};

			try
			{
				_context.EventNotifs.Add(notif); // DbSet 名稱請以你的 Context 為準
				await _context.SaveChangesAsync(); // UNIQUE 索引重複會在此丟 DbUpdateException
			}
			catch (DbUpdateException)
			{
				// 已有同鍵（他處已處理/處理中）→ 視為成功，不重寄
				return true;
			}

			if (string.IsNullOrWhiteSpace(to))
			{
				// 沒有收件者 Email → 標記 Failed（仍保留台帳）
				try
				{
					notif.Status = "Failed";
					await _context.SaveChangesAsync();
				}
				catch { }
				return false;
			}

			try
			{
				await _mail.SendRegistrationSuccessEmail(to, p.EventNoNavigation.Title, p.EventNoNavigation.StartDate);

				notif.Status = "Success";
				// 若要把 Senttime 視為「實際寄出時間」可改成：
				// notif.Senttime = DateTime.UtcNow;
				await _context.SaveChangesAsync();
				return true;
			}
			catch
			{
				try
				{
					notif.Status = "Failed";
					// notif.Senttime = DateTime.UtcNow;
					await _context.SaveChangesAsync();
				}
				catch { }
				return false;
			}
		}

		// 狀態判斷：支援 "Success" 與中文「報名成功」
		private static bool IsSuccessStatus(string? s)
		{
			if (string.IsNullOrWhiteSpace(s)) return false;
			s = s.Trim();
			return s.Equals("Success", StringComparison.OrdinalIgnoreCase) || s == "報名成功";
		}

		// 產生下一個報名代碼（EV + 五位數遞增）
		private async Task<string> GenerateNextParticipantCodeAsync(CancellationToken ct = default)
		{
			var last5 = await _context.Participants
				.Where(x => x.Code != null && x.Code.StartsWith("EV") && x.Code.Length >= 7)
				.Select(x => x.Code.Substring(2, 5))
				.OrderByDescending(s => s)
				.FirstOrDefaultAsync(ct);

			var n = 0;
			if (!string.IsNullOrEmpty(last5) && int.TryParse(last5, out var parsed)) n = parsed;
			return "EV" + (n + 1).ToString("D5"); // EV00001, EV00002, …
		}

		// 判斷是否為 UNIQUE 衝突（SQL Server 2627/2601）
		private static bool IsUniqueCodeViolation(DbUpdateException ex)
		{
			return ex.GetBaseException() is SqlException sql && (sql.Number == 2627 || sql.Number == 2601);
		}
	}
}
