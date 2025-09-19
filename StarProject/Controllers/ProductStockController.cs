using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StarProject.Helpers;
using StarProject.Models;
using StarProject.ViewModels;

namespace StarProject.Controllers
{
	public class ProductStockController : Controller
	{
		private const int pageNumber = 20;
		private readonly StarProjectContext _context;

		public ProductStockController(StarProjectContext context)
		{
			_context = context;
		}

		// 商品列表+頁數(GET)
		// GET: Product
		[HttpGet]
		public async Task<IActionResult> Index(int page = 1, int pageSize = pageNumber)
		{
			// Step 1: 先在 DB 層做 GroupBy，算好 Sum / Max
			var stockSummaryQuery = _context.ProductStocks
				.GroupBy(ps => ps.ProductNo)
				.Select(g => new
				{
					ProductNo = g.Key,
					SumQuantity = g.Sum(x => x.TransQuantity),
					UpdateDate = g.Max(x => x.Date)
				});

			// Step 2: 再用上面的結果去 Join Products
			var query = from s in stockSummaryQuery
						join p in _context.Products.Include(p => p.ProCategoryNoNavigation)
							on s.ProductNo equals p.No
						select new ProductStockSumViewModel
						{
							ProductNo = s.ProductNo,
							ProductName = p.Name,
							ProCategoryName = p.ProCategoryNoNavigation.Name,
							SumQuantity = s.SumQuantity,
							UpdateDate = s.UpdateDate,
						};

			query = query.OrderByDescending(x => x.UpdateDate);

			// Step 3: 分頁 (這時 query 仍是 IQueryable<ProductStockSumViewModel>)
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// Step 4: ViewBag
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			return View(items);
		}

		// 商品庫存查詢
		// POST: Product/SearchSelect 
		[HttpPost]
		public async Task<IActionResult> SearchSelect([FromBody] SearchFilterVM filters)
		{
			// 從 filters 取得 pageSize，如果沒有就給預設值
			int page = filters.Page > 0 ? filters.Page : 1;
			int pageSize = filters.PageSize >= 0 ? filters.PageSize : 20;

			// Step 1: 先在 DB 層做 GroupBy，算好 Sum / Max
			var stockSummaryQuery = _context.ProductStocks
				.GroupBy(ps => ps.ProductNo)
				.Select(g => new
				{
					ProductNo = g.Key,
					SumQuantity = g.Sum(x => x.TransQuantity),
					UpdateDate = g.Max(x => x.Date)
				});

			// Step 2: 再用上面的結果去 Join Products
			var query = from s in stockSummaryQuery
						join p in _context.Products.Include(p => p.ProCategoryNoNavigation)
							on s.ProductNo equals p.No
						select new ProductStockSumViewModel
						{
							ProductNo = s.ProductNo,
							ProductName = p.Name,
							ProCategoryName = p.ProCategoryNoNavigation.Name,
							SumQuantity = s.SumQuantity,
							UpdateDate = s.UpdateDate,
						};

			query = query.OrderByDescending(x => x.UpdateDate);

			// keyword
			if (!string.IsNullOrEmpty(filters.keyword))
			{
				query = query.Where(x => x.ProductName.Contains(filters.keyword)
									|| x.ProCategoryName.Contains(filters.keyword)
									|| x.ProductNo.ToString().Contains(filters.keyword));
			}

			//// 分類
			if (filters.Categories != null && filters.Categories.Any())
				query = query.Where(x => filters.Categories.Contains(x.ProCategoryName));

			// 庫存數量
			if (filters.QuantityLowerVal.HasValue && filters.QuantityHigherVal.HasValue)
				query = query.Where(x => x.SumQuantity > filters.QuantityHigherVal.Value
									  || x.SumQuantity < filters.QuantityLowerVal.Value)
							 .OrderBy(x => x.SumQuantity);

			else if(filters.QuantityLowerVal.HasValue)
				query = query.Where(x => x.SumQuantity < filters.QuantityLowerVal.Value)
							 .OrderBy(x => x.SumQuantity);

			else if (filters.QuantityHigherVal.HasValue)
				query = query.Where(x => x.SumQuantity > filters.QuantityHigherVal.Value)
							 .OrderByDescending(x => x.SumQuantity); 

			// 呼叫分頁工具
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			var tableHtml = await RenderPartialViewToString("_ProductStockRows", items);
			var paginationHtml = await RenderPartialViewToString("_ProductStockPagination", null);

			return Json(new { tableHtml, paginationHtml });
		}

		// 商品列表-更新頁碼+搜尋
		// GET: Product/RenderPartialViewToString
		[HttpGet]
		public async Task<string> RenderPartialViewToString(string viewName, object model)
		{
			if (string.IsNullOrEmpty(viewName))
				viewName = ControllerContext.ActionDescriptor.ActionName;

			ViewData.Model = model;

			using (var sw = new StringWriter())
			{
				var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
				var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

				if (viewResult.Success == false)
					throw new ArgumentNullException($"View {viewName} not found.");

				var viewContext = new ViewContext(
					ControllerContext,
					viewResult.View,
					ViewData,
					TempData,
					sw,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync(viewContext);
				return sw.GetStringBuilder().ToString();
			}
		}

		// 詳細資料-商品異動庫存
		// Get: ProductStock/GetLogs/5
		[HttpGet]
		public async Task<IActionResult> GetLogs(int productNo)
		{
			// 從 DB 抓該商品的異動紀錄
			var logs = await _context.ProductStocks
				.Where(l => l.ProductNo == productNo)
				.OrderByDescending(l => l.Date)
				.Select(l => new {
					TransNo = l.No,
					ChangeType = l.Type,
					ChangeQuantity = l.TransQuantity,
					Note = l.Note,
					Date = l.Date.ToString("yyyy-MM-dd HH:mm") // 前端好處理
				})
				.ToListAsync();

			return Json(logs); // 回傳 JSON
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{

			ViewBag.TransType = new List<SelectListItem>
				{
					new SelectListItem { Value = "入庫", Text = "入庫" },
					new SelectListItem { Value = "退貨", Text = "退貨" },
					new SelectListItem { Value = "盤差", Text = "盤差" },
				};
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(ProductStock pStock)
		{
			var productStock = new ProductStock
			{
				ProductNo = pStock.ProductNo,
				Type = pStock.Type,
				TransQuantity = pStock.TransQuantity,
				Date = DateTime.Now,
				Note = pStock.Note,
			};

			// 移除 ModelState 中無法綁定的欄位
			ModelState.Remove("ProductNoNavigation");
			if (ModelState.IsValid)
			{
				try
				{
					_context.Add(productStock);
					await _context.SaveChangesAsync();

					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					return RedirectToAction("Error");
				}
			}

			ViewBag.TransType = new List<SelectListItem>
				{
					new SelectListItem { Value = "入庫", Text = "入庫" },
					new SelectListItem { Value = "退貨", Text = "退貨" },
					new SelectListItem { Value = "盤差", Text = "盤差" },
				};
			return View();
		}

		// Key商品編號出現商品名稱
		// GET: GetProductName
		[HttpGet]
		public async Task<JsonResult> GetProductName(int productNo)
		{
			var product = await _context.Products
				.Where(p => p.No == productNo)
				.Select(p => new { p.Name })
				.FirstOrDefaultAsync();

			if (product == null)
				return Json(new { success = false, name = "" });

			return Json(new { success = true, name = product.Name });
		}
	}
}