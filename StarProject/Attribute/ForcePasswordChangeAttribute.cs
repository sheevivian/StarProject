using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;
using System.Security.Claims;

namespace StarProject.Attributes
{
	/// <summary>
	/// 強制修改密碼過濾器屬性
	/// 用於檢查使用者是否需要強制修改密碼
	/// </summary>
	public class ForcePasswordChangeAttribute : IAsyncActionFilter
	{
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var controller = context.RouteData.Values["controller"]?.ToString();
			var action = context.RouteData.Values["action"]?.ToString();

			// 排除的控制器和動作
			var excludeControllers = new[]
			{
				"Login",     // 登入相關
                "Password",  // 密碼修改相關
                "Account"    // 如果有帳戶相關控制器
            };

			var excludeActions = new[]
			{
				"Logout",    // 登出
                "Change",    // 修改密碼
                "Error",     // 錯誤頁面
                "AccessDenied" // 訪問拒絕
            };

			// 如果是排除的控制器或動作，直接通過
			if (excludeControllers.Contains(controller, StringComparer.OrdinalIgnoreCase) ||
				excludeActions.Contains(action, StringComparer.OrdinalIgnoreCase))
			{
				await next();
				return;
			}

			// 檢查用戶是否已登入
			var user = context.HttpContext.User;
			if (user.Identity?.IsAuthenticated != true)
			{
				// 未登入用戶，交給正常的驗證流程處理
				await next();
				return;
			}

			// 檢查是否需要強制修改密碼
			var empNo = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!string.IsNullOrEmpty(empNo))
			{
				try
				{
					var serviceProvider = context.HttpContext.RequestServices;
					var dbContext = serviceProvider.GetRequiredService<StarProjectContext>();
					var employee = await dbContext.Emps.FindAsync(empNo);

					if (employee != null && employee.ForceChangePassword)
					{
						// 重定向到強制修改密碼頁面
						context.Result = new RedirectToActionResult("Change", "Password", new { forced = true });
						return;
					}
				}
				catch (Exception ex)
				{
					// 記錄錯誤，但不阻止正常流程
					var logger = context.HttpContext.RequestServices.GetService<ILogger<ForcePasswordChangeAttribute>>();
					logger?.LogError(ex, "檢查強制修改密碼時發生錯誤: {ErrorMessage}", ex.Message);
				}
			}

			await next();
		}
	}
}