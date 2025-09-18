using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using StarProject.Models;
using StarProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public class TicketStockController : Controller
    {
        private readonly StarProjectContext _context;

		public TicketStockController(StarProjectContext context)
        {
            _context = context;
        }


		// GET: TicketStock
		public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
			// 如果沒有傳入參數，就給預設值
			if (!startDate.HasValue)
				startDate = DateTime.Today;

			if (!endDate.HasValue)
				endDate = DateTime.Today.AddDays(6);

			// 1️⃣ 把每日庫存投影成同樣結構
			var dailyStocks = _context.TicketStocks
				.Select(s => new
				{
					TicketNo = s.TicketNo,
					TicketName = s.TicketNoNavigation.Name,
					TicketImage = s.TicketNoNavigation.Image,
					TicCategory = s.TicketNoNavigation.TicCategoryNoNavigation.Name,
					TicType = s.TicketNoNavigation.Type,
					Date = s.Date.Date,
					Stock = (int?)s.Stock,      // 用 int? 讓 null 可能存在
					TransQuantity = (int?)null  // 異動先為 null
				});

            // 2️⃣ 把異動表投影成同樣結構（Stock=null）
            var transStocks = _context.TicketTransStocks
                .Select(t => new
                {
                    TicketNo = t.TicketNo,
					TicketName = t.TicketNoNavigation.Name,
					TicketImage = t.TicketNoNavigation.Image,
					TicCategory = t.TicketNoNavigation.TicCategoryNoNavigation.Name,
					TicType = t.TicketNoNavigation.Type,
					Date = t.Date.Date,
                    Stock = (int?)null,
                    // 判斷加減數：如果 TransQuantity 自己已正負，就不用再判斷
                    TransQuantity = (int?)t.TransQuantity
                });

			// 3️⃣ 合併兩個查詢
			var combined = dailyStocks.Concat(transStocks);

			// 4️⃣ GroupBy 票券+日期，計算合計
			var query = from c in combined
						group c by new
						{
							c.TicketNo,
							c.TicketName,
                            c.TicketImage,
							c.TicCategory,
							c.TicType,
							c.Date
						} into g
						select new TicketStockSumViewModel
						{
							No = g.Key.TicketNo,
                            Name = g.Key.TicketName,
							TicCategory = g.Key.TicCategory,
							Type = g.Key.TicType,
							ReleaseDate = g.Key.Date,
							InitialStock = g.Sum(x => x.Stock ?? 0),
							TotalStock = g.Sum(x => x.Stock ?? 0)
									   + g.Sum(x => x.TransQuantity ?? 0)
						};

			// 篩選開始日期
			if (startDate.HasValue)
			{
				query = query.Where(x => x.ReleaseDate >= startDate.Value.Date);
			}

			// 篩選結束日期
			if (endDate.HasValue)
			{
				query = query.Where(x => x.ReleaseDate <= endDate.Value.Date);
			}

			var result = await query.ToListAsync();

			// 也把預設值傳到 View，方便表單顯示
			ViewData["startDate"] = startDate.Value.ToString("yyyy-MM-dd");
			ViewData["endDate"] = endDate.Value.ToString("yyyy-MM-dd");

			return View(result);
		}

		

		// GET: TicketStock/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketStock = await _context.TicketStocks
                .Include(t => t.TicketNoNavigation)
                .FirstOrDefaultAsync(m => m.No == id);
            if (ticketStock == null)
            {
                return NotFound();
            }

            return View(ticketStock);
        }

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		// 票券庫存建立-多筆(GET)
		// GET: TicketStock/DownloadTemplate
		[HttpGet]
		public IActionResult DownloadTemplate()
		{
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exceltemps", "TicketStockTemplate.xlsx");
			var fileBytes = System.IO.File.ReadAllBytes(filePath);
			return File(fileBytes,
						"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
						"TicketStockTemplate.xlsx");
		}

		// 票券庫存建立-多筆(POST)
		// POST: Product/CreateMultiple
		[HttpPost]
		public async Task<IActionResult> CreateMultiple(IFormFile excelFile)
		{
			if (excelFile == null || excelFile.Length == 0)
			{
				ModelState.AddModelError("", "請選擇檔案");
				return View();
			}

			var ticketstocks = new List<TicketStock>();

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


				for (int row = 2; row <= sheet.LastRowNum; row++) // 從第 3 列開始 (第1列標題、第2列範例)
				{
					var currentRow = sheet.GetRow(row);
					if (currentRow == null) continue;

					string? ticketNoStr = currentRow.GetCell(0)?.ToString();
					int.TryParse(ticketNoStr, out int ticketNo);

					string? dateStr = currentRow.GetCell(1)?.ToString();
					DateTime date;

					DateTime.TryParseExact(dateStr, "yyyyMMdd",
						CultureInfo.InvariantCulture,
						DateTimeStyles.None,
						out date);

					string? stockStr = currentRow.GetCell(2)?.ToString();
					int.TryParse(stockStr, out int stock);

					var ticketstock = new TicketStock
					{
						TicketNo = ticketNo,
						Date = date,
						Stock = stock,
					};
					ticketstocks.Add(ticketstock);
				}
				_context.TicketStocks.AddRange(ticketstocks);
				await _context.SaveChangesAsync();
			}
			TempData["Success"] = $"成功匯入 {ticketstocks.Count} 筆資料";
			return RedirectToAction("Index");
		}



		private bool TicketStockExists(int id)
        {
            return _context.TicketStocks.Any(e => e.No == id);
        }
    }
}
