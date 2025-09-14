using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.DTOs;
using StarProject.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace StarProject.Controllers
{
	[AllowAnonymous]
	public class LoginController : Controller
	{
		private readonly StarProjectContext _context;

		public LoginController(StarProjectContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Index(LoginDTO value)
		{
			try
			{
				// 從資料庫查詢員工資料，包含角色和權限
				var employee = await _context.Emps
					.Include(e => e.RoleNoNavigation)
					.Include(e => e.DeptNoNavigation)
					.FirstOrDefaultAsync(e => e.EmpCode == value.Account && e.Status == true);

				if (employee == null)
				{
					ViewBag.Message = "帳號不存在或已被停用";
					return View(value);
				}

				// 驗證密碼
				if (!VerifyPassword(value.Password, employee.PasswordHash, employee.PasswordSalt))
				{
					ViewBag.Message = "密碼錯誤";
					return View(value);
				}

				// 建立 Claims
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, employee.EmpCode),
					new Claim(ClaimTypes.NameIdentifier, employee.No),
					new Claim("FullName", employee.Name),
					new Claim("DeptName", employee.DeptNoNavigation.DeptName),
					new Claim("RoleName", employee.RoleNoNavigation.RoleName),
					new Claim(ClaimTypes.Role, employee.RoleNoNavigation.RoleName), // 用於 [Authorize(Roles = "HR")]
                };

				// 加入權限 Claims
				var role = employee.RoleNoNavigation;
				if (role.Emp) claims.Add(new Claim("Permission", "emp"));
				if (role.User) claims.Add(new Claim("Permission", "user"));
				if (role.Info) claims.Add(new Claim("Permission", "info"));
				if (role.Event) claims.Add(new Claim("Permission", "event"));
				if (role.Pd) claims.Add(new Claim("Permission", "pd"));
				if (role.Tic) claims.Add(new Claim("Permission", "tic"));
				if (role.Pm) claims.Add(new Claim("Permission", "pm"));
				if (role.Order) claims.Add(new Claim("Permission", "order"));
				if (role.Cs) claims.Add(new Claim("Permission", "cs"));
				if (role.Oa) claims.Add(new Claim("Permission", "oa"));
				if (role.CoNlist) claims.Add(new Claim("Permission", "conlist"));
				if (role.CoNe) claims.Add(new Claim("Permission", "cone"));

				var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

				// 更新最後登入時間
				employee.LastLogin = DateTime.Now;
				await _context.SaveChangesAsync();

				// 檢查是否需要強制修改密碼
				if (employee.ForceChangePassword)
				{
					return RedirectToAction("Change", "Password", new { forced = true });
				}
				return RedirectToAction("Index", "Home");
			}
			catch (Exception ex)
			{
				ViewBag.Message = "系統錯誤，請稍後再試";
				return View(value);
			}
		}

		private bool VerifyPassword(string password, string hash, string salt)
		{
			byte[] saltBytes = Convert.FromBase64String(salt);
			string computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: password,
				salt: saltBytes,
				prf: KeyDerivationPrf.HMACSHA256,
				iterationCount: 10000,
				numBytesRequested: 32
			));

			return computedHash == hash;
		}
	}
}