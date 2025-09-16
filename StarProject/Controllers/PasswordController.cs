using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;
using StarProject.ViewModels;
using System.Security.Claims;

namespace StarProject.Controllers
{
	[Authorize]
	public class PasswordController : Controller
	{
		private readonly StarProjectContext _context;

		public PasswordController(StarProjectContext context)
		{
			_context = context;
		}

		// GET: Password/Change
		public IActionResult Change(bool forced = false)
		{
			var viewModel = new ChangePasswordViewModel
			{
				IsForced = forced
			};

			if (forced)
			{
				ViewBag.Message = "為了您的帳戶安全，請修改預設密碼";
				ViewBag.IsForced = true;
			}

			return View(viewModel);
		}

		// POST: Password/Change
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Change(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var empNo = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				var employee = await _context.Emps.FindAsync(empNo);

				if (employee == null)
				{
					ModelState.AddModelError("", "找不到員工資料");
					return View(model);
				}

				// 驗證目前密碼
				if (!VerifyPassword(model.CurrentPassword, employee.PasswordHash, employee.PasswordSalt))
				{
					ModelState.AddModelError("CurrentPassword", "目前密碼不正確");
					return View(model);
				}

				// 檢查新密碼是否與舊密碼相同
				if (VerifyPassword(model.NewPassword, employee.PasswordHash, employee.PasswordSalt))
				{
					ModelState.AddModelError("NewPassword", "新密碼不能與目前密碼相同");
					return View(model);
				}

				// 更新密碼
				(employee.PasswordHash, employee.PasswordSalt) = PasswordHelper.HashPassword(model.NewPassword);
				employee.ForceChangePassword = false; // 取消強制修改密碼標記

				await _context.SaveChangesAsync();

				TempData["PasswordChanged"] = "密碼修改成功";

				// 如果是強制修改，導向首頁；否則導回原來的頁面
				if (model.IsForced)
				{
					return RedirectToAction("Index", "Home");
				}
				else
				{
					return RedirectToAction("Profile", "Account"); // 或其他適當的頁面
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"密碼修改失敗：{ex.Message}");
				return View(model);
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