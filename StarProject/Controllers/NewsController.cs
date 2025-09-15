using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			return View(items);
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

            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            return View(news);
        }

        // POST: News/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("No,Category,Title,Content,CreatedDate,PublishDate")] News news)
        {
            if (id != news.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(news);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewsExists(news.No))
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
            return View(news);
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
                _context.News.Remove(news);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.No == id);
        }
    }
}
