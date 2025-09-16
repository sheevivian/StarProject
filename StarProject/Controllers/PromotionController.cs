using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarProject.Models;
using StarProject.ViewModels;
using StarProject.Services;

namespace StarProject.Controllers
{
    public class PromotionController : Controller
    {
        private readonly StarProjectContext _context;
        private readonly IPromotionService _promotionService;

        public PromotionController(StarProjectContext context, IPromotionService promotionService)
        {
            _context = context;
            _promotionService = promotionService;
        }

        // GET: Promotion/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var promotions = await _context.Promotions.ToListAsync();
                return View(promotions);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "資料庫連線錯誤：" + ex.Message;
                return View(new List<Promotion>());
            }
        }

        // GET: Promotion/Create
        public IActionResult Create()
        {
            var vm = new PromotionFormViewModel
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };
            return View(vm);
        }

        // POST: Promotion/Create (保持原本的)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionFormViewModel vm)
        {
            // 原本的程式碼保持不變
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var promo = new Promotion
                {
                    Name = vm.Name,
                    CouponCode = vm.CouponCode,
                    Category = vm.Category,
                    StartDate = vm.StartDate,
                    EndDate = vm.EndDate,
                    Status = vm.Status,
                    Limit = vm.LimitMode == "unlimited" ? null : vm.Limit,
                    Reuse = vm.Reuse,
                    UsesTime = vm.UsesTimeMode == "unlimited" ? null : vm.UsesTime
                };

                _context.Promotions.Add(promo);
                await _context.SaveChangesAsync();

                var rule = new PromotionRule
                {
                    PromotionNo = promo.No,
                    RuleType = vm.RuleType,
                    TargetCategory = vm.Category,
                    ConditionType = vm.ConditionType,
                    ConditionAmount = vm.ConditionType == "Amount" ? vm.ConditionAmount : null,
                    MemberLevel = vm.ConditionType == "MemberLevel" ? vm.MemberLevel : null,
                    DiscountValue = vm.DiscountValue,
                    Description = vm.Description
                };

                _context.PromotionRules.Add(rule);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "優惠券新增成功！";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "儲存時發生錯誤：" + ex.Message);
                return View(vm);
            }
        }
    }
}