using Microsoft.AspNetCore.Mvc;
using StarProject.Models;
using StarProject.Services;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
	public class MailController : Controller
	{
		private readonly MailService _mailService;
		public MailController(MailService mailService) => _mailService = mailService;

		// GET: /Mail/Test
		public IActionResult Test()
		{
			return View();
		}

		// POST: /Mail/Send
		[HttpPost]
		public async Task<IActionResult> Send(MailMessageModel model)
		{
			if (ModelState.IsValid)
			{
				await _mailService.SendEmailAsync(model);
				ViewBag.Message = "郵件已寄出";
				return View("Test");  // 回到測試頁面顯示訊息
			}

			ViewBag.Message = "資料格式有誤";
			return View("Test");
		}
	}
}
