using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;

namespace StarProject.Controllers
{
	public class ParticipantsController : Controller
	{
		private readonly StarProjectContext _context;

		public ParticipantsController(StarProjectContext context)
		{
			_context = context;
		}

		// GET: Participants
		public async Task<IActionResult> Index(int selectedEventId = 0, string searchEventKeyword = "", string searchUserKeyword = "")
		{
			var participants = _context.Participants
				.Include(p => p.EventNoNavigation)
				.Include(p => p.PaymentNoNavigation)
				.Include(p => p.UsersNoNavigation)
				.AsNoTracking() // 列表頁建議使用，避免追蹤開銷
				.AsQueryable();

			// 篩選活動編號
			if (selectedEventId > 0)
			{
				participants = participants.Where(p => p.EventNo == selectedEventId);
			}

			// 篩選活動名稱（保留 Contains；若要更一致可改 EF.Functions.Like）
			if (!string.IsNullOrEmpty(searchEventKeyword))
			{
				participants = participants.Where(p => p.EventNoNavigation != null &&
					p.EventNoNavigation.Title.Contains(searchEventKeyword));
			}

			// 篩選會員姓名
			if (!string.IsNullOrEmpty(searchUserKeyword))
			{
				participants = participants.Where(p => p.UsersNoNavigation != null &&
					p.UsersNoNavigation.Name.Contains(searchUserKeyword));
			}

			// ★ 排序：先以報名時間新→舊；若同時間再以流水號大→小，避免看起來像被編號主導
			participants = participants
				.OrderByDescending(p => p.RegisteredDate)
				.ThenByDescending(p => p.No);

			// 傳送所有活動給下拉選單
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
			return View();
		}

		// POST: Participants/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("No,EventNo,UsersNo,RegisteredDate,Status,PaymentNo")] Participant participant)
		{
			if (ModelState.IsValid)
			{
				participant.RegisteredDate = DateTime.Now;
				participant.UpdatedAt = DateTime.Now;
				_context.Add(participant);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["EventNo"] = new SelectList(_context.Events, "No", "Title", participant.EventNo);
			ViewData["PaymentNo"] = new SelectList(_context.PaymentTransactions, "No", "No", participant.PaymentNo);
			ViewData["UsersNo"] = new SelectList(_context.Users, "No", "Name", participant.UsersNo);
			return View(participant);
		}

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

			if (ModelState.IsValid)
			{
				var existing = await _context.Participants.FindAsync(id);
				if (existing == null) return NotFound();

				existing.Status = participant.Status;
				existing.PaymentNo = participant.PaymentNo;
				existing.UpdatedAt = DateTime.Now;

				await _context.SaveChangesAsync();

				return Json(new { success = true });
			}

			// 驗證失敗：重新載入完整資料供局部檢視
			var fullParticipant = await _context.Participants
				.Include(p => p.EventNoNavigation)
				.Include(p => p.UsersNoNavigation)
				.Include(p => p.PaymentNoNavigation)
				.FirstOrDefaultAsync(p => p.No == id);

			if (fullParticipant == null)
			{
				return NotFound();
			}

			return PartialView("_EditPartial", fullParticipant);
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
	}
}
