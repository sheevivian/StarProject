using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StarProject.DTOs.EmpsDTOs;
using StarProject.Services;
using StarProject.Models;
using StarProject.ViewModels;
using StarProject.Attributes;
using StarProject.Helpers;
using StarProject.DTOs.RoleDTOs;
using System.IO;

namespace StarProject.Controllers
{
	public class EmpsController : Controller
	{
		private readonly StarProjectContext _context;
		private readonly IEmailService _emailService;
		private readonly IRazorViewEngine _razorViewEngine;
		private readonly ITempDataProvider _tempDataProvider;
		private readonly IServiceProvider _serviceProvider;

		public EmpsController(
			StarProjectContext context,
			IEmailService emailService,
			IRazorViewEngine razorViewEngine,
			ITempDataProvider tempDataProvider,
			IServiceProvider serviceProvider)
		{
			_context = context;
			_emailService = emailService;
			_razorViewEngine = razorViewEngine;
			_tempDataProvider = tempDataProvider;
			_serviceProvider = serviceProvider;
		}

		// 查看員工清單 - 需要員工管理或用戶管理權限
		[Permission("emp", "user")]
		public async Task<IActionResult> Index()
		{
			var emps = await _context.Emps
				.Include(e => e.DeptNoNavigation)
				.Include(e => e.RoleNoNavigation)
				.OrderBy(e => e.EmpCode)
				.Take(10) // 預設載入前 10 筆
				.ToListAsync();

			ViewBag.Total = await _context.Emps.CountAsync();
			ViewBag.Page = 1;
			ViewBag.PageSize = 10;
			ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.Total / ViewBag.PageSize);
			// 加入 RoleHelper 到 ViewBag 供 View 使用
			ViewBag.RoleHelper = typeof(RoleHelper);

			return View(emps);
		}

		// 搜尋和分頁功能
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SearchEmps([FromBody] SearchEmpRequest request)
		{
			var query = _context.Emps
				.Include(e => e.DeptNoNavigation)
				.Include(e => e.RoleNoNavigation)
				.AsQueryable();

			// 修改 SearchEmps 方法中的搜尋邏輯
			// 關鍵字搜尋 - 根據你的實際欄位名稱
			if (!string.IsNullOrWhiteSpace(request.Keyword))
			{
				var kw = request.Keyword.Trim();
				query = query.Where(e =>
					e.Name.Contains(kw) ||
					e.EmpCode.Contains(kw) ||
					e.DeptNoNavigation.DeptName.Contains(kw) ||
					e.DeptNoNavigation.DeptCode.Contains(kw) ||
					e.RoleNoNavigation.RoleName.Contains(kw));
			}

			// 部門篩選 - 使用 DeptCode 
			if (request.Departments != null && request.Departments.Any())
			{
				query = query.Where(e => request.Departments.Contains(e.DeptNoNavigation.DeptCode));
			}

			// 職位篩選 - 這裡比較複雜，因為你的資料庫沒有 RoleCode
			// 方案1: 如果前端傳來的是 RoleCode (RS, EX 等)
			if (request.Roles != null && request.Roles.Any())
			{
				// 將 RoleCode 轉換為 RoleName 來查詢
				var roleNames = request.Roles.Select(code => RoleHelper.GetRoleDisplayName(code)).ToList();
				query = query.Where(e => roleNames.Contains(e.RoleNoNavigation.RoleName));
			}


			// 狀態
			if (request.Statuses != null && request.Statuses.Any())
			{
				bool wantIn = request.Statuses.Contains("在職");
				bool wantOut = request.Statuses.Contains("離職");
				query = query.Where(e => (wantIn && e.Status) || (wantOut && !e.Status));
			}

			// 入職日期
			if (!string.IsNullOrWhiteSpace(request.DateFrom) && DateTime.TryParse(request.DateFrom, out DateTime from))
			{
				query = query.Where(e => e.HireDate >= from);
			}
			if (!string.IsNullOrWhiteSpace(request.DateTo) && DateTime.TryParse(request.DateTo, out DateTime to))
			{
				query = query.Where(e => e.HireDate <= to);
			}

			// 分頁
			var totalCount = await query.CountAsync();
			var page = Math.Max(request.Page, 1);
			var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

			var items = await query
				.OrderBy(e => e.EmpCode)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// 設定 ViewBag 資料
			ViewBag.Total = totalCount;
			ViewBag.PageSize = pageSize;
			ViewBag.Page = page;
			ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

			// Render 部分 View - 先渲染員工列表
			var empRowsHtml = await RenderPartialViewToStringAsync("_EmpRowsPartial", items);

			// 再渲染分頁 - 這裡需要傳遞一個空的 model，但 ViewBag 會保持
			var paginationHtml = await RenderPartialViewToStringAsync("_PaginationPartial", new object());

			return Json(new
			{
				empRows = empRowsHtml,
				pagination = paginationHtml
			});
		}

		// GET: Emps/Details/5
		public async Task<IActionResult> Details(string id)
		{
			if (id == null)
				return NotFound();

			var emp = await _context.Emps
				.Include(e => e.DeptNoNavigation)
				.Include(e => e.RoleNoNavigation)
				.FirstOrDefaultAsync(m => m.No == id);

			if (emp == null)
				return NotFound();

			return View(emp);
		}

		// 建立員工 - 只有員工管理權限可以
		[Permission("emp")]
		// GET: Emps/Create
		public IActionResult Create()
		{
			var viewModel = new CreateEmpViewModel
			{
				HireDate = DateTime.Today
			};

			LoadDropdowns(viewModel); // 載入下拉式選單
			return View(viewModel);
		}

		[Permission("emp")]
		// POST: Emps/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreateEmpViewModel viewModel)
		{
			// 臨時調試：檢查 EmailService 是否正確注入
			if (_emailService == null)
			{
				TempData["EmailError"] = "EmailService 未正確注入";
				return RedirectToAction(nameof(Index));
			}

			// 檢查ModelState
			if (!ModelState.IsValid)
			{
				LoadDropdowns(viewModel); // 驗證失敗時，重新載入下拉式選單
				return View(viewModel);
			}

			try
			{
				// 建立新的Emp物件
				var emp = new Emp
				{
					No = Guid.NewGuid().ToString(),
					Name = viewModel.Name,
					DeptNo = viewModel.DeptNo,
					RoleNo = viewModel.RoleNo,
					HireDate = viewModel.HireDate,
					Email = viewModel.Email,
					Phone = viewModel.Phone,
					IdNumber = viewModel.IdNumber,
					BirthDate = viewModel.BirthDate,
					Status = true,
					ForceChangePassword = true
				};

				// 生成員工編號
				emp.EmpCode = await GenerateEmpCodeAsync(emp.DeptNo);

				// 生成預設密碼及雜湊
				string defaultPassword = "Abc12345";
				(emp.PasswordHash, emp.PasswordSalt) = PasswordHelper.HashPassword(defaultPassword);

				// 存入資料庫
				_context.Emps.Add(emp);
				await _context.SaveChangesAsync();

				// 發送歡迎郵件
				if (!string.IsNullOrEmpty(emp.Email))
				{
					try
					{
						await _emailService.SendWelcomeEmailAsync(emp.Email, emp.Name, emp.EmpCode, defaultPassword, emp.HireDate);
						TempData["EmailSent"] = true;
					}
					catch (Exception ex)
					{
						TempData["EmailError"] = $"Email發送失敗：{ex.Message}";
					}
				}
				else
				{
					TempData["EmailNotSent"] = "未填寫Email，跳過發送通知";
				}

				// HR 可以看到剛生成的員工編號
				TempData["NewEmpCode"] = emp.EmpCode;
				TempData["TempPassword"] = defaultPassword;

				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"建立員工時發生錯誤：{ex.Message}");
			}

			LoadDropdowns(viewModel);
			return View(viewModel);
		}

		// 生成員工編號
		private async Task<string> GenerateEmpCodeAsync(int deptNo)
		{
			var dept = await _context.Depts.FindAsync(deptNo);
			if (dept == null)
				throw new Exception("部門不存在");

			string deptCode = dept.DeptCode;

			int count = await _context.Emps.CountAsync(e => e.DeptNo == deptNo);

			string empCode = $"{deptCode}{(count + 1):D3}";
			return empCode;
		}

		[Permission("emp")]
		public async Task<IActionResult> Edit(string id)
		{
			if (id == null)
				return NotFound();

			var emp = await _context.Emps.FindAsync(id);
			if (emp == null)
				return NotFound();

			// 建立 ViewModel 並映射資料
			var viewModel = new EditEmpViewModel
			{
				No = emp.No,
				Name = emp.Name,
				RoleNo = emp.RoleNo,
				DeptNo = emp.DeptNo,
				HireDate = emp.HireDate,
				Status = emp.Status,
				Email = emp.Email,
				Phone = emp.Phone,
				IdNumber = emp.IdNumber,
				BirthDate = emp.BirthDate
			};

			LoadDropdowns(viewModel); // 載入下拉式選單
			return View(viewModel);
		}

		// POST: Emps/Edit/5
		[HttpPost]
		[Permission("emp")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, EditEmpViewModel viewModel)
		{
			if (id != viewModel.No)
				return NotFound();

			System.Diagnostics.Debug.WriteLine($"收到編輯資料: Name={viewModel.Name}, DeptNo={viewModel.DeptNo}, RoleNo={viewModel.RoleNo}");
			System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

			if (!ModelState.IsValid)
			{
				System.Diagnostics.Debug.WriteLine("ModelState驗證失敗:");
				foreach (var error in ModelState)
				{
					System.Diagnostics.Debug.WriteLine($"欄位: {error.Key}, 錯誤: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
				}

				LoadDropdowns(viewModel); // 驗證失敗時，重新載入下拉式選單
				return View(viewModel);
			}

			try
			{
				// 從資料庫取得原始員工資料
				var existingEmp = await _context.Emps.FindAsync(id);
				if (existingEmp == null)
					return NotFound();

				// 只更新允許修改的欄位
				existingEmp.Name = viewModel.Name;
				existingEmp.RoleNo = viewModel.RoleNo;
				existingEmp.DeptNo = viewModel.DeptNo;
				existingEmp.HireDate = viewModel.HireDate;
				existingEmp.Status = viewModel.Status;
				existingEmp.Email = viewModel.Email;
				existingEmp.Phone = viewModel.Phone;
				existingEmp.IdNumber = viewModel.IdNumber;
				existingEmp.BirthDate = viewModel.BirthDate;

				// 保存變更
				await _context.SaveChangesAsync();

				System.Diagnostics.Debug.WriteLine("員工資料更新成功，準備跳轉到 Index");
				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!EmpExists(viewModel.No))
					return NotFound();
				else
					throw;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"更新員工錯誤: {ex.Message}");
				ModelState.AddModelError("", $"更新員工時發生錯誤：{ex.Message}");
			}

			// 如果到這裡，表示有錯誤，重新顯示表單
			LoadDropdowns(viewModel); // 驗證失敗時，重新載入下拉式選單
			return View(viewModel);
		}

		[Permission("emp")]
		// GET: Emps/Delete/5
		public async Task<IActionResult> Delete(string id)
		{
			if (id == null)
				return NotFound();

			var emp = await _context.Emps
				.Include(e => e.DeptNoNavigation)
				.Include(e => e.RoleNoNavigation)
				.FirstOrDefaultAsync(m => m.No == id);

			if (emp == null)
				return NotFound();

			return View(emp);
		}

		[Permission("emp")]
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(string id)
		{
			var emp = await _context.Emps.FindAsync(id);
			if (emp != null)
			{
				// 軟刪除：只改變狀態，不實際刪除記錄
				emp.Status = false; // 設為離職狀態

				await _context.SaveChangesAsync();
				TempData["SuccessMessage"] = "員工已設為離職狀態";
			}
			return RedirectToAction(nameof(Index));
		}

		// 批量刪除
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteMultiple([FromBody] List<string> empIds)
		{
			if (empIds == null || empIds.Count == 0)
				return Json(new { success = false, message = "沒有選取任何員工" });

			var emps = await _context.Emps.Where(e => empIds.Contains(e.No)).ToListAsync();
			if (emps.Count == 0)
				return Json(new { success = false, message = "找不到選取的員工" });

			_context.Emps.RemoveRange(emps);
			await _context.SaveChangesAsync();

			return Json(new { success = true });
		}

		// Helper: 把 PartialView Render 成字串
		private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
		{
			var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor);

			using (var sw = new StringWriter())
			{
				var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);
				if (viewResult.View == null)
				{
					viewResult = _razorViewEngine.GetView(null, viewName, false);
					if (viewResult.View == null)
					{
						throw new InvalidOperationException($"找不到 View {viewName}");
					}
				}

				var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), ModelState)
				{
					Model = model
				};

				// 複製當前的 ViewBag 資料到新的 ViewDataDictionary
				foreach (var item in ViewData)
				{
					viewDictionary[item.Key] = item.Value;
				}

				var tempData = new TempDataDictionary(HttpContext, _tempDataProvider);

				var viewContext = new ViewContext(
					actionContext,
					viewResult.View,
					viewDictionary,
					tempData,
					sw,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync(viewContext);
				return sw.ToString();
			}
		}

		// 建立一個私有方法來載入下拉式選單資料
		private void LoadDropdowns(CreateEmpViewModel viewModel)
		{
			viewModel.Depts = new SelectList(_context.Depts, "No", "DeptDescription", viewModel.DeptNo);
			viewModel.Roles = new SelectList(_context.Roles, "No", "RoleName", viewModel.RoleNo);
		}

		// 建立一個私有方法來載入編輯頁面的下拉式選單資料
		private void LoadDropdowns(EditEmpViewModel viewModel)
		{
			viewModel.Depts = new SelectList(_context.Depts, "No", "DeptDescription", viewModel.DeptNo);
			viewModel.Roles = new SelectList(_context.Roles, "No", "RoleName", viewModel.RoleNo);
		}
		private bool EmpExists(string id)
		{
			return _context.Emps.Any(e => e.No == id);
		}
	}
}