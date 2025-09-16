using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StarProject.Helpers;
using StarProject.Models;
using StarProject.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public class FaqsController : Controller
    {
		private const int pageNumber = 10;

		private readonly StarProjectContext _context;

        public FaqsController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: Faqs
        public async Task<IActionResult> Index(int page = 1, int pageSize = pageNumber)
		{

			var query = _context.Faqs
                            .OrderBy(x => x.No)
                            .Include(f => f.CategoryNoNavigation);
			// 呼叫分頁工具
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			return View(items);
		}

        // GET: Faqs/Create
        public IActionResult Create()
        {
            ViewData["CategoryNo"] = new SelectList(_context.Faqcategories, "No", "Name");
            return View();
        }

        // POST: Faqs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("No,CategoryNo,Question,Answer,UpdateDate")] Faq faq)
        {
            if (ModelState.IsValid)
            {
				faq.UpdateDate = DateTime.Now;

				_context.Add(faq);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryNo"] = new SelectList(_context.Faqcategories, "No", "Name", faq.CategoryNo);
            return View(faq);
        }

        // GET: Faqs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faq = await _context.Faqs.FindAsync(id);
            if (faq == null)
            {
                return NotFound();
            }
            ViewData["CategoryNo"] = new SelectList(_context.Faqcategories, "No", "Name", faq.CategoryNo);
            return View(faq);
        }

        // POST: Faqs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("No,CategoryNo,Question,Answer,UpdateDate")] Faq faq)
        {
            if (id != faq.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					faq.UpdateDate = DateTime.Now;

					_context.Update(faq);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FaqExists(faq.No))
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
            ViewData["CategoryNo"] = new SelectList(_context.Faqcategories, "No", "Name", faq.CategoryNo);
            return View(faq);
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteMultiple(int[] ids)
		{
			if (ids == null || ids.Length == 0)
			{
				return BadRequest();
			}

			var items = await _context.Faqs.Where(x => ids.Contains(x.No)).ToListAsync();

			if (items.Any())
			{
				_context.Faqs.RemoveRange(items);
				await _context.SaveChangesAsync();
			}

			return Ok();
		}

		// POST: StarMaps/SearchSelect
		[HttpPost]
		public async Task<IActionResult> SearchSelect([FromBody] SearchFilterVM filters)
		{
			// 從 filters 取得 pageSize，如果沒有就給預設值
			int page = filters.Page > 0 ? filters.Page : 1;
			int pageSize = filters.PageSize >= 0 ? filters.PageSize : 10;

			var query = _context.Faqs.Include(f => f.CategoryNoNavigation).AsQueryable();
			query = query.OrderBy(x => x.No);

			// keyword
			if (!string.IsNullOrEmpty(filters.keyword))
			{
				query = query.Where(x => x.Question.Contains(filters.keyword)
									 || x.CategoryNoNavigation.Name.Contains(filters.keyword));
			}

			// 分類模糊搜尋
			if (filters.Categories != null && filters.Categories.Any())
			{
				query = query.Where(x => filters.Categories.Any(c => x.CategoryNoNavigation.Name.Contains(c)));
			}

			// 呼叫分頁工具
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			var tableHtml = await RenderPartialViewToString("_FaqsRows", items);
			var paginationHtml = await RenderPartialViewToString("_Pagination", null);

			return Json(new { tableHtml, paginationHtml });
		}

		private bool FaqExists(int id)
        {
            return _context.Faqs.Any(e => e.No == id);
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
