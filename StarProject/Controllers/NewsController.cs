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
    public class NewsController : Controller
    {
		private const int pageNumber = 10;

		private readonly StarProjectContext _context;

        public NewsController(StarProjectContext context)
        {
            _context = context;
        }

		// GET: News
		[HttpGet]
		public async Task<IActionResult> Index(int page = 1, int pageSize = pageNumber)
		{
			// 先組 IQueryable (全部資料，還沒篩選)
			var query = _context.News.Include(x => x.NewsImages).OrderByDescending(x => x.PublishDate);
			// 呼叫分頁工具
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// 轉成 ViewModel
			var newsVMList = items.Select(n => new NewsVM
			{
				No = n.No,
				Title = n.Title,
				Content = n.Content,
				PublishDate = n.PublishDate,
				CreatedDate = n.CreatedDate,
				Category = n.Category,
				Images = n.NewsImages.OrderBy(img => img.OrderNo).Select(img => img.Image).ToList()
			}).ToList();

			// 分頁資訊
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			return View(newsVMList);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View(new NewsVM());
		}

		// POST: News/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsVM model)
        {
            if (ModelState.IsValid)
            {
				// 1️⃣ 新增 News
				var news = new News
				{
					Title = model.Title,
					Content = model.Content,
					PublishDate = model.PublishDate,
					CreatedDate = DateTime.Now,
					Category = model.Category
				};

				_context.Add(news);
                await _context.SaveChangesAsync();

				if (model.ImageFiles != null && model.ImageFiles.Count > 0)
				{
					for (int i = 0; i < model.ImageFiles.Count; i++)
					{
						var file = model.ImageFiles[i];
						if (file.Length > 0)
						{
                            try
                            {
                                string imageUrl = await ImgUploadHelper.UploadToImgBB(file);

							    int orderNo = (model.ImageOrderNos != null && i < model.ImageOrderNos.Count)
										      ? model.ImageOrderNos[i]
										      : i + 1; // 預設順序依上傳順序

							    var newsImage = new NewsImage
							    {
								    NewsNo = news.No,
								    Image = imageUrl,
								    OrderNo = orderNo
							    };
							    _context.NewsImages.Add(newsImage);
                            }
                            catch (Exception ex)
							{
								ModelState.AddModelError("", $"圖片 {file.FileName} 上傳失敗: {ex.Message}");
							}
							
						}
					}

					await _context.SaveChangesAsync();
				}

				return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

	        var news = await _context.News
		        .Include(n => n.NewsImages) // 撈出圖片
		        .FirstOrDefaultAsync(n => n.No == id);

			if (news == null)
			{
				return NotFound();
			}

			// 塞進 VM
			var model = new NewsVM
			{
				No = news.No,
				Title = news.Title,
				Content = news.Content,
				Category = news.Category,
				PublishDate = news.PublishDate,
				CreatedDate = news.CreatedDate,
				// 舊圖片
				Images = news.NewsImages
				  .OrderBy(img => img.OrderNo)
				  .Select(img => img.Image)
				  .ToList(),
				ImageIds = news.NewsImages
				   .OrderBy(img => img.OrderNo)
				   .Select(img => img.No)    // ⚠ 注意這裡要拿 Id
				   .ToList(),
				ImageOrderNos = news.NewsImages
						.OrderBy(img => img.OrderNo)
						.Select(img => img.OrderNo)
						.ToList()
			};

			return View(model);
		}

        // POST: News/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NewsVM model)
        {
			var original = await _context.News.AsNoTracking().FirstOrDefaultAsync(x => x.No == id);
			if (original == null) return NotFound();

			if (id != model.No)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
            {
				var news = await _context.News
				   .Include(n => n.NewsImages)
				   .FirstOrDefaultAsync(n => n.No == id);

				if (news == null)
				{
					return NotFound();
				}

				try
				{
					// 1️⃣ 更新 News 基本資料
					news.Title = model.Title;
					news.Content = model.Content;
					news.Category = model.Category;
					news.PublishDate = model.PublishDate;
					news.CreatedDate = original.CreatedDate;

                    // 2️⃣ 刪除舊圖 (優先)
                    if (model.DeleteImageIds != null && model.DeleteImageIds.Any())
                    {
                        var toDelete = news.NewsImages
                            .Where(img => model.DeleteImageIds.Contains(img.No))
                            .ToList();
                        _context.NewsImages.RemoveRange(toDelete);
                    }

                    // 3️⃣ 更新舊圖順序
                    if (model.ImageOrderMap != null && model.ImageOrderMap.Count > 0)
                    {
                        foreach (var kv in model.ImageOrderMap) // key=ImageId, value=OrderNo
                        {
                            var img = news.NewsImages.FirstOrDefault(x => x.No == kv.Key);
                            if (img != null)
                            {
                                img.OrderNo = kv.Value;
                            }
                        }
                    }

                    // 4️⃣ 上傳新圖
                    if (model.ImageFiles != null && model.ImageFiles.Count > 0)
                    {
                        // 取得目前最大 OrderNo
                        int maxOrder = news.NewsImages.Any() ? news.NewsImages.Max(x => x.OrderNo) : 0;

                        for (int i = 0; i < model.ImageFiles.Count; i++)
                        {
                            var file = model.ImageFiles[i];
                            if (file.Length > 0)
                            {
                                try
                                {
                                    string imageUrl = await ImgUploadHelper.UploadToImgBB(file);

                                    var newsImage = new NewsImage
                                    {
                                        NewsNo = news.No,
                                        Image = imageUrl,
                                        OrderNo = ++maxOrder
                                    };
                                    _context.NewsImages.Add(newsImage);
                                }
                                catch (Exception ex)
                                {
                                    ModelState.AddModelError("", $"圖片 {file.FileName} 上傳失敗: {ex.Message}");
                                }
                            }
                        }
                    }

                    // 5️⃣ 存檔
                    await _context.SaveChangesAsync();
                }
				catch (DbUpdateConcurrencyException)
				{
					if (!NewsExists(model.No))
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
            return View(model);
        }

        // GET: News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.No == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

		// POST: News/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var news = await _context.News.FindAsync(id);

			if (news != null)
			{
				// 先找出相關的圖片
				var images = _context.NewsImages.Where(x => x.NewsNo == id);

				// 移除圖片
				_context.NewsImages.RemoveRange(images);

				// 再移除新聞
				_context.News.Remove(news);

				await _context.SaveChangesAsync();
			}

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

			// 找出要刪的新聞
			var items = await _context.News.Where(x => ids.Contains(x.No)).ToListAsync();

			if (items.Any())
			{
				// 先刪掉 NewsImage
				var images = _context.NewsImages.Where(img => ids.Contains(img.NewsNo));
				_context.NewsImages.RemoveRange(images);

				// 再刪掉 News
				_context.News.RemoveRange(items);

				await _context.SaveChangesAsync();
			}

			return Ok();
		}


		// POST: News/SearchSelect
		[HttpPost]
		public async Task<IActionResult> SearchSelect([FromBody] SearchFilterVM filters)
		{
			// 從 filters 取得 pageSize，如果沒有就給預設值
			int page = filters.Page > 0 ? filters.Page : 1;
			int pageSize = filters.PageSize >= 0 ? filters.PageSize : 10;

			var query = _context.News.AsQueryable();
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

			// 將資料轉成 ViewModel
			var newsVMList = items.Select(x => new NewsVM
			{
				No = x.No,
				Title = x.Title,
				Content = x.Content,
				PublishDate = x.PublishDate,
				CreatedDate = x.CreatedDate,
				Category = x.Category,
				// 舊圖片
				Images = _context.NewsImages
					 .Where(img => img.NewsNo == x.No)
					 .Select(img => img.Image)
					 .ToList()
			}).ToList();

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			var tableHtml = await RenderPartialViewToString("_NewsRows", newsVMList);
			var paginationHtml = await RenderPartialViewToString("_Pagination", null);

			return Json(new { tableHtml, paginationHtml });
		}

		private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.No == id);
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
