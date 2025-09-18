using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Build.Graph;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using StarProject.Helpers;
using StarProject.Models;
using StarProject.ViewModel;
using StarProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public partial class ProductController : Controller
    {
		private const int pageNumber = 20;

		private readonly StarProjectContext _context;

        public ProductController(StarProjectContext context)
        {
            _context = context;
        }

		// 商品列表+頁數(GET)
        // GET: Product
        [HttpGet]
		public async Task<IActionResult> Index(int page = 1, int pageSize = pageNumber)
        {
			// 先組 IQueryable (全部資料，還沒篩選)
			var query = _context.Products
						.Include(p => p.ProCategoryNoNavigation)
						.Include(p => p.ProductImages)
						.OrderByDescending(x => x.No);
			// 呼叫分頁工具
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			return View(items);
		}

		// 商品列表-搜尋+進階篩選功能
		// POST: Product/SearchSelect
		[HttpPost]
		public async Task<IActionResult> SearchSelect([FromBody] SearchFilterVM filters)
		{
			// 從 filters 取得 pageSize，如果沒有就給預設值
			int page = filters.Page > 0 ? filters.Page : 1;
			int pageSize = filters.PageSize >= 0 ? filters.PageSize : 10;

			var query = _context.Products
						.Include(p => p.ProCategoryNoNavigation)
						.Include(p => p.ProductImages).AsQueryable();
			query = query.OrderByDescending(x => x.No);

			// keyword
			if (!string.IsNullOrEmpty(filters.keyword))
			{
				query = query.Where(x => x.Name.Contains(filters.keyword)
									 || x.ProCategoryNoNavigation.Name.Contains(filters.keyword)
									 || x.Status.Contains(filters.keyword));
			}

			// 進階篩選>分類
			if (filters.Categories != null && filters.Categories.Any())
				query = query.Where(x => filters.Categories.Contains(x.ProCategoryNo));

			// 進階篩選>狀態
			if (filters.Statuses != null && filters.Statuses.Any())
				query = query.Where(x => filters.Statuses.Contains(x.Status));

			// 日期區間
			if (!string.IsNullOrEmpty(filters.DateFrom))
				query = query.Where(x => x.ReleaseDate >= DateTime.Parse(filters.DateFrom));

			if (!string.IsNullOrEmpty(filters.DateTo))
				query = query.Where(x => x.ReleaseDate <= DateTime.Parse(filters.DateTo));


			// 呼叫分頁工具
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			var tableHtml = await RenderPartialViewToString("_ProductRows", items);
			var paginationHtml = await RenderPartialViewToString("_ProductPagination", null);

			return Json(new { tableHtml, paginationHtml });
		}

		// 商品列表-更新頁碼+搜尋
		// GET: Product/RenderPartialViewToString
		[HttpGet]
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
			
			// 移除 ModelState 中無法綁定的欄位
			ModelState.Remove("ProCategoryNoNavigation");

			if (ModelState.IsValid)
			{
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
			}
			// 如果失敗就回到 Create 畫面，顯示錯誤
			ViewData["ProCategoryName"] = new SelectList(_context.ProCategories, "No", "Name");
			ViewBag.StatusList = new List<SelectListItem>
			{
				new SelectListItem { Value = "上架", Text = "上架" },
				new SelectListItem { Value = "預購", Text = "預購" },
			};
			return View();
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

			// 產生Excel並暫存於 wwwroot/exceltemps
			var fileName = $"ImportResult_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
			var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exceltemps", fileName);

			var downloadWorkbook = new XSSFWorkbook();
			var downloadSheet = downloadWorkbook.CreateSheet("匯入商品圖片");
			var headerRow = downloadSheet.CreateRow(0);
			headerRow.CreateCell(0).SetCellValue("商品編號");
			headerRow.CreateCell(1).SetCellValue("圖片路徑");
			headerRow.CreateCell(2).SetCellValue("圖片順序");

			for (int i = 0; i < products.Count; i++)
			{
				var row = downloadSheet.CreateRow(i + 1);
				row.CreateCell(0).SetCellValue(products[i].No);
			}

			using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
			{
				downloadWorkbook.Write(fs, true);
			}

			// 回傳下載連結
			var downloadUrl = Url.Content("~/exceltemps/" + fileName);
			return Json(new { success = true, message = $"已成功建立{products.Count}筆商品檔案！", downloadUrl });
		}

		// 新品上架-多筆上傳圖片(POST)
		// POST: Product/CreateImageMultiple
		[HttpPost]
		public async Task<IActionResult> CreateImageMultiple(IFormFile excelFile)
		{
			if (excelFile == null || excelFile.Length == 0)
			{
				ModelState.AddModelError("", "請選擇檔案");
				return View("Error");
			}

			var productImages = new List<ProductImage>();

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

				string imgPath;

				for (int row = 2; row <= sheet.LastRowNum; row++) // 從第 3 列開始 (第1列標題、第2列範例)
				{
					var currentRow = sheet.GetRow(row);
					if (currentRow == null) continue;

					imgPath = currentRow.GetCell(1)?.ToString();

					var productImage = new ProductImage
					{
						ProductNo = int.TryParse(currentRow.GetCell(0)?.ToString(), out var productNo) ? productNo : 0,
						Image = await ImgPathHelper.UploadToImgBB(imgPath),
						ImgOrder = int.TryParse(currentRow.GetCell(2)?.ToString(), out var imgOd) ? imgOd : 0
					};
					productImages.Add(productImage);

					var product = await _context.Products.FindAsync(productNo);
					product.UpdateDate = DateTime.Now;
				}
				_context.ProductImages.AddRange(productImages);

				await _context.SaveChangesAsync();
			}
			TempData["Success"] = $"成功匯入 {productImages.Count} 筆資料！";
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
					new SelectListItem { Value = "絕版", Text = "絕版" },
				},
				"Value", "Text", product.Status);
			return View(vm); // 同一個 View
		}

		// 編輯商品(POST)
		// POST: Tickets/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, ProductEditViewModel proEditVM)
		{
			if (id != proEditVM.Product.No)
			{
				return NotFound();
			}

			// 1. 先抓出資料庫裡的 Product
			var product = await _context.Products.FirstOrDefaultAsync(p => p.No == id);
			if (product == null)
				return NotFound();

			// 2. 更新有變更的欄位
			product.Name = proEditVM.Product.Name;
			product.ProCategoryNo = proEditVM.Product.ProCategoryNo;
			product.Price = proEditVM.Product.Price;
			product.Status = proEditVM.Product.Status;
			product.ReleaseDate = proEditVM.Product.ReleaseDate;
			product.UpdateDate = DateTime.Now; // 更新日期

		
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
			return RedirectToAction("Edit", new { id = proEditVM.Product.No });
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

			// 只更新 UpdateDate
			var product = await _context.Products.FirstOrDefaultAsync(p => p.No == proEditVM.Product.No);
			if (product != null)
				product.UpdateDate = DateTime.Now;

			await _context.SaveChangesAsync();


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

		// 更新照片區(POST)
		// POST: Product/ImgSave
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

		// 新增商品介紹(GET)
		// GET: Product/Article/5
		[HttpGet]
		public async Task<IActionResult> Article(int id)
		{
			var proIntro = _context.ProductIntroduces
								   .Include(pi => pi.ProductNoNavigation)
								   .FirstOrDefault(p => p.ProductNo == id);
			return View(proIntro);
		}


		private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.No == id);
        }
    }
}
