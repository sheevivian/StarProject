using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using StarProject.Models;

namespace StarProject.Controllers
{
	[Authorize] // 在這裡加上 [Authorize] 而不是全域設定
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[AllowAnonymous] // 錯誤頁面允許匿名訪問
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[AllowAnonymous] // 訪問拒絕頁面允許匿名訪問
		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}