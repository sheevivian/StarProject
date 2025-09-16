using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Attributes;
using StarProject.Models;
using System.Security.Claims;

namespace StarProject.Controllers
{
	[Authorize] // 需要登入
	[Permission("oa")] // 需要 OA 權限 (Office Automation)
	public class AttendanceController : Controller
	{
		private readonly StarProjectContext _context;

		public AttendanceController(StarProjectContext context)
		{
			_context = context;
		}

		// 取得當前登入員工的編號
		private string GetCurrentEmpNo()
		{
			return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
		}

		// 取得當前員工資訊
		private async Task<Emp?> GetCurrentEmployee()
		{
			var empNo = GetCurrentEmpNo();
			return await _context.Emps
				.Include(e => e.DeptNoNavigation)
				.FirstOrDefaultAsync(e => e.No == empNo && e.Status == true);
		}

		// 出勤打卡首頁
		public async Task<IActionResult> Index()
		{
			var employee = await GetCurrentEmployee();
			if (employee == null)
			{
				return RedirectToAction("Index", "Login");
			}

			var empNo = employee.No;
			var today = DateOnly.FromDateTime(DateTime.Today);

			// 取得今日打卡記錄
			var todayRecord = await _context.AttendanceRecords
				.FirstOrDefaultAsync(a => a.EmpNo == empNo && a.ClockDate == today);

			// 取得最近7天的記錄
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

		// 上班打卡 API
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ClockIn()
		{
			try
			{
				var employee = await GetCurrentEmployee();
				if (employee == null)
				{
					return Json(new { success = false, message = "請重新登入" });
				}

				var empNo = employee.No;
				var today = DateOnly.FromDateTime(DateTime.Today);
				var now = DateTime.Now;

				// 檢查是否已經打過上班卡
				var existingRecord = await _context.AttendanceRecords
					.FirstOrDefaultAsync(a => a.EmpNo == empNo && a.ClockDate == today);

				if (existingRecord?.ClockInTime != null)
				{
					return Json(new { success = false, message = "今日已經打過上班卡了！" });
				}

				if (existingRecord == null)
				{
					// 建立新記錄
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
					// 更新現有記錄
					existingRecord.ClockInTime = now;
					existingRecord.UpdatedAt = now;
				}

				await _context.SaveChangesAsync();

				return Json(new
				{
					success = true,
					message = $"{employee.Name} 上班打卡成功！",
					time = now.ToString("HH:mm:ss")
				});
			}
			catch (Exception ex)
			{
				// 建議加入日誌記錄
				// _logger.LogError(ex, "Clock in failed");
				return Json(new { success = false, message = $"打卡失敗：{ex.Message}" });
			}
		}

		// 下班打卡 API
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ClockOut()
		{
			try
			{
				var employee = await GetCurrentEmployee();
				if (employee == null)
				{
					return Json(new { success = false, message = "請重新登入" });
				}

				var empNo = employee.No;
				var today = DateOnly.FromDateTime(DateTime.Today);
				var now = DateTime.Now;

				var record = await _context.AttendanceRecords
					.FirstOrDefaultAsync(a => a.EmpNo == empNo && a.ClockDate == today);

				if (record?.ClockInTime == null)
				{
					return Json(new { success = false, message = "請先打上班卡！" });
				}

				if (record.ClockOutTime != null)
				{
					return Json(new { success = false, message = "今日已經打過下班卡了！" });
				}

				record.ClockOutTime = now;
				record.UpdatedAt = now;

				await _context.SaveChangesAsync();

				// 在這裡處理沒有 WorkHours 的問題
				var workHours = record.ClockOutTime - record.ClockInTime;
				var hoursText = workHours?.ToString(@"hh\:mm") ?? "0:00";

				return Json(new
				{
					success = true,
					message = $"{employee.Name} 下班打卡成功！",
					time = now.ToString("HH:mm:ss"),
					workHours = hoursText
				});
			}

			catch (Exception ex)
			{
				var inner = ex.InnerException?.InnerException?.Message
							?? ex.InnerException?.Message
							?? ex.Message;

				return Json(new { success = false, message = $"打卡失敗：{inner}" });
			}


		}
	}
	
}