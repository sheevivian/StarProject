using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using StarProject.Helpers;
using StarProject.Models;
using StarProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public class StarMapsController : Controller
    {
        private const int pageNumber = 10;

        private readonly StarProjectContext _context;

        public StarMapsController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: StarMaps
        public async Task<IActionResult> Index(int page = 1, int pageSize = pageNumber)
        {
            // 先組 IQueryable (全部資料，還沒篩選)
            var query = _context.StarMaps.OrderByDescending(x => x.No);
            // 呼叫分頁工具
            var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

            // 把分頁資訊丟給 View
            ViewBag.Total = total;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(items);
        }

        // GET: StarMaps/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: StarMaps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StarMapVM starMap)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    starMap.Image = await ImgUploadHelper.UploadToImgBB(starMap.ImageFile);

                    StarMap map = new StarMap
                    {
                        Name = starMap.Name,
                        Desc = starMap.Desc,
                        Address = starMap.Address,
                        MapLatitude = starMap.MapLatitude,
                        MapLongitude = starMap.MapLongitude,
                        Image = starMap.Image
                    };

                    _context.Add(map);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "上傳圖片或儲存失敗：" + ex.Message);
                }
            }
            return View(starMap);
        }

        // GET: StarMaps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var starMap = await _context.StarMaps.FindAsync(id);
            if (starMap == null)
            {
                return NotFound();
            }
            // 轉成 ViewModel
            var map = new StarMapVM
            {
                No = starMap.No,
                Name = starMap.Name,
                Desc = starMap.Desc,
                Address = starMap.Address,
                MapLatitude = starMap.MapLatitude,
                MapLongitude = starMap.MapLongitude,
                Image = starMap.Image
            };
            return View(map);
        }

        // POST: StarMaps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("No,Name,Desc,Image,Address,MapLatitude,MapLongitude,ImageFile")] StarMapVM starMap)
        {
            var original = await _context.StarMaps.AsNoTracking().FirstOrDefaultAsync(x => x.No == id);
            if (original == null) return NotFound();
            starMap.Image = original.Image;

            if (id != starMap.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (starMap.ImageFile != null)
                    {
                        starMap.Image = await ImgUploadHelper.UploadToImgBB(starMap.ImageFile);
                    }
                    else
                    {
                        starMap.Image = original.Image;
                    }

                    StarMap map = new StarMap
                    {
                        No = id,
                        Name = starMap.Name,
                        Desc = starMap.Desc,
                        Address = starMap.Address,
                        MapLatitude = starMap.MapLatitude,
                        MapLongitude = starMap.MapLongitude,
                        Image = starMap.Image
                    };

                    _context.Update(map);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StarMapExists(starMap.No))
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
            return View(starMap);
        }

        // GET: StarMaps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var starMap = await _context.StarMaps
                .FirstOrDefaultAsync(m => m.No == id);
            if (starMap == null)
            {
                return NotFound();
            }

            var paginationHtml = await RenderPartialViewToString("_Pagination", null);

            return View(starMap);
        }

        // POST: StarMaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var starMap = await _context.StarMaps.FindAsync(id);
            if (starMap != null)
            {
                _context.StarMaps.Remove(starMap);
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

            var items = await _context.StarMaps.Where(x => ids.Contains(x.No)).ToListAsync();

            if (items.Any())
            {
                _context.StarMaps.RemoveRange(items);
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

            var query = _context.StarMaps.AsQueryable();
            query = query.OrderByDescending(x => x.No);

            // keyword
            if (!string.IsNullOrEmpty(filters.keyword))
            {
                query = query.Where(x => x.Name.Contains(filters.keyword)
                                     || x.Desc.Contains(filters.keyword)
                                     || x.Address.Contains(filters.keyword));
            }

            // 呼叫分頁工具
            var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

            // 把分頁資訊丟給 View
            ViewBag.Total = total;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            var tableHtml = await RenderPartialViewToString("_StarMapRows", items);
            var paginationHtml = await RenderPartialViewToString("_Pagination", null);

            return Json(new { tableHtml, paginationHtml});
        }

        private bool StarMapExists(int id)
        {
            return _context.StarMaps.Any(e => e.No == id);
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
