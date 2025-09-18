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
using System.Text;                     // CSV 匯出
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

        // 其餘動作（Delete / ListByEvent / 匯出 CSV & Excel）維持原樣 …

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendSignup(int id)
        {
            var (ok, reason) = await SendSignupEmailAndRecordAsync(id, forceResend: true);
            if (ok) return Json(new { success = true });
            return Json(new { success = false, message = reason });
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
