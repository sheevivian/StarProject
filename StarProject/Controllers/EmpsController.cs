using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StarProject.DTOs.EmpsDTOs;
using StarProject.Models;
using System.IO;

namespace StarProject.Controllers
{
	public class EmpsController : Controller
	{
		private readonly StarProjectContext _context;
		private readonly IRazorViewEngine _razorViewEngine;
		private readonly ITempDataProvider _tempDataProvider;
		private readonly IServiceProvider _serviceProvider;

		public EmpsController(
			StarProjectContext context,
			IRazorViewEngine razorViewEngine,
			ITempDataProvider tempDataProvider,
			IServiceProvider serviceProvider)
		{
			_context = context;
			_razorViewEngine = razorViewEngine;
			_tempDataProvider = tempDataProvider;
			_serviceProvider = serviceProvider;
		}

		// Index 頁面
		public async Task<IActionResult> Index()
		{
			var emps = await _context.Emps
				.Include(e => e.DeptNoNavigation)
				.Include(e => e.RoleNoNavigation)
				.OrderBy(e => e.EmpCode)
				.Take(10) // 預設載入前 10 筆
				.ToListAsync();

			ViewBag.Total = await _context.Emps.CountAsync();
			ViewBag.Page = 1;
			ViewBag.PageSize = 10;
			ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.Total / ViewBag.PageSize);

			return View(emps);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SearchEmps([FromBody] SearchEmpRequest request)
		{
			var query = _context.Emps
				.Include(e => e.DeptNoNavigation)
				.Include(e => e.RoleNoNavigation)
				.AsQueryable();

			// 關鍵字
			if (!string.IsNullOrWhiteSpace(request.Keyword))
			{
				var kw = request.Keyword.Trim();
				query = query.Where(e =>
					e.Name.Contains(kw) ||
					e.EmpCode.Contains(kw) ||
					e.DeptNoNavigation.DeptName.Contains(kw) ||
					e.RoleNoNavigation.RoleName.Contains(kw));
			}

			// 部門
			if (request.Departments != null && request.Departments.Any())
			{
				query = query.Where(e => request.Departments.Contains(e.DeptNoNavigation.DeptName));
			}

			// 職位
			if (request.Roles != null && request.Roles.Any())
			{
				query = query.Where(e => request.Roles.Contains(e.RoleNoNavigation.RoleName));
			}

			// 狀態
			if (request.Statuses != null && request.Statuses.Any())
			{
				bool wantIn = request.Statuses.Contains("在職");
				bool wantOut = request.Statuses.Contains("離職");
				query = query.Where(e => (wantIn && e.Status) || (wantOut && !e.Status));
			}

			// 入職日期
			if (!string.IsNullOrWhiteSpace(request.DateFrom) && DateTime.TryParse(request.DateFrom, out DateTime from))
			{
				query = query.Where(e => e.HireDate >= from);
			}
			if (!string.IsNullOrWhiteSpace(request.DateTo) && DateTime.TryParse(request.DateTo, out DateTime to))
			{
				query = query.Where(e => e.HireDate <= to);
			}

			// 分頁
			var totalCount = await query.CountAsync();
			var page = Math.Max(request.Page, 1);
			var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

			var items = await query
				.OrderBy(e => e.EmpCode)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// 設定 ViewBag 資料
			ViewBag.Total = totalCount;
			ViewBag.PageSize = pageSize;
			ViewBag.Page = page;
			ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

			// Render 部分 View - 先渲染員工列表
			var empRowsHtml = await RenderPartialViewToStringAsync("_EmpRowsPartial", items);

			// 再渲染分頁 - 這裡需要傳遞一個空的 model，但 ViewBag 會保持
			var paginationHtml = await RenderPartialViewToStringAsync("_PaginationPartial", new object());

			return Json(new
			{
				empRows = empRowsHtml,
				pagination = paginationHtml
			});
		}


		// 批量刪除
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteMultiple([FromBody] List<string> empIds)
		{
			if (empIds == null || empIds.Count == 0)
				return Json(new { success = false, message = "沒有選取任何員工" });

			var emps = await _context.Emps.Where(e => empIds.Contains(e.No)).ToListAsync();
			if (emps.Count == 0)
				return Json(new { success = false, message = "找不到選取的員工" });

			_context.Emps.RemoveRange(emps);
			await _context.SaveChangesAsync();

			return Json(new { success = true });
		}

		// Helper: 把 PartialView Render 成字串
		private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
		{
			var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor);

			using (var sw = new StringWriter())
			{
				var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);
				if (viewResult.View == null)
				{
					viewResult = _razorViewEngine.GetView(null, viewName, false);
					if (viewResult.View == null)
					{
						throw new InvalidOperationException($"找不到 View {viewName}");
					}
				}

				var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), ModelState)
				{
					Model = model
				};

				// 複製當前的 ViewBag 資料到新的 ViewDataDictionary
				foreach (var item in ViewData)
				{
					viewDictionary[item.Key] = item.Value;
				}

				var tempData = new TempDataDictionary(HttpContext, _tempDataProvider);

				var viewContext = new ViewContext(
					actionContext,
					viewResult.View,
					viewDictionary,
					tempData,
					sw,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync(viewContext);
				return sw.ToString();
			}
		}
	}
}
