using Microsoft.AspNetCore.Mvc;

namespace StarProject.Controllers
{
	public class BaseController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
