using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Attributes;
using StarProject.DTOs.RoleDTOs;
using StarProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace StarProject.Controllers
{
	[Authorize]
	public class RoleController : Controller
	{
		private readonly StarProjectContext _context;
		private readonly ILogger<RoleController> _logger;

		public RoleController(StarProjectContext context, ILogger<RoleController> logger)
		{
			_context = context;
			_logger = logger;
		}

		[Permission("emp")]
		public async Task<IActionResult> Index()
		{
			var roles = await _context.Roles.OrderBy(r => r.No).ToListAsync();
			return View(roles);
		}

		[HttpPost]
		[Permission("emp")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdatePermissions([FromBody] List<RoleUpdateDto> updates)
		{
			try
			{
				var updatedRoleIds = new List<int>();

				foreach (var update in updates)
				{
					var role = await _context.Roles.FindAsync(update.RoleId);
					if (role != null)
					{
						role.Emp = update.Emp;
						role.User = update.User;
						role.Info = update.Info;
						role.Event = update.Event;
						role.Pd = update.Pd;
						role.Tic = update.Tic;
						role.Pm = update.Pm;
						role.Order = update.Order;
						role.Cs = update.Cs;
						role.Oa = update.Oa;
						role.CoNlist = update.CoNlist;
						role.CoNe = update.CoNe;

						updatedRoleIds.Add(update.RoleId);
					}
				}

				await _context.SaveChangesAsync();

				// 如果當前用戶的權限被更新，立即更新其 Claims
				await UpdateCurrentUserClaimsIfNeeded(updatedRoleIds);

				_logger.LogInformation($"權限更新成功，影響 {updates.Count} 個角色");

				return Json(new { success = true, message = "權限更新成功，變更立即生效" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "更新權限時發生錯誤");
				return Json(new { success = false, message = $"更新失敗：{ex.Message}" });
			}
		}

		// 如果當前用戶的角色權限被更改，立即更新其 Claims
		private async Task UpdateCurrentUserClaimsIfNeeded(List<int> updatedRoleIds)
		{
			try
			{
				var currentUserId = GetCurrentUserId();
				if (currentUserId == null) return;

				var currentUserEmp = await _context.Emps
					.Include(e => e.RoleNoNavigation)
					.FirstOrDefaultAsync(e => e.No == currentUserId.ToString());

				if (currentUserEmp?.RoleNoNavigation != null && updatedRoleIds.Contains(currentUserEmp.RoleNoNavigation.No))
				{
					await RefreshCurrentUserClaims(currentUserEmp);
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "更新用戶 Claims 時發生錯誤");
			}
		}

		private async Task RefreshCurrentUserClaims(Emp emp)
		{
			try
			{
				var identity = (ClaimsIdentity)HttpContext.User.Identity;

				var oldPermissionClaims = identity.Claims
					.Where(c => c.Type == "Permission")
					.ToList();

				foreach (var claim in oldPermissionClaims)
				{
					identity.RemoveClaim(claim);
				}

				var newPermissionClaims = GetPermissionClaims(emp.RoleNoNavigation);
				foreach (var claim in newPermissionClaims)
				{
					identity.AddClaim(claim);
				}

				var principal = new ClaimsPrincipal(identity);
				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

				_logger.LogInformation($"已更新用戶 {emp.No} 的權限 Claims");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "重新簽署用戶認證時發生錯誤");
			}
		}

		private List<Claim> GetPermissionClaims(Role role)
		{
			var claims = new List<Claim>();

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

			return claims;
		}

		private int? GetCurrentUserId()
		{
			var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			return int.TryParse(userIdClaim, out var userId) ? userId : null;
		}

	}
}