using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

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

			// 檢查是否擁有任一權限
			var userPermissions = user.Claims
				.Where(c => c.Type == "Permission")
				.Select(c => c.Value)
				.ToList();

			bool hasPermission = _permissions.Any(p => userPermissions.Contains(p));

			if (!hasPermission)
			{
				context.Result = new ForbidResult();
			}
		}
	}
}