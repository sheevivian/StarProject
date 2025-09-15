using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Attributes;
using StarProject.Models;
using System.Security.Claims;

namespace StarProject.Controllers
{
	[Authorize] // 需要登入
	[Permission("oa")] // 需要 OA 權限
	public class LeaveController : Controller
	{
		private readonly StarProjectContext _context;

		public LeaveController(StarProjectContext context)
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

		// 請假申請首頁
		public async Task<IActionResult> Index()
		{
			var employee = await GetCurrentEmployee();
			if (employee == null)
			{
				return RedirectToAction("Index", "Login");
			}

			var empNo = employee.No;

			// 取得所有請假類型
			var leaveTypes = await _context.LeaveTypes
				.OrderBy(lt => lt.TypeCode)
				.ToListAsync();

			// 取得我的請假記錄
			var myApplications = await _context.LeaveApplications
				.Include(l => l.LeaveType)
				.Where(l => l.EmpNo == empNo)
				.OrderByDescending(l => l.AppliedAt)
				.Take(10)
				.ToListAsync();

			ViewBag.LeaveTypes = leaveTypes;
			ViewBag.MyApplications = myApplications;
			ViewBag.EmployeeName = employee.Name;

			return View();
		}

		// 提交請假申請
		[HttpPost]
		public async Task<IActionResult> Apply(int LeaveTypeId, DateTime StartDate, DateTime EndDate, string Reason)
		{
			try
			{
				var employee = await GetCurrentEmployee();
				if (employee == null)
				{
					return Json(new { success = false, message = "請重新登入" });
				}

				// 驗證輸入
				if (LeaveTypeId <= 0 || string.IsNullOrWhiteSpace(Reason))
				{
					return Json(new { success = false, message = "請填寫完整的申請資訊" });
				}

				// 修正：將 DateTime 轉換為 DateOnly 進行比較
				if (DateOnly.FromDateTime(EndDate) < DateOnly.FromDateTime(StartDate))
				{
					return Json(new { success = false, message = "結束日期不能早於開始日期" });
				}

				// 計算請假天數 (簡化版，實際可能需要排除週末和假日)
				var totalDays = (decimal)(EndDate - StartDate).Days + 1;

				// 檢查請假類型是否存在
				var leaveType = await _context.LeaveTypes.FindAsync(LeaveTypeId);
				if (leaveType == null)
				{
					return Json(new { success = false, message = "請假類型不存在" });
				}

				var application = new LeaveApplication
				{
					EmpNo = employee.No,
					LeaveTypeId = LeaveTypeId,
					StartDate = DateOnly.FromDateTime(StartDate),
					EndDate = DateOnly.FromDateTime(EndDate),
					TotalDays = totalDays,
					Reason = Reason,
					Status = "pending",
					AppliedAt = DateTime.Now
				};

				_context.LeaveApplications.Add(application);
				await _context.SaveChangesAsync();

				return Json(new { success = true, message = "請假申請已送出，等待審核中" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "申請失敗，請稍後再試" });
			}
		}
	}
}