using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Graph;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using StarProject.Helpers;
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

		// 商品查詢+頁數
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

		// 新品上架-多筆(GET)
		// GET: Product/DownloadTemplate
		[HttpGet]
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

		// 編輯商品(GET)
		// GET: Product/Edit/5
		[HttpGet]
		public IActionResult Edit(int id)
        {
            var product = _context.Products
								  .Include(p => p.ProCategoryNoNavigation)
								  .FirstOrDefault(p => p.No == id);
			var images = _context.ProductImages
					 .Where(i => i.ProductNo == id)
					 .OrderBy(i => i.ImgOrder)
					 .ToList();

			var vm = new ProductEditViewModel
			{
				Product = product,
				ProImages = images
			};

			ViewData["ProCategoryName"] = new SelectList(_context.ProCategories, "No", "Name", product.ProCategoryNo);
			ViewBag.StatusList = new SelectList(
				new List<SelectListItem>
				{
					new SelectListItem { Value = "上架", Text = "上架" },
					new SelectListItem { Value = "預購", Text = "預購" },
				},
				"Value", "Text", product.Status);
			return View(vm); // 同一個 View
		}

		// 上傳照片(POST)
		// POST: Product/ImgUpload
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ImgUpload(ProductEditViewModel proEditVM)
		{
			if (proEditVM.ImageFiles == null)
			{
				ModelState.AddModelError("", "請選擇要上傳的圖片");
				return View(proEditVM); // 記得 return，不然會繼續往下跑
			}

			// 確保ProImage不為null
			if (proEditVM.ProImage == null)
			{
				proEditVM.ProImage = new ProductImage();
			}

			for (int i = 0; i < proEditVM.ImageFiles.Count; i++)
			{
				var file = proEditVM.ImageFiles[i];
				var imgUrl = await ImgUploadHelper.UploadToImgBB(file);

				var proImg = new ProductImage
				{
					ProductNo = proEditVM.Product.No,
					Image = imgUrl,
					ImgOrder = proEditVM.ImageOrders?[i] ?? i + 1
				};

				_context.ProductImages.Add(proImg);
				await _context.SaveChangesAsync();
			}

			var images = _context.ProductImages
				.Where(i => i.ProductNo == proEditVM.Product.No)
				.OrderBy(i => i.ImgOrder)
				.ToList();

			var vm = new ProductEditViewModel
			{
				Product = proEditVM.Product,
				ProImage = new ProductImage { ProductNo = proEditVM.Product.No },
				ProImages = images
			};

			return PartialView("_PicturePartial", vm);
			// return RedirectToAction(nameof(Edit), new { id = proEditVM.ProImage.ProductNo });
		}

		// 刪除照片(POST)
		// POST: Product/ImgDelete
		[HttpPost]
		public IActionResult ImgDelete(int id, int productNo)
		{
			var img = _context.ProductImages.FirstOrDefault(x => x.No == id);
			if (img != null)
			{
				_context.ProductImages.Remove(img);
				_context.SaveChanges();
			}

			// 刪除後重新撈圖片
			var images = _context.ProductImages
				.Where(i => i.ProductNo == productNo)
				.OrderBy(i => i.ImgOrder)
				.ToList();

			var vm = new ProductEditViewModel
			{
				Product = _context.Products.FirstOrDefault(p => p.No == productNo),
				ProImage = new ProductImage { ProductNo = productNo },
				ProImages = images
			};

			return PartialView("_PicturePartial", vm);
		}

		public class ImgOrderDto
		{
			public int OrderId { get; set; }  // 圖片 ID
			public int OrderOd { get; set; }  // 新的排序
		}

		public class ImgSaveDto
		{
			public List<ImgOrderDto> ImgData { get; set; }
			public List<int> DeletedIds { get; set; }
		}

		// 更新照片區(POST)
		[HttpPost]
		public async Task<IActionResult> ImgSave([FromBody] ImgSaveDto imgSaveDto)
		{
			// 刪除資料庫圖片
			if (imgSaveDto.DeletedIds != null && imgSaveDto.DeletedIds.Any())
			{
				var imgsToDelete = _context.ProductImages
										   .Where(i => imgSaveDto.DeletedIds.Contains(i.No));
				_context.ProductImages.RemoveRange(imgsToDelete);
			}

			// 依序更新
			if (imgSaveDto.ImgData != null && imgSaveDto.ImgData.Any())
			{
				foreach (var item in imgSaveDto.ImgData)
				{
					var img = await _context.ProductImages.FirstOrDefaultAsync(i => i.No == item.OrderId);
					if (img != null)
					{
						img.ImgOrder = item.OrderOd;
					}
				}
			}

			await _context.SaveChangesAsync();

			// 把更新後的圖片再抓出來顯示
			var firstId = imgSaveDto.ImgData.First().OrderId;
			var productNo = _context.ProductImages
				.Where(x => x.No == firstId)
				.Select(x => x.ProductNo)
				.FirstOrDefault();

			var vm = new ProductEditViewModel
			{
				Product = _context.Products.FirstOrDefault(p => p.No == productNo),
				ProImages = _context.ProductImages
					.Where(p => p.ProductNo == productNo)
					.OrderBy(p => p.ImgOrder)
					.ToList()
			};

			return PartialView("_PicturePartial", vm);
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
