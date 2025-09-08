using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Graph;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using StarProject.Models;
using StarProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly StarProjectContext _context;

        public ProductController(StarProjectContext context)
        {
            _context = context;
        }

        // GET: Product
        [HttpGet]
		public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            // var starProjectContext = _context.Products.Include(p => p.ProCategoryNoNavigation);
            // return View(await starProjectContext.ToListAsync());

			// return View(_context.Products.Include(p => p.ProCategoryNoNavigation));

			// 總筆數
			var totalCount = await _context.Products.CountAsync();

			// 計算要跳過幾筆
			var products = await _context.Products
				.Include(p => p.ProCategoryNoNavigation)
				.OrderBy(p => p.No)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// 準備 ViewModel
			var viewModel = new ProductListViewModel
			{
				Products = products,
				CurrentPage = page,
				PageSize = pageSize,
				TotalCount = totalCount
			};

			return View(viewModel);
		}

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProCategoryNoNavigation)
                .FirstOrDefaultAsync(m => m.No == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

		// 新品上架-單筆(GET)
		// GET: Product/Create
		public IActionResult Create()
        {
            ViewData["ProCategoryName"] = new SelectList(_context.ProCategories, "No", "Name");
			ViewBag.StatusList = new List<SelectListItem>
                {
	                new SelectListItem { Value = "上架", Text = "上架" },
	                new SelectListItem { Value = "預購", Text = "預購" },
                };
			return View();
        }

		// 新品上架-單筆(POST)
		// POST: Product/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Name,ProCategoryNo,Price,Status,ReleaseDate")] Product proModel)
		{
			int lastNo = _context.Products.Max(p => (int?)p.No) ?? 99999;
			proModel.No = lastNo + 1;   // 新商品編號

			proModel.UpdateDate = DateTime.Now;
			try
			{
				_context.Add(proModel);
				await _context.SaveChangesAsync();

				return RedirectToAction("Edit", new { id = proModel.No });
			}
			catch (Exception ex)
			{
				// 這裡把錯誤訊息寫進 ModelState，讓畫面上可以看到
				ModelState.AddModelError("", $"資料存檔失敗: {ex.Message}");
			}

			// 如果失敗就回到 Create 畫面，顯示錯誤
			ViewData["ProCategoryName"] = new SelectList(_context.ProCategories, "No", "Name");
			ViewBag.StatusList = new List<SelectListItem>
			{
				new SelectListItem { Value = "上架", Text = "上架" },
				new SelectListItem { Value = "預購", Text = "預購" },
			};

			return View(proModel);
		}

		[HttpGet]
		// GET: Product/DownloadTemplate
		public IActionResult DownloadTemplate()
		{
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exceltemps","ProductTemplate.xlsx");
			var fileBytes = System.IO.File.ReadAllBytes(filePath);
			return File(fileBytes,
						"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
						"ProductTemplate.xlsx");
		}

		// 新品上架-多筆(POST)
		// POST: Product/CreateMultiple
		[HttpPost]
		public async Task<IActionResult> CreateMultiple(IFormFile excelFile)
		{
			if (excelFile == null || excelFile.Length == 0)
			{
				ModelState.AddModelError("", "請選擇檔案");
				return View();
			}

			var products = new List<Product>();

			using (var stream = excelFile.OpenReadStream())
			{
				IWorkbook workbook;
				if (Path.GetExtension(excelFile.FileName).ToLower() == ".xls")
				{
					workbook = new HSSFWorkbook(stream); // 2003 格式
				}
				else
				{
					workbook = new XSSFWorkbook(stream); // 2007+
				}

				var sheet = workbook.GetSheetAt(0); // 讀第一個工作表

				int lastNo = _context.Products.Max(p => (int?)p.No) ?? 99999;

				for (int row = 2; row <= sheet.LastRowNum; row++) // 從第 3 列開始 (第1列標題、第2列範例)
				{
					var currentRow = sheet.GetRow(row);
					if (currentRow == null) continue;
					lastNo++; // 每次加 1，避免重複
					var product = new Product
					{
						No = lastNo,
						Name = currentRow.GetCell(0)?.ToString(),
						ProCategoryNo = currentRow.GetCell(1)?.ToString(),
						Price = decimal.TryParse(currentRow.GetCell(2)?.ToString(), out var price) ? price : 0,
						Status = currentRow.GetCell(3)?.ToString(),
						ReleaseDate = DateTime.TryParse(currentRow.GetCell(4)?.ToString(), out var dt) ? dt : DateTime.Now,
						UpdateDate = DateTime.Now
					};
					products.Add(product);
				}
				_context.Products.AddRange(products);
				await _context.SaveChangesAsync();
			}
			TempData["Success"] = $"成功匯入 {products.Count} 筆資料";
			return RedirectToAction("Index");
		}


		// GET: Product/Edit/5
		[HttpGet]
		public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.No == id);
			var images = _context.ProductImages
					 .Where(i => i.ProductNo == id)
					 .OrderBy(i => i.ImgOrder)
					 .ToList();

			var vm = new ProductEditViewModel
			{
				Product = product,
				ProImages = images
			};
			return View(vm); // 同一個 View
		}

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("No,Name,ProCategoryNo,Price,Status,ReleaseDate,UpdateDate")] Product product)
        {
            if (id != product.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.No))
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
            ViewData["ProCategoryNo"] = new SelectList(_context.ProCategories, "No", "No", product.ProCategoryNo);
            return View(product);
        }

		// GET: Product/ImgEdit/5
		[HttpGet]
		public async Task<IActionResult> ImgEdit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound();
			}
			return View(product);
		}

		// POST: Product/ImgEdit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ImgEdit(int id, [Bind("ProductNo,Image,ImgOrder")] ProductImage proImg)
		{
			if (id != proImg.ProductNoNavigation.No)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(proImg);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ProductExists(proImg.ProductNoNavigation.No))
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

			return View(proImg);
		}


		// GET: Product/Delete/5
		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProCategoryNoNavigation)
                .FirstOrDefaultAsync(m => m.No == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.No == id);
        }
    }
}
