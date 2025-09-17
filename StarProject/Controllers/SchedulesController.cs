using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;
using StarProject.Data;

namespace StarProject.Controllers
{
	public class SchedulesController : Controller
	{
		private readonly StarProjectContext _context;

		public SchedulesController(StarProjectContext context)
		{
			_context = context;
		}

		// GET: Schedules
		public async Task<IActionResult> Index()
		{
			var schedules = await _context.Schedules.ToListAsync();
			return View(schedules);
		}

		// GET: Schedules/Details?eventNo=1&releaseDate=2025-09-05
		public async Task<IActionResult> Details(int eventNo, DateTime releaseDate)
		{
			var schedule = await _context.Schedules
				.FirstOrDefaultAsync(s => s.EventNo == eventNo && s.ReleaseDate == releaseDate);

			if (schedule == null) return NotFound();
			return View(schedule);
		}

		// GET: Schedules/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Schedules/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("EventNo,ReleaseDate,ExpirationDate,Executed")] Schedule schedule)
		{
			if (ModelState.IsValid)
			{
				_context.Add(schedule);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(schedule);
		}

		// GET: Schedules/Edit?eventNo=1&releaseDate=2025-09-05
		public async Task<IActionResult> Edit(int eventNo, DateTime releaseDate)
		{
			var schedule = await _context.Schedules
				.FirstOrDefaultAsync(s => s.EventNo == eventNo && s.ReleaseDate == releaseDate);

			if (schedule == null) return NotFound();
			return View(schedule);
		}

		// POST: Schedules/Edit
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int eventNo, DateTime releaseDate, [Bind("EventNo,ReleaseDate,ExpirationDate,Executed")] Schedule schedule)
		{
			var existing = await _context.Schedules
				.FirstOrDefaultAsync(s => s.EventNo == eventNo && s.ReleaseDate == releaseDate);

			if (existing == null) return NotFound();

			if (ModelState.IsValid)
			{
				existing.ExpirationDate = schedule.ExpirationDate;
				existing.Executed = schedule.Executed;

				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(schedule);
		}

		// GET: Schedules/Delete?eventNo=1&releaseDate=2025-09-05
		public async Task<IActionResult> Delete(int eventNo, DateTime releaseDate)
		{
			var schedule = await _context.Schedules
				.FirstOrDefaultAsync(s => s.EventNo == eventNo && s.ReleaseDate == releaseDate);

			if (schedule == null) return NotFound();
			return View(schedule);
		}

		// POST: Schedules/DeleteConfirmed
		[HttpPost, ActionName("DeleteConfirmed")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int eventNo, DateTime releaseDate)
		{
			var schedule = await _context.Schedules
				.FirstOrDefaultAsync(s => s.EventNo == eventNo && s.ReleaseDate == releaseDate);

			if (schedule != null)
			{
				_context.Schedules.Remove(schedule);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Index));
		}
	}
}
