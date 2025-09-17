using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StarProject.DTOs.PromotionDTOs;
using StarProject.Models;

namespace StarProject.Controllers
{
    public class PromotionAnalysisController : Controller
    {
        private readonly StarProjectContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public PromotionAnalysisController(StarProjectContext context, ICompositeViewEngine viewEngine)
        {
            _context = context;
            _viewEngine = viewEngine;
        }

        public IActionResult Index()
        {
            // ✅ 修改：型別對齊 View（使用 DTO），也避免 null
            return View(Enumerable.Empty<PromotionAnalysisDto>());
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(int page = 1, int pageSize = 10, string? search = null)
        {
            if (page < 1) page = 1;          // ✅ 邊界保護
            if (pageSize <= 0) pageSize = 10;

            // ✅ 修改：改用 DTO 的查詢組裝（AsNoTracking 提升效能）
            var baseQuery = PromotionAnalysisDto.BuildQuery(
                _context.Promotions.AsNoTracking(),
                _context.PromotionUsages.AsNoTracking(),
                search
            );

            var total = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var data = await baseQuery
                .OrderBy(p => p.PromotionNo) // ✅ 固定排序
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ 渲染資料列（注意：Rows partial 也會調整型別 → DTO）
            var rowsHtml = await RenderPartialViewToStringAsync("_PromotionAnalysisRows", data);

            // ✅ 改用共用分頁 Partial（Views/Shared/_Pagination.cshtml）
            ViewBag.Total = total;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;

            var paginationHtml = await RenderPartialViewToStringAsync("_Pagination", model: null);

            return Json(new { rows = rowsHtml, pagination = paginationHtml });
        }

        // 共用：將 Partial 渲染成字串，給 AJAX 回傳
        private async Task<string> RenderPartialViewToStringAsync(string viewName, object? model)
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

        // ❌ 已移除：GeneratePaginationHtml
        // ✅ 原因：統一使用 Shared/_Pagination，樣式一致，Controller 更乾淨
    }
}
