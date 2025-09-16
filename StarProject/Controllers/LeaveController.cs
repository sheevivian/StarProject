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
		private readonly ILogger<LeaveController> _logger;

		public LeaveController(StarProjectContext context, ILogger<LeaveController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// 取得當前登入員工的編號
		private string GetCurrentEmpNo()
		{
			var empNo = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
			_logger.LogInformation($"當前員工編號: {empNo}");
			return empNo;
		}

		// 取得當前員工資訊
		private async Task<Emp?> GetCurrentEmployee()
		{
			try
			{
				var empNo = GetCurrentEmpNo();
				if (string.IsNullOrEmpty(empNo))
				{
					_logger.LogWarning("員工編號為空");
					return null;
				}

				var employee = await _context.Emps
					.Include(e => e.DeptNoNavigation)
					.FirstOrDefaultAsync(e => e.No == empNo && e.Status == true);

				if (employee == null)
				{
					_logger.LogWarning($"找不到員工資料，編號: {empNo}");
				}
				else
				{
					_logger.LogInformation($"找到員工: {employee.Name}");
				}

				return employee;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "取得員工資料時發生錯誤");
				return null;
			}
		}

		// 請假申請首頁
		public async Task<IActionResult> Index()
		{
			try
			{
				_logger.LogInformation("進入請假申請頁面");

				// 檢查數據庫連線
				if (!await _context.Database.CanConnectAsync())
				{
					_logger.LogError("數據庫連線失敗");
					ViewBag.ErrorMessage = "數據庫連線失敗，請稍後再試";
					return View();
				}

				var employee = await GetCurrentEmployee();
				if (employee == null)
				{
					_logger.LogWarning("員工資料為空，重導向到登入頁面");
					return RedirectToAction("Index", "Login");
				}

				var empNo = employee.No;

				// 取得所有請假類型
				var leaveTypes = await _context.LeaveTypes
					.OrderBy(lt => lt.TypeCode)
					.ToListAsync();

				_logger.LogInformation($"找到 {leaveTypes.Count} 個請假類型");

				// 取得我的請假記錄
				var myApplications = await _context.LeaveApplications
					.Include(l => l.LeaveType)
					.Where(l => l.EmpNo == empNo)
					.OrderByDescending(l => l.AppliedAt)
					.Take(10)
					.ToListAsync();

				_logger.LogInformation($"找到 {myApplications.Count} 筆請假記錄");

				ViewBag.LeaveTypes = leaveTypes;
				ViewBag.MyApplications = myApplications;
				ViewBag.EmployeeName = employee.Name;

				return View();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "請假申請頁面載入時發生錯誤");
				ViewBag.ErrorMessage = "頁面載入失敗，請稍後再試";
				return View();
			}
		}

		// 提交請假申請
		[HttpPost]
		public async Task<IActionResult> Apply(int LeaveTypeId, DateTime StartDate, DateTime EndDate, string Reason)
		{
			try
			{
				_logger.LogInformation($"請假申請: LeaveTypeId={LeaveTypeId}, StartDate={StartDate}, EndDate={EndDate}");

				var employee = await GetCurrentEmployee();
				if (employee == null)
				{
					_logger.LogWarning("請假申請失敗：找不到員工資料");
					return Json(new { success = false, message = "請重新登入" });
				}

				// 驗證輸入
				if (LeaveTypeId <= 0 || string.IsNullOrWhiteSpace(Reason))
				{
					_logger.LogWarning("請假申請失敗：輸入資料不完整");
					return Json(new { success = false, message = "請填寫完整的申請資訊" });
				}

				// 修正：將 DateTime 轉換為 DateOnly 進行比較
				if (DateOnly.FromDateTime(EndDate) < DateOnly.FromDateTime(StartDate))
				{
					_logger.LogWarning("請假申請失敗：結束日期早於開始日期");
					return Json(new { success = false, message = "結束日期不能早於開始日期" });
				}

				// 計算請假天數
				var totalDays = (decimal)(EndDate - StartDate).Days + 1;

				// 檢查請假類型是否存在
				var leaveType = await _context.LeaveTypes.FindAsync(LeaveTypeId);
				if (leaveType == null)
				{
					_logger.LogWarning($"請假申請失敗：找不到請假類型 ID={LeaveTypeId}");
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
				var result = await _context.SaveChangesAsync();

				_logger.LogInformation($"請假申請成功，受影響的記錄數: {result}");

				return Json(new { success = true, message = "請假申請已送出，等待審核中" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "請假申請時發生錯誤");
				return Json(new { success = false, message = $"申請失敗：{ex.Message}" });
			}
		}

		// 新增：檢查系統狀態的方法
		public async Task<IActionResult> SystemCheck()
		{
			var status = new
			{
				DatabaseConnected = await _context.Database.CanConnectAsync(),
				CurrentUser = User.Identity?.Name,
				CurrentEmpNo = GetCurrentEmpNo(),
				IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
				Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
			};

			return Json(status);
		}
	}
}