using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;
using System.Security.Claims;

namespace StarProject.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		private readonly StarProjectContext _context;

		public HomeController(StarProjectContext context)
		{
			_context = context;
		}

		private string GetCurrentEmpNo()
		{
			return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
		}

		private async Task<Emp?> GetCurrentEmployee()
		{
			var empNo = GetCurrentEmpNo();
			return await _context.Emps
				.Include(e => e.DeptNoNavigation)
				.FirstOrDefaultAsync(e => e.No == empNo && e.Status == true);
		}

		public async Task<IActionResult> Index()
		{
			var employee = await GetCurrentEmployee();
			if (employee == null)
			{
				return RedirectToAction("Index", "Login");
			}

			var empNo = employee.No;
			var today = DateOnly.FromDateTime(DateTime.Today);

			// ���o���饴�d�O��
			var todayRecord = await _context.AttendanceRecords
				.FirstOrDefaultAsync(a => a.EmpNo == empNo && a.ClockDate == today);

			// ���o�̪�7�Ѫ��O��
			var recentRecords = await _context.AttendanceRecords
				.Where(a => a.EmpNo == empNo)
				.OrderByDescending(a => a.ClockDate)
				.Take(7)
				.ToListAsync();

			ViewBag.EmployeeName = employee.Name;
			ViewBag.DepartmentName = employee.DeptNoNavigation.DeptName;
			ViewBag.TodayRecord = todayRecord;
			ViewBag.RecentRecords = recentRecords;

			return View();
		}

		// ���d�ϸ��
		public async Task<IActionResult> AttendanceCard()
		{
			var employee = await GetCurrentEmployee();
			if (employee == null)
			{
				return RedirectToAction("Index", "Login");
			}

			var empNo = employee.No;
			var today = DateOnly.FromDateTime(DateTime.Today);

			var todayRecord = await _context.AttendanceRecords
				.FirstOrDefaultAsync(a => a.EmpNo == empNo && a.ClockDate == today);

			ViewBag.EmployeeName = employee.Name;
			ViewBag.DepartmentName = employee.DeptNoNavigation.DeptName;
			ViewBag.TodayRecord = todayRecord;

			return PartialView("_AttendanceCardPartial");
		}

		// POST �W�Z���d
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ClockIn()
		{
			try
			{
				var employee = await GetCurrentEmployee();
				if (employee == null)
				{
					return Json(new { success = false, message = "�Э��s�n�J" });
				}

				var empNo = employee.No;
				var today = DateOnly.FromDateTime(DateTime.Today);
				var now = DateTime.Now;

				// �ˬd�O�_�w�g���L�W�Z�d
				var existingRecord = await _context.AttendanceRecords
					.FirstOrDefaultAsync(a => a.EmpNo == empNo && a.ClockDate == today);

				if (existingRecord?.ClockInTime != null)
				{
					return Json(new { success = false, message = "����w�g���L�W�Z�d�F�I" });
				}

				if (existingRecord == null)
				{
					// �إ߷s�O��
					existingRecord = new AttendanceRecord
					{
						EmpNo = empNo,
						ClockDate = today,
						ClockInTime = now,
						UpdatedAt = now
					};
					_context.AttendanceRecords.Add(existingRecord);
				}
				else
				{
					// ��s�{���O��
					existingRecord.ClockInTime = now;
					existingRecord.UpdatedAt = now;
				}

				await _context.SaveChangesAsync();

				return Json(new
				{
					success = true,
					message = $"{employee.Name} �W�Z���d���\�I",
					time = now.ToString("HH:mm:ss")
				});
			}
			catch (Exception ex)
			{
				// ��ĳ�[�J��x�O��
				// _logger.LogError(ex, "Clock in failed");
				return Json(new { success = false, message = $"���d���ѡG{ex.Message}" });
			}
		}

		// POST �U�Z���d
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ClockOut()
		{
			try
			{
				var employee = await GetCurrentEmployee();
				if (employee == null)
				{
					return Json(new { success = false, message = "�Э��s�n�J" });
				}

				var empNo = employee.No;
				var today = DateOnly.FromDateTime(DateTime.Today);
				var now = DateTime.Now;

				var record = await _context.AttendanceRecords
					.FirstOrDefaultAsync(a => a.EmpNo == empNo && a.ClockDate == today);

				if (record?.ClockInTime == null)
				{
					return Json(new { success = false, message = "�Х����W�Z�d�I" });
				}

				if (record.ClockOutTime != null)
				{
					return Json(new { success = false, message = "����w�g���L�U�Z�d�F�I" });
				}

				record.ClockOutTime = now;
				record.UpdatedAt = now;

				await _context.SaveChangesAsync();

				// �b�o�̳B�z�S�� WorkHours �����D
				var workHours = record.ClockOutTime - record.ClockInTime;
				var hoursText = workHours?.ToString(@"hh\:mm") ?? "0:00";

				return Json(new
				{
					success = true,
					message = $"{employee.Name} �U�Z���d���\�I",
					time = now.ToString("HH:mm:ss"),
					workHours = hoursText
				});
			}

			catch (Exception ex)
			{
				var inner = ex.InnerException?.InnerException?.Message
							?? ex.InnerException?.Message
							?? ex.Message;

				return Json(new { success = false, message = $"���d���ѡG{inner}" });
			}


		}
	}
}
