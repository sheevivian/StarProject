using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NuGet.Protocol.Core.Types;
using StarProject.Helpers;
using StarProject.Models;
using StarProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public class LostInfoController : Controller
    {
		private const int pageNumber = 10;

		private readonly StarProjectContext _context;

        public LostInfoController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: LostInfo
        public async Task<IActionResult> Index(int page = 1, int pageSize = pageNumber)
        {
			// 先組 IQueryable (全部資料，還沒篩選)
			var query = _context.LostInfos.OrderByDescending(x => x.CreatedDate);
			// 呼叫分頁工具
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			return View(items);
		}

        // GET: LostInfo/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LostInfo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LostInfoVM lostInfo)
        {
            if (ModelState.IsValid)
            {
                try
                {
					lostInfo.Image = await ImgUploadHelper.UploadToImgBB(lostInfo.ImageFile);

					LostInfo lost = new LostInfo
					{
						Name = lostInfo.Name,
						Category = lostInfo.Category,
						Desc = lostInfo.Desc,
						Status = lostInfo.Status,
						FoundDate = lostInfo.FoundDate,
						CreatedDate = DateTime.Now,
						OwnerName = lostInfo.OwnerName,
						OwnerPhone = lostInfo.OwnerPhone,
						Image = lostInfo.Image
					};

					_context.Add(lost);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
                catch (Exception ex)
				{
					ModelState.AddModelError("", "上傳圖片或儲存失敗：" + ex.Message);
				}
			}
            return View(lostInfo);
        }

        // GET: LostInfo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lostInfo = await _context.LostInfos.FindAsync(id);
            if (lostInfo == null)
            {
                return NotFound();
            }
			// 轉成 ViewModel
			var vm = new LostInfoVM
			{
				No = lostInfo.No,
				Name = lostInfo.Name,
				Category = lostInfo.Category,
				Desc = lostInfo.Desc,
				Image = lostInfo.Image,
				Status = lostInfo.Status,
				FoundDate = lostInfo.FoundDate,
				CreatedDate = lostInfo.CreatedDate,
				OwnerName = lostInfo.OwnerName,
				OwnerPhone = lostInfo.OwnerPhone
			};

			return View(vm);
		}

        // POST: LostInfo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("No,Name,Category,Desc,Image,Status,FoundDate,CreatedDate,OwnerName,OwnerPhone,ImageFile")] LostInfoVM lostInfo)
        {
			var original = await _context.LostInfos.AsNoTracking().FirstOrDefaultAsync(x => x.No == id);
			if (original == null) return NotFound();

			// 保持原本的 CreatedDate，不讓它被編輯
			lostInfo.CreatedDate = original.CreatedDate;
			lostInfo.Image = original.Image;

			if (id != lostInfo.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (lostInfo.ImageFile != null)
                    {
						lostInfo.Image = await ImgUploadHelper.UploadToImgBB(lostInfo.ImageFile);
                    }
                    else
                    {
                        lostInfo.Image = original.Image;
					}

					LostInfo lost = new LostInfo
					{
                        No = id,
						Name = lostInfo.Name,
						Category = lostInfo.Category,
						Desc = lostInfo.Desc,
						Status = lostInfo.Status,
						FoundDate = lostInfo.FoundDate,
						CreatedDate = original.CreatedDate,
						OwnerName = lostInfo.OwnerName,
						OwnerPhone = lostInfo.OwnerPhone,
						Image = lostInfo.Image
					};

					_context.Update(lost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LostInfoExists(lostInfo.No))
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
            return View(lostInfo);
        }

        // GET: LostInfo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lostInfo = await _context.LostInfos
                .FirstOrDefaultAsync(m => m.No == id);
            if (lostInfo == null)
            {
                return NotFound();
            }

			var paginationHtml = await RenderPartialViewToString("_Pagination", null);

			return View(lostInfo);
        }

        // POST: LostInfo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lostInfo = await _context.LostInfos.FindAsync(id);
            if (lostInfo != null)
            {
                _context.LostInfos.Remove(lostInfo);
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

            var items = await _context.LostInfos.Where(x => ids.Contains(x.No)).ToListAsync();
			
            if (items.Any())
			{
				_context.LostInfos.RemoveRange(items);
				await _context.SaveChangesAsync();
			}

			return Ok();
		}

		// POST: LostInfo/SearchSelect
		[HttpPost]
		public async Task<IActionResult> SearchSelect([FromBody] SearchFilterVM filters)
		{
			// 從 filters 取得 pageSize，如果沒有就給預設值
			int page = filters.Page > 0 ? filters.Page : 1;
			int pageSize = filters.PageSize >=0 ? filters.PageSize : 10;

			var query = _context.LostInfos.AsQueryable();
			query = query.OrderByDescending(x => x.CreatedDate);

			// keyword
			if (!string.IsNullOrEmpty(filters.keyword))
			{
				query = query.Where(x => x.Name.Contains(filters.keyword)
									 || x.Category.Contains(filters.keyword)
									 || x.Status.Contains(filters.keyword));
			}

			// 分類
			if (filters.Categories != null && filters.Categories.Any())
				query = query.Where(x => filters.Categories.Contains(x.Category));

			// 狀態
			if (filters.Statuses != null && filters.Statuses.Any())
				query = query.Where(x => filters.Statuses.Contains(x.Status));

			// 日期區間
			if (!string.IsNullOrEmpty(filters.DateFrom))
				query = query.Where(x => x.FoundDate >= DateTime.Parse(filters.DateFrom));

			if (!string.IsNullOrEmpty(filters.DateTo))
				query = query.Where(x => x.FoundDate <= DateTime.Parse(filters.DateTo));


			// 呼叫分頁工具
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			var tableHtml = await RenderPartialViewToString("_LostInfoRows", items);
			var paginationHtml = await RenderPartialViewToString("_Pagination", null);

			return Json(new { tableHtml, paginationHtml });
		}

		private bool LostInfoExists(int id)
        {
            return _context.LostInfos.Any(e => e.No == id);
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
