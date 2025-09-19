using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using StarProject.Models;

namespace StarProject.Attributes
{
	public class PermissionAttribute : Attribute, IAuthorizationFilter
	{
		private readonly string[] _permissions;

		public PermissionAttribute(params string[] permissions)
		{
			_permissions = permissions;
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var user = context.HttpContext.User;

			if (!user.Identity.IsAuthenticated)
			{
				context.Result = new RedirectToActionResult("Index", "Login", null);
				return;
			}

			// 先嘗試從 Claims 檢查權限
			if (CheckUserPermissionsFromClaims(user, _permissions))
			{
				return; // 有權限，允許訪問
			}

			// 如果 Claims 檢查失敗，再從資料庫檢查
			var dbContext = context.HttpContext.RequestServices.GetService<StarProjectContext>();
			if (dbContext == null)
			{
				// 修正：重定向到正確的 Error Controller
				context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
				return;
			}

			var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim))
			{
				// 修正：重定向到正確的 Error Controller
				context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
				return;
			}

			try
			{
				bool hasPermission = CheckUserPermissionsFromDatabase(dbContext, userIdClaim, _permissions);
				if (!hasPermission)
				{
					// 修正：重定向到正確的 Error Controller
					context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
				}
			}
			catch
			{
				// 修正：重定向到正確的 Error Controller
				context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
			}
		}

		// 新增：從 Claims 檢查權限
		private bool CheckUserPermissionsFromClaims(ClaimsPrincipal user, string[] requiredPermissions)
		{
			try
			{
				var userPermissions = user.FindAll("Permission").Select(c => c.Value.ToLower()).ToHashSet();

				// 檢查是否有任何一個必需的權限
				foreach (var permission in requiredPermissions)
				{
					if (userPermissions.Contains(permission.ToLower()))
					{
						return true;
					}
				}

				return false;
			}
			catch
			{
				return false;
			}
		}

		private bool CheckUserPermissionsFromDatabase(StarProjectContext context, string userNo, string[] requiredPermissions)
		{
			try
			{
				var userEmp = context.Emps
					.Include(e => e.RoleNoNavigation)
					.FirstOrDefault(e => e.No == userNo);

				if (userEmp == null || userEmp.RoleNoNavigation == null)
					return false;

				// 檢查是否有任何一個必需的權限
				foreach (var permission in requiredPermissions)
				{
					if (HasSpecificPermission(userEmp.RoleNoNavigation, permission))
						return true;
				}

				return false;
			}
			catch
			{
				return false;
			}
		}

		private bool HasSpecificPermission(Role role, string permission)
		{
			return permission.ToLower() switch
			{
				"emp" => role.Emp,
				"user" => role.User,
				"info" => role.Info,
				"event" => role.Event,
				"pd" => role.Pd,
				"tic" => role.Tic,
				"pm" => role.Pm,
				"order" => role.Order,
				"cs" => role.Cs,
				"oa" => role.Oa,
				"conlist" => role.CoNlist,
				"cone" => role.CoNe,
				_ => false
			};
		}
	}
}