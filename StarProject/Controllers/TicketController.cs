using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SixLabors.ImageSharp;
using StarProject.Helpers;
using StarProject.Models;
using StarProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
    public class TicketController : Controller
    {
		private const int pageNumber = 10;
		private readonly StarProjectContext _context;

        public TicketController(StarProjectContext context)
        {
            _context = context;
        }

		// 票券列表+頁數(GET)
		// GET: Ticket
		public async Task<IActionResult> Index(int page = 1, int pageSize = pageNumber)
        {
            var query = _context.Tickets
					.Include(t => t.TicCategoryNoNavigation)
					.GroupBy(t => t.Name)
				.Select(g => new TicketNameViewModel
				{
					TicName = g.Key,
					TicImage = g.Max(x => x.Image),
					TicCategory = g.Max(x => x.TicCategoryNoNavigation.Name),
					TicStatus = g.Max(x => x.Status),
				});

			// Step 3: 分頁 (這時 query 仍是 IQueryable<ProductStockSumViewModel>)
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// Step 4: ViewBag
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			return View(items);
		}

		// 票券查詢
		// POST: Ticket/SearchSelect 
		[HttpPost]
		public async Task<IActionResult> SearchSelect([FromBody] SearchFilterVM filters)
		{
			// 從 filters 取得 pageSize，如果沒有就給預設值
			int page = filters.Page > 0 ? filters.Page : 1;
			int pageSize = filters.PageSize >= 0 ? filters.PageSize : 10;

			var query = _context.Tickets
						.Include(t => t.TicCategoryNoNavigation)
						.GroupBy(t => t.Name)
						.Select(g => new TicketNameViewModel
							{
								TicName = g.Key,
								TicImage = g.Max(x => x.Image),
								TicCategory = g.Max(x => x.TicCategoryNoNavigation.Name),
								TicStatus = g.Max(x => x.Status),
							});

			// keyword
			if (!string.IsNullOrEmpty(filters.keyword))
			{
				query = query.Where(x => x.TicName.Contains(filters.keyword)
									|| x.TicCategory.Contains(filters.keyword));
			}

			// 分類
			if (filters.Categories != null && filters.Categories.Any())
				query = query.Where(x => filters.Categories.Contains(x.TicCategory));

			// 狀態
			if (filters.Statuses != null && filters.Statuses.Any())
				query = query.Where(x => filters.Statuses.Contains(x.TicStatus));

			// 呼叫分頁工具
			var (items, total, totalPages) = await PaginationHelper.PaginateAsync(query, page, pageSize);

			// 把分頁資訊丟給 View
			ViewBag.Total = total;
			ViewBag.TotalPages = totalPages;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;

			var tableHtml = await RenderPartialViewToString("_TicketRows", items);
			var paginationHtml = await RenderPartialViewToString("_TicketPagination", items);

			return Json(new { tableHtml, paginationHtml });
		}



		// 票券列表-更新頁碼+搜尋
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





		// GET: Ticket/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.TicCategoryNoNavigation)
                .FirstOrDefaultAsync(m => m.No == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

		// 票券建立-單筆(GET)
		// GET: Ticket/Create
		[HttpGet]
        public IActionResult Create()
        {
            ViewData["TicCategoryNo"] = new SelectList(_context.TicCategories, "No", "Name");

			ViewBag.TypeList = new List<SelectListItem>
				{
					new SelectListItem { Value = "普通票", Text = "普通票" },
					new SelectListItem { Value = "優待票", Text = "優待票" },
					new SelectListItem { Value = "團體票", Text = "團體票" },
					new SelectListItem { Value = "免費入場", Text = "免費入場" },
				};

			return View();
		}

		// 票券建立-單筆(POST)
		// POST: Ticket/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateViewModel ticketVM)
        {
			if (ModelState.IsValid)
			{
                try{ 
                    int lastNo = _context.Tickets.Max(t => (int?)t.No) ?? 29999;
                    
                    string imgUrl = await ImgUploadHelper.UploadToImgBB(ticketVM.ImageFile);

					var tickets = new Ticket
                    {
						No = lastNo + 1,
						Name = ticketVM.Name,
                        Image = imgUrl,
                        TicCategoryNo = ticketVM.TicCategoryNo,
                        Type = ticketVM.Type,
                        Price = ticketVM.Price,
                        Status = "使用",
				        ReleaseDate = ticketVM.ReleaseDate,
                        UpdateDate = DateTime.Now,
				        Desc = ticketVM.Desc,
			        };

                    _context.Tickets.Add(tickets);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
				}
                catch (Exception ex) {
					ModelState.AddModelError("", "上傳圖片或儲存失敗：" + ex.Message);
				}

			ViewData["TicCategoryNo"] = new SelectList(_context.TicCategories, "No", "Name");

			ViewBag.TypeList = new List<SelectListItem>
				{
					new SelectListItem { Value = "普通票", Text = "普通票" },
					new SelectListItem { Value = "優待票", Text = "優待票" },
					new SelectListItem { Value = "團體票", Text = "團體票" },
					new SelectListItem { Value = "免費入場", Text = "免費入場" },
				};

			}
			return View(ticketVM);
		}

		// 票券建立-多筆(GET)
		// GET: Ticket/DownloadTemplate
		[HttpGet]
		public IActionResult DownloadTemplate()
		{
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exceltemps", "TicketTemplate.xlsx");
			var fileBytes = System.IO.File.ReadAllBytes(filePath);
			return File(fileBytes,
						"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
						"TicketTemplate.xlsx");
		}

		// 票券建立-多筆(POST)
		// POST: Ticket/CreateMultiple
		[HttpPost]
		public async Task<IActionResult> CreateMultiple(IFormFile excelFile)
		{
			if (excelFile == null || excelFile.Length == 0)
			{
				ModelState.AddModelError("", "請選擇檔案");
				return View();
			}

			var tickets = new List<Ticket>();

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

				int lastNo = _context.Tickets.Max(p => (int?)p.No) ?? 99999;
                string imgPath;


				for (int row = 2; row <= sheet.LastRowNum; row++) // 從第 3 列開始 (第1列標題、第2列範例)
				{
					var currentRow = sheet.GetRow(row);
					if (currentRow == null) continue;

					lastNo++; // 每次加 1，避免重複
                    imgPath = currentRow.GetCell(1)?.ToString();

					var ticket = new Ticket
					{
						No = lastNo,
						Name = currentRow.GetCell(0)?.ToString(),
						Image = await ImgPathHelper.UploadToImgBB(imgPath),
						TicCategoryNo = currentRow.GetCell(2)?.ToString(),
                        Type = currentRow.GetCell(3)?.ToString(),
						Price = decimal.TryParse(currentRow.GetCell(4)?.ToString(), out var price) ? price : 0,
						Status = "使用",
						ReleaseDate = DateTime.TryParse(currentRow.GetCell(5)?.ToString(), out var dt) ? dt : DateTime.Now,
						UpdateDate = DateTime.Now,
						Desc = currentRow.GetCell(6)?.ToString(),
					};
					tickets.Add(ticket);
				}
				_context.Tickets.AddRange(tickets);
				await _context.SaveChangesAsync();
			}
			TempData["Success"] = $"成功匯入 {tickets.Count} 筆資料";
			return RedirectToAction("Index");
		}

        // 票券編輯(GET)
		// GET: Ticket/Edit/5
		public async Task<IActionResult> Edit(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var ticketEditVm = new TicketEditViewModel
            {
				No = id,
                Name = ticket.Name,
                Image = ticket.Image,
                TicCategoryNo = ticket.TicCategoryNo,
				Type = ticket.Type,
                Price = ticket.Price,
				Status = ticket.Status,
                ReleaseDate = ticket.ReleaseDate,
                Desc = ticket.Desc
			};

			ViewData["TicCategoryNo"] = new SelectList(_context.TicCategories, "No", "Name", ticketEditVm.TicCategoryNo);

			ViewBag.TypeList = new SelectList (new List<SelectListItem>
				{
					new SelectListItem { Value = "普通票", Text = "普通票" },
					new SelectListItem { Value = "優待票", Text = "優待票" },
					new SelectListItem { Value = "團體票", Text = "團體票" },
					new SelectListItem { Value = "免費入場", Text = "免費入場" }
				}, "Value", "Text", ticketEditVm.Type);

			ViewBag.StatusList = new SelectList(new List<SelectListItem>
				{
					new SelectListItem { Value = "使用", Text = "使用" },
					new SelectListItem { Value = "停用", Text = "停用" },
				}, "Value", "Text", ticketEditVm.Status);

			return View(ticketEditVm);
        }

		// 票券編輯(POST)
        // POST: Ticket/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TicketEditViewModel ticketEditVM)
        {
            if (id != ticketEditVM.No)
            {
                return NotFound();
            }

			string imgUrl = await ImgUploadHelper.UploadToImgBB(ticketEditVM.ImageFile);

			ModelState.Remove("Image");

			if (ModelState.IsValid)
            {
                try
                {
					var ticket = await _context.Tickets.FindAsync(id);
					
					ticket.Name = ticketEditVM.Name;
					ticket.Image = imgUrl;
					ticket.TicCategoryNo = ticketEditVM.TicCategoryNo;
					ticket.Type = ticketEditVM.Type;
					ticket.Price = ticketEditVM.Price;
					ticket.Status = ticketEditVM.Status;
					ticket.ReleaseDate = ticketEditVM.ReleaseDate;
					ticket.UpdateDate = DateTime.Now;
					ticket.Desc = ticketEditVM.Desc;


					_context.Tickets.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticketEditVM.No))
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

			ViewData["TicCategoryNo"] = new SelectList(_context.TicCategories, "No", "Name", ticketEditVM.TicCategoryNo);

			ViewBag.TypeList = new SelectList(new List<SelectListItem>
				{
					new SelectListItem { Value = "普通票", Text = "普通票" },
					new SelectListItem { Value = "優待票", Text = "優待票" },
					new SelectListItem { Value = "團體票", Text = "團體票" },
					new SelectListItem { Value = "免費入場", Text = "免費入場" }
				}, "Value", "Text", ticketEditVM.Type);

			ViewBag.StatusList = new SelectList(new List<SelectListItem>
				{
					new SelectListItem { Value = "使用", Text = "使用" },
					new SelectListItem { Value = "停用", Text = "停用" },
				}, "Value", "Text", ticketEditVM.Status);
			return View(ticketEditVM);
        }

        // GET: Ticket/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.TicCategoryNoNavigation)
                .FirstOrDefaultAsync(m => m.No == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Ticket/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.No == id);
        }
    }
}
