using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StarProject.DTOs.PromotionDTOs; // ✅ 保持你的命名空間
using StarProject.Models;
using StarProject.ViewModels;
using System.Text;

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
                .OrderBy(p => p.No)
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
            var now = DateTime.Now;
            var start = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
            var end = start.AddDays(7);

            var vm = new PromotionFormViewModel
            {
                StartDate = start,
                EndDate = end,
                UsesTime = 1,            // ✅ 預設每人 1 次
                UsesTimeMode = "limited" // ✅ 讓欄位與模式一致
            };

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
            // 回填選單
            var dbCategories = await _context.Promotions
                .AsNoTracking()
                .Select(p => p.Category)
                .Where(c => c != null && c != "")
                .Distinct()
                .ToListAsync();
            vm.FillCategoryOptions(dbCategories);

            if (!ModelState.IsValid) return View(vm);

            vm.NormalizeModes();

            // ✅ 伺服器端重複檢查
            var duplicateCode = await _context.Promotions.AnyAsync(p => p.CouponCode == vm.CouponCode);
            var duplicateName = await _context.Promotions.AnyAsync(p => p.Name == vm.Name);
            if (duplicateCode) ModelState.AddModelError(nameof(vm.CouponCode), "優惠代碼已存在！");
            if (duplicateName) ModelState.AddModelError(nameof(vm.Name), "優惠名稱已存在！");
            if (!ModelState.IsValid) return View(vm);

            // ✅ 商業驗證（啟用期間需涵蓋現在；可重複 -> 每人 >= 2）
            var businessErrors = vm.ValidateBusinessRules(DateTime.Now);
            if (businessErrors.Any())
            {
                foreach (var e in businessErrors) ModelState.AddModelError(string.Empty, e);
                ViewBag.PopupError = string.Join("\n", businessErrors); // ✅ 交給 View 用 Modal 顯示
                return View(vm);
            }

            // ✅ 修正：用 execution strategy 包住「使用者交易」
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Promotion
                    var entity = vm.ToEntity();
                    _context.Promotions.Add(entity);
                    await _context.SaveChangesAsync(); // 取得 entity.No

                    // PromotionRule
                    var rule = vm.ToRuleEntity(entity.No);
                    _context.PromotionRules.Add(rule);
                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });

            TempData["SuccessMessage"] = "新增成功";
            return RedirectToAction(nameof(Index));
        }

        // ============ 重複檢查（Create/ Edit 共用）===========
        [HttpPost]
        public async Task<IActionResult> CheckDuplicate(string? couponCode, string? name, int? id)
        {
            var code = (couponCode ?? "").Trim().ToUpper();
            var nm = (name ?? "").Trim();

            var query = _context.Promotions.AsQueryable();

            // ✅ 正確邏輯：
            // - Create 時：id 是 null，檢查所有記錄
            // - Edit 時：id 有值，排除自己，檢查其他記錄
            if (id.HasValue)
                query = query.Where(p => p.No != id.Value);

            var duplicateCode = !string.IsNullOrEmpty(code) &&
                               await query.AnyAsync(p => p.CouponCode == code);
            var duplicateName = !string.IsNullOrEmpty(nm) &&
                               await query.AnyAsync(p => p.Name == nm);

            return Json(new { duplicateCode, duplicateName });
        }

        // ============ Edit ============

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _context.Promotions.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = PromotionFormViewModel.FromEntity(entity);

            // ✅ 載入對應規則
            var rule = await _context.PromotionRules
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Promotion_No == entity.No);
            vm.ApplyRule(rule);

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
            if (id != vm.No) return NotFound();

            var dbCategories = await _context.Promotions
                .AsNoTracking()
                .Select(p => p.Category)
                .Where(c => c != null && c != "")
                .Distinct()
                .ToListAsync();
            vm.FillCategoryOptions(dbCategories);

            if (!ModelState.IsValid) return View(vm);

            vm.NormalizeModes();

            // ✅ 伺服器端重複檢查（排除自己）
            var duplicateCode = await _context.Promotions.AnyAsync(p => p.No != id && p.CouponCode == vm.CouponCode);
            var duplicateName = await _context.Promotions.AnyAsync(p => p.No != id && p.Name == vm.Name);
            if (duplicateCode) ModelState.AddModelError(nameof(vm.CouponCode), "優惠代碼已存在！");
            if (duplicateName) ModelState.AddModelError(nameof(vm.Name), "優惠名稱已存在！");
            if (!ModelState.IsValid) return View(vm);

            // ✅ 商業驗證
            var businessErrors = vm.ValidateBusinessRules(DateTime.Now);
            if (businessErrors.Any())
            {
                foreach (var e in businessErrors) ModelState.AddModelError(string.Empty, e);
                ViewBag.PopupError = string.Join("\n", businessErrors);
                return View(vm);
            }

            // ✅ 修正：execution strategy + user transaction
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _context.Promotions.FindAsync(id);
                    if (existing == null) throw new InvalidOperationException("Promotion not found.");

                    vm.ToEntity(existing);
                    _context.Update(existing);
                    await _context.SaveChangesAsync();

                    var rule = await _context.PromotionRules.FirstOrDefaultAsync(r => r.Promotion_No == id);
                    if (rule == null)
                    {
                        rule = vm.ToRuleEntity(id);
                        _context.PromotionRules.Add(rule);
                    }
                    else
                    {
                        vm.ToRuleEntity(id, rule); // 覆寫欄位
                        _context.PromotionRules.Update(rule);
                    }
                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });

            TempData["SuccessMessage"] = "修改成功";
            return RedirectToAction(nameof(Index));
        }

        // ============ Delete ============

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
                return Json(new { success = false, message = "找不到該優惠券" });

            var rules = _context.PromotionRules.Where(r => r.Promotion_No == id);
            _context.PromotionRules.RemoveRange(rules);

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "刪除成功" });
        }

        // ============ 共用：Partial 渲染 ============

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
    }
}
