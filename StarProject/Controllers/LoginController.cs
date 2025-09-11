using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarProject.DTOs;
using System.Security.Claims;

namespace StarProject.Controllers
{
	[AllowAnonymous]  //允許匿名使用者存取
	public class LoginController : Controller
	{
		public IActionResult Index()
		{
			HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return View();
		}

		[HttpPost]
		public IActionResult Index(LoginDTO value)
		{

			////要去資料庫撈資料
			if (value.Account.ToLower() == "11111".ToLower() && value.Password == "123456")
			{
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, value.Account),
					new Claim("FullName", "kai")
				};

				var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
				HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

				return RedirectToAction("Index", "Home");
			}
			else
			{
				ViewBag.Message = "帳號或密碼錯誤";
				return View(value);
			}
		}
	}
}
