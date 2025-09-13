using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Attributes;
using StarProject.Models;

namespace StarProject.Controllers
{
	[Authorize]
	public class RoleController : Controller
	{
		private readonly StarProjectContext _context;

		public RoleController(StarProjectContext context)
		{
			_context = context;
		}

		// 權限管理頁面 - 只有擁有員工管理權限的人可以訪問
		[Permission("emp")]
		public async Task<IActionResult> Index()
		{
			var roles = await _context.Roles.OrderBy(r => r.No).ToListAsync();
			return View(roles);
		}

		// 更新權限的 API
		[HttpPost]
		[Permission("emp")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdatePermissions([FromBody] List<RoleUpdateDto> updates)
		{
			try
			{
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
					}
				}

				await _context.SaveChangesAsync();
				return Json(new { success = true, message = "權限更新成功" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = $"更新失敗：{ex.Message}" });
			}
		}
	}

	// DTO for role updates
	public class RoleUpdateDto
	{
		public int RoleId { get; set; }
		public bool Emp { get; set; }
		public bool User { get; set; }
		public bool Info { get; set; }
		public bool Event { get; set; }
		public bool Pd { get; set; }
		public bool Tic { get; set; }
		public bool Pm { get; set; }
		public bool Order { get; set; }
		public bool Cs { get; set; }
		public bool Oa { get; set; }
		public bool CoNlist { get; set; }
		public bool CoNe { get; set; }
	}
}