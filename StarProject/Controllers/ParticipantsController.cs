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
				participants = participants.Where(p => p.EventNo == selectedEventId);

			if (!string.IsNullOrEmpty(searchEventKeyword))
				participants = participants.Where(p => p.EventNoNavigation != null && p.EventNoNavigation.Title.Contains(searchEventKeyword));

			if (!string.IsNullOrEmpty(searchUserKeyword))
				participants = participants.Where(p => p.UsersNoNavigation != null && p.UsersNoNavigation.Name.Contains(searchUserKeyword));

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

		// GET: Participants/Create
		[HttpGet]
		public async Task<IActionResult> Create()
		{
			// 只提供「報名中」活動
			var openEvents = await _context.Events
				.AsNoTracking()
				.Where(e => e.Status == "報名中")
				.OrderBy(e => e.StartDate)
				.Select(e => new { e.No, e.Title })
				.ToListAsync();
			ViewBag.EventNo = new SelectList(openEvents, "No", "Title");

			// 會員下拉（UsersNo 為字串）
			var users = await _context.Users
				.AsNoTracking()
				.OrderBy(u => u.Name)
				.Select(u => new { Value = u.No.ToString(), Text = u.Name })
				.ToListAsync();
			ViewBag.UsersNo = new SelectList(users, "Value", "Text");

			return View(new Participant { Status = "報名成功" });
		}

		// POST: Participants/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			[Bind("No,EventNo,UsersNo,RegisteredDate,Status")] Participant participant)
		{
			// 狀態英中一致化（這段在記憶體中執行，與 EF 翻譯無關，保留可用）
			if (string.Equals(participant.Status, "Success", StringComparison.OrdinalIgnoreCase))
				participant.Status = "報名成功";

			// 不連結付款單
			participant.PaymentNo = null;

			if (!ModelState.IsValid)
			{
				await PopulateCreateDropdownsAsync(participant.EventNo, participant.UsersNo);
				return View(participant);
			}

			// 後端把關：同會員 + 同活動 已有「報名成功」就拒絕
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
				// 伺服端產碼與時間欄位
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
						// 繼續重試
					}
				}

				// 報名成功才寄信
				bool sent = false;
				if (IsSuccessStatus(participant.Status) || participant.Status == "報名成功")
				{
					var (ok, _) = await SendSignupEmailAndRecordAsync(participant.No);
					sent = ok;
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

			// 失敗回表單：回填下拉
			await PopulateCreateDropdownsAsync(participant.EventNo, participant.UsersNo);
			return View(participant);
		}

		// ---- 私有小幫手：回填 Create 頁面下拉 ----
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

			ViewBag.UsersNo = new SelectList(
				userItems, "Value", "Text",
				selectedUsersNo?.ToString()
			);
		}

		//「重寄通知mail」按鈕（強制重寄）
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResendSignup(int id)
		{
			var (ok, reason) = await SendSignupEmailAndRecordAsync(id, forceResend: true);

			if (ok)
			{
				TempData["Success"] = "已重寄報名成功通知。";
			}
			else
			{
				TempData["Error"] = reason switch
				{
					"NoParticipant" => "重寄失敗：找不到這筆報名資料。",
					"NoEvent" => "重寄失敗：找不到對應的活動資訊。",
					"NoEmail" => "重寄失敗：這位會員沒有 Email，請先補齊會員 Email。",
					"InvalidEmail" => "重寄失敗：會員 Email 格式不正確。",
					"SmtpAuth" => "重寄失敗：SMTP 驗證失敗，請檢查帳密/應用程式密碼。",
					"SmtpConnect" => "重寄失敗：SMTP 連線失敗，請檢查 Server/Port/防火牆。",
					_ => $"重寄失敗：{reason}"
				};
			}

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
				await SendSignupEmailAndRecordAsync(existing.No);

			return Json(new { success = true });
		}

		// GET: Participants/Delete/5
		public async Task<IActionResult> Delete(int? id)
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
		// 共用：報名成功信 + EventNotif （含可強制重寄）
		// ===========================
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

			// 先做 Email 格式檢查
			try { _ = MimeKit.MailboxAddress.Parse(to); }
			catch { return (false, "InvalidEmail"); }

			var qr = $"SP|P={p.No}|E={p.EventNo}|T={DateTime.UtcNow:yyyyMMddHHmmss}|K={Guid.NewGuid():N}";

			if (!forceResend)
			{
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
					_context.EventNotifs.Add(notif);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateException)
				{
					return (true, "AlreadySent");
				}

				try
				{
					await _mail.SendRegistrationSuccessEmail(
						to: to!,
						eventName: p.EventNoNavigation.Title,
						eventTime: p.EventNoNavigation.StartDate,
						qrPayload: qr,
						recipientName: p.UsersNoNavigation?.Name
					);

					notif.Status = "Success";
					await _context.SaveChangesAsync();
					return (true, "OK");
				}
				catch (MailKit.Security.AuthenticationException) { notif.Status = "Failed"; TrySave(); return (false, "SmtpAuth"); }
				catch (MailKit.Net.Smtp.SmtpProtocolException) { notif.Status = "Failed"; TrySave(); return (false, "SmtpConnect"); }
				catch (Exception ex) { notif.Status = "Failed"; TrySave(); return (false, GetInnermost(ex)); }
			}
			else
			{
				var notif = await _context.EventNotifs.FirstOrDefaultAsync(n =>
					n.EventNo == p.EventNo &&
					n.ParticipantNo == p.No &&
					n.Category == "Signup"
				);

				if (notif == null)
				{
					notif = new EventNotif
					{
						EventNo = p.EventNo,
						ParticipantNo = p.No,
						Category = "Signup",
						Status = "Pending",
						Senttime = DateTime.UtcNow
					};
					try
					{
						_context.EventNotifs.Add(notif);
						await _context.SaveChangesAsync();
					}
					catch (Exception ex)
					{
						return (false, $"CreateLogFailed: {GetInnermost(ex)}");
					}
				}

				try
				{
					await _mail.SendRegistrationSuccessEmail(
						to: to!,
						eventName: p.EventNoNavigation.Title,
						eventTime: p.EventNoNavigation.StartDate,
						qrPayload: qr,
						recipientName: p.UsersNoNavigation?.Name
					);

					notif.Status = "Success";
					notif.Senttime = DateTime.UtcNow;

					await _context.SaveChangesAsync();
					return (true, "OK");
				}
				catch (MailKit.Security.AuthenticationException) { notif.Status = "Failed"; notif.Senttime = DateTime.UtcNow; TrySave(); return (false, "SmtpAuth"); }
				catch (MailKit.Net.Smtp.SmtpProtocolException) { notif.Status = "Failed"; notif.Senttime = DateTime.UtcNow; TrySave(); return (false, "SmtpConnect"); }
				catch (Exception ex) { notif.Status = "Failed"; notif.Senttime = DateTime.UtcNow; TrySave(); return (false, GetInnermost(ex)); }
			}

			void TrySave()
			{
				try { _context.SaveChanges(); } catch { /* ignore */ }
			}
			string GetInnermost(Exception ex)
			{
				while (ex.InnerException != null) ex = ex.InnerException;
				return ex.Message;
			}
		}

		// 狀態判斷：支援 "Success" 與中文「報名成功」
		private static bool IsSuccessStatus(string? s)
		{
			if (string.IsNullOrWhiteSpace(s)) return false;
			s = s.Trim();
			return s.Equals("Success", StringComparison.OrdinalIgnoreCase) || s == "報名成功";
		}

		// GET: 前端檢查是否已存在「同會員 + 同活動」的『報名成功』資料
		[HttpGet]
		public async Task<IActionResult> CheckDuplicate(int eventId, string usersNo)
		{
			var exists = await _context.Participants
				.AsNoTracking()
				.AnyAsync(p =>
					p.EventNo == eventId &&
					p.UsersNo == usersNo &&
					(p.Status == "報名成功" || p.Status == "Success")
				);

			return Json(new { exists });
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
			return "EV" + (n + 1).ToString("D5"); // EV00001, EV00002, …
		}

		// 判斷是否為 UNIQUE 衝突
		private static bool IsUniqueCodeViolation(DbUpdateException ex)
		{
			return ex.GetBaseException() is SqlException sql && (sql.Number == 2627 || sql.Number == 2601);
		}
	}
}
