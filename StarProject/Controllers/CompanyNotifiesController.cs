using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StarProject.Helpers;
using StarProject.Models;
using StarProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace StarProject.Controllers
{
    public class CompanyNotifiesController : Controller
    {

		private const int pageNumber = 10;

		private readonly StarProjectContext _context;

        public CompanyNotifiesController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: CompanyNotifies
        public async Task<IActionResult> Index(int page = 1, int pageSize = pageNumber)
		{
			// 先組 IQueryable (全部資料，還沒篩選)
			var query = _context.CompanyNotifies.OrderByDescending(x => x.PublishDate);
			// 呼叫分頁工具
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			return View(items);
		}

		// GET: CompanyNotifies/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyNotify = await _context.CompanyNotifies
                .FirstOrDefaultAsync(m => m.No == id);
            if (companyNotify == null)
            {
                return NotFound();
            }

            return View(companyNotify);
        }

        // GET: CompanyNotifies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CompanyNotifies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("No,Title,Content,Category,PublishDate")] CompanyNotify companyNotify)
        {
			if (ModelState.IsValid)
			{
				// 由程式自動生成現在時間
				companyNotify.PublishDate = DateTime.Now;

				_context.Add(companyNotify);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(companyNotify);
		}

        // GET: CompanyNotifies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyNotify = await _context.CompanyNotifies.FindAsync(id);
            if (companyNotify == null)
            {
                return NotFound();
            }
            return View(companyNotify);
        }

        // POST: CompanyNotifies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("No,Title,Content,Category,PublishDate")] CompanyNotify companyNotify)
        {
			var original = await _context.CompanyNotifies.AsNoTracking().FirstOrDefaultAsync(x => x.No == id);
			if (original == null) return NotFound();


			if (id != companyNotify.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					// 由程式自動生成現在時間
					companyNotify.PublishDate = original.PublishDate;

					_context.Update(companyNotify);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyNotifyExists(companyNotify.No))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(companyNotify);
        }

        // GET: CompanyNotifies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyNotify = await _context.CompanyNotifies
                .FirstOrDefaultAsync(m => m.No == id);
            if (companyNotify == null)
            {
                return NotFound();
            }

			var paginationHtml = await RenderPartialViewToString("_Pagination", null);

			return View(companyNotify);
		}

        // POST: CompanyNotifies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var companyNotify = await _context.CompanyNotifies.FindAsync(id);
            if (companyNotify != null)
            {
                _context.CompanyNotifies.Remove(companyNotify);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteMultiple(int[] ids)
		{
			if (ids == null || ids.Length == 0)
			{
				return BadRequest();
			}

			var items = await _context.CompanyNotifies.Where(x => ids.Contains(x.No)).ToListAsync();

			if (items.Any())
			{
				_context.CompanyNotifies.RemoveRange(items);
				await _context.SaveChangesAsync();
			}

			return Ok();
		}

        // POST: CompanyNotifies/SearchSelect
        [HttpPost]
		public async Task<IActionResult> SearchSelect([FromBody] SearchFilterVM filters)
		{
			// 從 filters 取得 pageSize，如果沒有就給預設值
			int page = filters.Page > 0 ? filters.Page : 1;
			int pageSize = filters.PageSize >= 0 ? filters.PageSize : 10;

			var query = _context.CompanyNotifies.AsQueryable();
			query = query.OrderByDescending(x => x.PublishDate);

			// keyword
			if (!string.IsNullOrEmpty(filters.keyword))
			{
				query = query.Where(x => x.Title.Contains(filters.keyword)
									 || x.Category.Contains(filters.keyword));
			}

			// 分類
			if (filters.Categories != null && filters.Categories.Any())
				query = query.Where(x => filters.Categories.Contains(x.Category));

			// 日期區間
			if (!string.IsNullOrEmpty(filters.DateFrom))
				query = query.Where(x => x.PublishDate >= DateTime.Parse(filters.DateFrom));

			if (!string.IsNullOrEmpty(filters.DateTo))
				query = query.Where(x => x.PublishDate <= DateTime.Parse(filters.DateTo));


			// 呼叫分頁工具
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			var tableHtml = await RenderPartialViewToString("_CompanyNotifyRows", items);
			var paginationHtml = await RenderPartialViewToString("_Pagination", null);

			return Json(new { tableHtml, paginationHtml });
		}

		private bool CompanyNotifyExists(int id)
        {
            return _context.CompanyNotifies.Any(e => e.No == id);
        }

		//POST: CompanyNotifies/UploadImage
		[HttpPost]
		public async Task<IActionResult> UploadImage(IFormFile upload)
		{
			if (upload == null || upload.Length == 0)
				return Json(new { uploaded = 0, error = new { message = "No file uploaded." } });

			try
			{
				// 上傳到 ImgBB
				string url = await ImgUploadHelper.UploadToImgBB(upload);

				// 回傳 CKEditor 可用 JSON
				return Json(new { uploaded = 1, url = url });
			}
			catch (Exception ex)
			{
				return Json(new { uploaded = 0, error = new { message = ex.Message } });
			}
		}

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

	}
}
