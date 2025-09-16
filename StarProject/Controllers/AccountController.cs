using Microsoft.AspNetCore.Mvc;

namespace StarProject.Controllers
{
	public class AccountController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
