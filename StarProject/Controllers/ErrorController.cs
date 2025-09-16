using Microsoft.AspNetCore.Mvc;

namespace StarProject.Controllers
{
	public class ErrorController : Controller
	{
		[HttpGet]
		public IActionResult AccessDenied()
		{
			Response.StatusCode = 403;
			return View();
		}

		[HttpGet]
		public IActionResult InternalServerError()
		{
			Response.StatusCode = 500;
			return View();
		}
	}
}

// 在 Startup.cs 或 Program.cs 中配置錯誤頁面
/*
在 Configure 方法中添加：

app.UseStatusCodePagesWithReExecute("/Error/{0}");

或者使用自定義錯誤處理：

app.UseExceptionHandler("/Error/InternalServerError");
app.UseStatusCodePagesWithRedirects("/Error/AccessDenied?statusCode={0}");
*/