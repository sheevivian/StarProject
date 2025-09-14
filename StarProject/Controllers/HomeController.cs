using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using StarProject.Models;

namespace StarProject.Controllers
{
	[Authorize] // �b�o�̥[�W [Authorize] �Ӥ��O����]�w
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

		[AllowAnonymous] // ���~�������\�ΦW�X��
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[AllowAnonymous] // �X�ݩڵ��������\�ΦW�X��
		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}