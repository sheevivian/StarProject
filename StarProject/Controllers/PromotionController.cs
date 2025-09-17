using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StarProject.Models;
using StarProject.ViewModels;
using System.Text;
using StarProject.DTOs.PromotionDTOs;

namespace StarProject.Controllers
{
    public class PromotionController : Controller
    {
        private readonly StarProjectContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public PromotionController(StarProjectContext context, ICompositeViewEngine viewEngine)
        {
            _context = context;
            _viewEngine = viewEngine;
        }

        public IActionResult Index()
        {
            return View(Enumerable.Empty<PromotionListDto>());
        }

        // 列表分頁/搜尋
        [HttpGet]
        public async Task<IActionResult> GetPagedData(int page = 1, int pageSize = 10, string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Promotions
                .AsNoTracking()                       
                .Select(PromotionListDto.Projection); 

            if (!string.IsNullOrWhiteSpace(search))
            {
                var key = search.Trim();
                query = query.Where(p =>
                    (p.Name ?? "").Contains(key) ||
                    (p.CouponCode ?? "").Contains(key) ||
                    (p.Category ?? "").Contains(key)
                );
            }

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var data = await query
                .OrderBy(p => p.No)                   // ★ 明確排序，分頁穩定
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var rowsHtml = await RenderPartialViewToStringAsync("_PromotionRows", data);

            ViewBag.Total = total;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            var paginationHtml = await RenderPartialViewToStringAsync("_Pagination", model: null);

            return Json(new { rows = rowsHtml, pagination = paginationHtml });
        }

        // ============ Create ============

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new PromotionFormViewModel
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7)
            };

            // ★ 從 DB 撈已存在的類別，交給 VM 填入選項
            var dbCategories = await _context.Promotions
                .AsNoTracking()
                .Select(p => p.Category)
                .Where(c => c != null && c != "")
                .Distinct()
                .ToListAsync();

            vm.FillCategoryOptions(dbCategories);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // ★ 再次填回選項（避免回傳 View 時下拉變空）
                var dbCategories = await _context.Promotions
                    .AsNoTracking()
                    .Select(p => p.Category)
                    .Where(c => c != null && c != "")
                    .Distinct()
                    .ToListAsync();
                vm.FillCategoryOptions(dbCategories);
                return View(vm);
            }

            // ★ 交由 VM 正規化欄位值
            vm.NormalizeModes();

            // ★ VM 映射成 Entity
            var entity = vm.ToEntity();
            _context.Promotions.Add(entity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "新增成功";
            return RedirectToAction(nameof(Index));
        }

        // 供 Create.cshtml 的 fetch 檢查重複
        [HttpPost]
        public async Task<IActionResult> CheckDuplicate(string? couponCode, string? name)
        {
            var code = (couponCode ?? "").Trim().ToUpper();
            var nm = (name ?? "").Trim();

            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(nm))
                return Json(new { isDuplicate = false });

            var isDup = await _context.Promotions.AnyAsync(p =>
                (!string.IsNullOrEmpty(code) && p.CouponCode == code) ||
                (!string.IsNullOrEmpty(nm) && p.Name == nm));

            return Json(new { isDuplicate = isDup });
        }

        // ============ Edit ============

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var entity = await _context.Promotions.FindAsync(id);
            if (entity == null)
                return NotFound();

            // Entity → VM
            var vm = PromotionFormViewModel.FromEntity(entity);

            // 填充類別選項
            var dbCategories = await _context.Promotions
                .AsNoTracking()
                .Select(p => p.Category)
                .Where(c => c != null && c != "")
                .Distinct()
                .ToListAsync();
            vm.FillCategoryOptions(dbCategories);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PromotionFormViewModel vm)
        {
            if (id != vm.No)
                return NotFound();

            if (!ModelState.IsValid)
            {
                // 再次填回選項
                var dbCategories = await _context.Promotions
                    .AsNoTracking()
                    .Select(p => p.Category)
                    .Where(c => c != null && c != "")
                    .Distinct()
                    .ToListAsync();
                vm.FillCategoryOptions(dbCategories);
                return View(vm);
            }

            // 正規化欄位值
            vm.NormalizeModes();

            // 找出既有實體
            var existing = await _context.Promotions.FindAsync(id);
            if (existing == null)
                return NotFound();

            // VM 更新到 Entity
            var updated = vm.ToEntity(existing);

            try
            {
                _context.Update(updated);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "修改成功";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PromotionExists(vm.No))
                    return NotFound();
                else
                    throw;
            }
        }

        // ============ Delete ============

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
                return Json(new { success = false, message = "找不到該優惠券" });

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "刪除成功" });
        }

        // 供 Create.cshtml 和 Edit.cshtml 的 fetch 檢查重複
        [HttpPost]
        public async Task<IActionResult> CheckDuplicate(string? couponCode, string? name, int? id)
        {
            var code = (couponCode ?? "").Trim().ToUpper();
            var nm = (name ?? "").Trim();

            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(nm))
                return Json(new { isDuplicate = false });

            var query = _context.Promotions.AsQueryable();

            // 如果是編輯模式，排除自己
            if (id.HasValue)
                query = query.Where(p => p.No != id.Value);

            var isDup = await query.AnyAsync(p =>
                (!string.IsNullOrEmpty(code) && p.CouponCode == code) ||
                (!string.IsNullOrEmpty(nm) && p.Name == nm));

            return Json(new { isDuplicate = isDup });
        }

        private async Task<bool> PromotionExists(int id)
        {
            return await _context.Promotions.AnyAsync(e => e.No == id);
        }


        // ============ 共用：Partial 渲染 / 分頁 HTML ============

        private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            await using var sw = new StringWriter();
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
            if (viewResult.View == null)
                throw new ArgumentNullException($"{viewName} not found");

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

        private static string GeneratePaginationHtml(int currentPage, int totalPages)
        {
            var sb = new StringBuilder();

            var prevDisabled = currentPage <= 1 ? " disabled" : "";
            var prevPage = currentPage > 1 ? currentPage - 1 : 1;
            sb.Append($"<li class='page-item{prevDisabled}'><a class='page-link' href='javascript:void(0);' onclick='loadData({prevPage})'>&laquo;</a></li>");

            for (int i = 1; i <= totalPages; i++)
            {
                var active = i == currentPage ? " active" : "";
                sb.Append($"<li class='page-item{active}'><a class='page-link' href='javascript:void(0);' onclick='loadData({i})'>{i}</a></li>");
            }

            var nextDisabled = currentPage >= totalPages ? " disabled" : "";
            var nextPage = currentPage < totalPages ? currentPage + 1 : totalPages;
            sb.Append($"<li class='page-item{nextDisabled}'><a class='page-link' href='javascript:void(0);' onclick='loadData({nextPage})'>&raquo;</a></li>");

            return sb.ToString();
        }
    }
}
