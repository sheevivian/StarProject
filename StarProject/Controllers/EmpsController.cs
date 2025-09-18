using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StarProject.DTOs.EmpsDTOs;
using StarProject.Services;
using StarProject.Models;
using StarProject.ViewModels;
using StarProject.Attributes;
using StarProject.Helpers;

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

			if (!string.IsNullOrWhiteSpace(request.Keyword))
			{
				var kw = request.Keyword.Trim();

				var matchingRoleCodes = RoleHelper.RoleDisplayMap
										.Where(pair => pair.Value.Contains(kw))
										.Select(pair => pair.Key)
										.ToList();

				// 先在記憶體中找到所有匹配中文名稱的部門代碼
				var matchingDeptCodes = RoleHelper.DepartmentDisplayMap
												.Where(pair => pair.Value.Contains(kw))
												.Select(pair => pair.Key)
												.ToList();

				query = query.Where(e =>
					e.Name.Contains(kw) ||
					e.EmpCode.Contains(kw) ||
					(e.DeptNoNavigation != null && e.DeptNoNavigation.DeptName.Contains(kw)) ||
					(e.DeptNoNavigation != null && e.DeptNoNavigation.DeptCode.Contains(kw)) ||
					(e.RoleNoNavigation != null && e.RoleNoNavigation.RoleName.Contains(kw)) ||
					(e.RoleNoNavigation != null && matchingRoleCodes.Contains(e.RoleNoNavigation.RoleName)) ||
					(e.DeptNoNavigation != null && matchingDeptCodes.Contains(e.DeptNoNavigation.DeptCode))
				);
			}


			// 部門篩選 - 使用 DeptCode 
			if (request.Departments != null && request.Departments.Any())
			{
				query = query.Where(e => e.DeptNoNavigation != null && request.Departments.Contains(e.DeptNoNavigation.DeptCode));
			}

			// 職位篩選 - 修復：直接使用職位代碼查詢
			if (request.Roles != null && request.Roles.Any())
			{
				// Debug: 輸出接收到的職位篩選條件
				Console.WriteLine($"接收到的職位篩選: {string.Join(", ", request.Roles)}");

				// 因為資料庫的 RoleName 欄位存的就是職位代碼 (RS, EX, MK 等)
				// 所以直接用前端傳來的代碼查詢即可
				query = query.Where(e => e.RoleNoNavigation != null && request.Roles.Contains(e.RoleNoNavigation.RoleName));

				Console.WriteLine($"使用職位代碼篩選: {string.Join(", ", request.Roles)}");
			}

			// 狀態篩選
			if (request.Statuses != null && request.Statuses.Any())
			{
				bool wantActive = request.Statuses.Contains("在職");
				bool wantInactive = request.Statuses.Contains("離職");

				if (wantActive && !wantInactive)
				{
					query = query.Where(e => e.Status == true);
				}
				else if (!wantActive && wantInactive)
				{
					query = query.Where(e => e.Status == false);
				}
				// 如果兩個都選或都沒選，就不篩選
			}

			// 入職日期篩選
			if (!string.IsNullOrWhiteSpace(request.DateFrom))
			{
				if (DateTime.TryParse(request.DateFrom, out DateTime fromDate))
				{
					query = query.Where(e => e.HireDate >= fromDate);
				}
			}

			if (!string.IsNullOrWhiteSpace(request.DateTo))
			{
				if (DateTime.TryParse(request.DateTo, out DateTime toDate))
				{
					// 包含整天，所以加到隔天的 00:00:00
					toDate = toDate.AddDays(1);
					query = query.Where(e => e.HireDate < toDate);
				}
			}

			// 計算總數
			var totalCount = await query.CountAsync();

			// 分頁處理
			var page = Math.Max(request.Page, 1);
			var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 100); // 限制最大 100 筆
			var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

			// 如果當前頁數大於總頁數，調整到最後一頁
			if (page > totalPages && totalPages > 0)
			{
				page = totalPages;
			}

			// 取得資料
			var items = await query
				.OrderBy(e => e.EmpCode)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// 設定 ViewBag 資料供 Partial View 使用
			ViewBag.Total = totalCount;
			ViewBag.PageSize = pageSize;
			ViewBag.Page = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.RoleHelper = typeof(RoleHelper);

			try
			{
				// Render 員工列表
				var empRowsHtml = await RenderPartialViewToStringAsync("_EmpRowsPartial", items);

				// Render 分頁 
				var paginationHtml = await RenderPartialViewToStringAsync("_PaginationPartial", new object());

				return Json(new
				{
					success = true,
					empRows = empRowsHtml,
					pagination = paginationHtml,
					totalCount = totalCount,
					currentPage = page,
					totalPages = totalPages
				});
			}
			catch (Exception ex)
			{
				// 記錄錯誤
				Console.WriteLine($"SearchEmps 錯誤: {ex.Message}");

				return Json(new
				{
					success = false,
					message = "資料載入失敗，請稍後再試",
					error = ex.Message
				});
			}
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
			// 加入調試輸出

			System.Diagnostics.Debug.WriteLine($"=== Create Action 開始 ===");
			System.Diagnostics.Debug.WriteLine($"Name: '{viewModel.Name}'");
			System.Diagnostics.Debug.WriteLine($"DeptNo: {viewModel.DeptNo}");
			System.Diagnostics.Debug.WriteLine($"RoleNo: {viewModel.RoleNo}");
			System.Diagnostics.Debug.WriteLine($"Email: '{viewModel.Email}'");
			System.Diagnostics.Debug.WriteLine($"HireDate: {viewModel.HireDate}");

			// 驗證EmailService注入
			if (_emailService == null)
			{
				ModelState.AddModelError("", "系統配置錯誤，請聯繫管理員");
				LoadDropdowns(viewModel);
				return View(viewModel);
			}

			// 自定義驗證：檢查Email唯一性
			if (!string.IsNullOrEmpty(viewModel.Email))
			{
				var existingEmailEmp = await _context.Emps
					.FirstOrDefaultAsync(e => e.Email == viewModel.Email && e.Status == true);
				if (existingEmailEmp != null)
				{
					ModelState.AddModelError("Email", "此Email已被使用");
				}
			}

			// 自定義驗證：檢查身分證字號唯一性
			if (!string.IsNullOrEmpty(viewModel.IdNumber))
			{
				var existingIdEmp = await _context.Emps
					.FirstOrDefaultAsync(e => e.IdNumber == viewModel.IdNumber && e.Status == true);
				if (existingIdEmp != null)
				{
					ModelState.AddModelError("IdNumber", "此身分證字號已被使用");
				}
			}

			// 驗證到職日期不可早於今天
			if (viewModel.HireDate < DateTime.Today)
			{
				ModelState.AddModelError("HireDate", "到職日期不可早於今天");
			}

			// 驗證生日（如果有填寫）
			if (viewModel.BirthDate.HasValue)
			{
				if (viewModel.BirthDate.Value > DateTime.Today)
				{
					ModelState.AddModelError("BirthDate", "生日不可晚於今天");
				}
				else if (viewModel.BirthDate.Value < DateTime.Today.AddYears(-100))
				{
					ModelState.AddModelError("BirthDate", "請輸入合理的生日日期");
				}
			}

			// 檢查ModelState
			if (!ModelState.IsValid)
			{
				LoadDropdowns(viewModel); // 驗證失敗時，重新載入下拉式選單
				return View(viewModel);
			}

			// 使用資料庫交易確保資料一致性
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 生成員工編號（在交易內執行以避免併發問題）
				var empCode = await GenerateEmpCodeAsync(viewModel.DeptNo);

				// 檢查生成的員工編號是否已存在（雙重確認）
				var existingEmp = await _context.Emps
					.FirstOrDefaultAsync(e => e.EmpCode == empCode);
				if (existingEmp != null)
				{
					// 如果存在，重新生成
					empCode = await GenerateEmpCodeAsync(viewModel.DeptNo, true);
				}

				// 建立新的Emp物件
				var emp = new Emp
				{
					No = Guid.NewGuid().ToString(),
					EmpCode = empCode,
					Name = viewModel.Name?.Trim(),
					DeptNo = viewModel.DeptNo,
					RoleNo = viewModel.RoleNo,
					HireDate = viewModel.HireDate,
					Email = viewModel.Email?.Trim(),
					Phone = viewModel.Phone?.Trim(),
					IdNumber = viewModel.IdNumber?.Trim().ToUpper(),
					BirthDate = viewModel.BirthDate,
					Status = true,
					ForceChangePassword = true
				};

				// 生成預設密碼及雜湊
				string defaultPassword = GenerateDefaultPassword();
				(emp.PasswordHash, emp.PasswordSalt) = PasswordHelper.HashPassword(defaultPassword);

				// 存入資料庫
				_context.Emps.Add(emp);
				await _context.SaveChangesAsync();

				// 提交交易
				await transaction.CommitAsync();

				// 非同步發送歡迎郵件（不影響主流程）
				_ = Task.Run(async () =>
				{
					if (!string.IsNullOrEmpty(emp.Email))
					{
						try
						{
							await _emailService.SendWelcomeEmailAsync(
								emp.Email,
								emp.Name,
								emp.EmpCode,
								defaultPassword,
								emp.HireDate);

							// 記錄郵件發送成功（可選）
							TempData["EmailSent"] = true;
						}
						catch (Exception emailEx)
						{
							// 記錄郵件發送失敗但不中斷主流程
							TempData["EmailError"] = $"員工創建成功，但Email發送失敗：{emailEx.Message}";
							// 這裡可以加入日誌記錄
						}
					}
					else
					{
						TempData["EmailNotSent"] = "未填寫Email，跳過發送通知";
					}
				});

				// 設置成功訊息
				TempData["NewEmpCode"] = emp.EmpCode;
				TempData["TempPassword"] = defaultPassword;
				TempData["SuccessMessage"] = "員工創建成功！";

				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				ModelState.AddModelError("", $"建立員工時發生錯誤：{ex.Message}");

				// 記錄詳細錯誤（用於調試）
				// _logger?.LogError(ex, "創建員工失敗: {ViewModelData}", JsonSerializer.Serialize(viewModel));
			}

			LoadDropdowns(viewModel);
			return View(viewModel);
		}

		// 生成預設密碼的方法
		private string GenerateDefaultPassword()
		{
			// 可以改為更安全的隨機密碼生成
			var random = new Random();
			var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var password = "Abc" + new string(Enumerable.Repeat(chars, 5)
				.Select(s => s[random.Next(s.Length)]).ToArray());

			return password;
		}

		// 改進的生成員工編號方法
		private async Task<string> GenerateEmpCodeAsync(int deptNo, bool forceRegenerate = false)
		{
			var dept = await _context.Depts.FindAsync(deptNo);
			if (dept == null)
				throw new Exception("部門不存在");

			string deptCode = dept.DeptCode;

			// 使用更安全的計數方式
			int count = await _context.Emps
				.Where(e => e.DeptNo == deptNo)
				.CountAsync();

			if (forceRegenerate)
			{
				// 如果需要強制重新生成，找到最大的編號
				var maxCode = await _context.Emps
					.Where(e => e.EmpCode.StartsWith(deptCode))
					.Select(e => e.EmpCode)
					.ToListAsync();

				if (maxCode.Any())
				{
					var maxNumber = maxCode
						.Select(code =>
						{
							if (int.TryParse(code.Substring(deptCode.Length), out int num))
								return num;
							return 0;
						})
						.DefaultIfEmpty(0)
						.Max();

					count = maxNumber;
				}
			}

			string empCode = $"{deptCode}{(count + 1):D3}";

			// 確保生成的編號不重複
			while (await _context.Emps.AnyAsync(e => e.EmpCode == empCode))
			{
				count++;
				empCode = $"{deptCode}{(count + 1):D3}";
			}

			return empCode;
		}

		// GET: Edit
		[Permission("emp")]
		public async Task<IActionResult> Edit(string id)
		{
			if (string.IsNullOrEmpty(id))
				return NotFound();

			var emp = await _context.Emps
	.Include(e => e.RoleNoNavigation)   // ✅ 正確，載入 Role 導覽屬性
	.Include(e => e.DeptNoNavigation)   // ✅ 正確，載入 Dept 導覽屬性
	.FirstOrDefaultAsync(e => e.No == id);


			if (emp == null)
				return NotFound();

			var viewModel = new EditEmpViewModel
			{
				No = emp.No,
				Name = emp.Name,
				RoleNo = emp.RoleNo,
				DeptNo = emp.DeptNo,
				HireDate = emp.HireDate,
				Email = emp.Email,
				Phone = emp.Phone,
				IdNumber = emp.IdNumber,
				BirthDate = (DateTime)emp.BirthDate,
				Status = emp.Status
			};

			LoadDropdowns(viewModel);

			System.Diagnostics.Debug.WriteLine($"Edit GET - 載入員工: {emp.Name}, DeptNo: {emp.DeptNo}, RoleNo: {emp.RoleNo}");

			return View(viewModel);
		}

		// POST: Edit
		[HttpPost]
		[Permission("emp")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, EditEmpViewModel viewModel)
		{
			System.Diagnostics.Debug.WriteLine($"=== Edit POST 開始 ===");
			System.Diagnostics.Debug.WriteLine($"URL ID: {id}, ViewModel No: {viewModel.No}");
			System.Diagnostics.Debug.WriteLine($"接收到的資料:");
			System.Diagnostics.Debug.WriteLine($"  Name: '{viewModel.Name}'");
			System.Diagnostics.Debug.WriteLine($"  DeptNo: {viewModel.DeptNo}");
			System.Diagnostics.Debug.WriteLine($"  RoleNo: {viewModel.RoleNo}");
			System.Diagnostics.Debug.WriteLine($"  Email: '{viewModel.Email}'");
			System.Diagnostics.Debug.WriteLine($"  Phone: '{viewModel.Phone}'");
			System.Diagnostics.Debug.WriteLine($"  IdNumber: '{viewModel.IdNumber}'");
			System.Diagnostics.Debug.WriteLine($"  Status: {viewModel.Status}");

			if (id != viewModel.No)
			{
				System.Diagnostics.Debug.WriteLine("ID 不符，返回 NotFound");
				return NotFound();
			}

			// 手動檢查必要欄位
			if (string.IsNullOrWhiteSpace(viewModel.Name))
			{
				ModelState.AddModelError("Name", "員工姓名為必填");
			}

			if (viewModel.DeptNo <= 0)
			{
				ModelState.AddModelError("DeptNo", "請選擇部門");
				System.Diagnostics.Debug.WriteLine($"DeptNo 無效: {viewModel.DeptNo}");
			}

			if (viewModel.RoleNo <= 0)
			{
				ModelState.AddModelError("RoleNo", "請選擇職位");
				System.Diagnostics.Debug.WriteLine($"RoleNo 無效: {viewModel.RoleNo}");
			}

			// 檢查部門和職位是否存在
			if (viewModel.DeptNo > 0)
			{
				var deptExists = await _context.Depts.AnyAsync(d => d.No == viewModel.DeptNo);
				if (!deptExists)
				{
					ModelState.AddModelError("DeptNo", "選擇的部門不存在");
					System.Diagnostics.Debug.WriteLine($"部門 {viewModel.DeptNo} 不存在");
				}
			}

			if (viewModel.RoleNo > 0)
			{
				var roleExists = await _context.Roles.AnyAsync(r => r.No == viewModel.RoleNo);
				if (!roleExists)
				{
					ModelState.AddModelError("RoleNo", "選擇的職位不存在");
					System.Diagnostics.Debug.WriteLine($"職位 {viewModel.RoleNo} 不存在");
				}
			}

			// 檢查 ModelState
			System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
			if (!ModelState.IsValid)
			{
				System.Diagnostics.Debug.WriteLine("ModelState 驗證失敗，錯誤如下:");
				foreach (var error in ModelState)
				{
					if (error.Value.Errors.Any())
					{
						System.Diagnostics.Debug.WriteLine($"  欄位 '{error.Key}': {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
					}
				}

				// 重新載入下拉選單
				LoadDropdowns(viewModel);
				return View(viewModel);
			}

			try
			{
				var existingEmp = await _context.Emps.FindAsync(id);
				if (existingEmp == null)
				{
					System.Diagnostics.Debug.WriteLine($"找不到員工 ID: {id}");
					return NotFound();
				}

				System.Diagnostics.Debug.WriteLine("開始更新員工資料...");

				// 更新欄位
				existingEmp.Name = viewModel.Name?.Trim();
				existingEmp.RoleNo = viewModel.RoleNo;
				existingEmp.DeptNo = viewModel.DeptNo;
				existingEmp.HireDate = viewModel.HireDate;
				existingEmp.Status = viewModel.Status;
				existingEmp.Email = viewModel.Email?.Trim();
				existingEmp.Phone = viewModel.Phone?.Trim();
				existingEmp.IdNumber = viewModel.IdNumber?.Trim().ToUpper();
				existingEmp.BirthDate = viewModel.BirthDate;

				await _context.SaveChangesAsync();

				System.Diagnostics.Debug.WriteLine("員工資料更新成功，跳轉到 Index");
				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateConcurrencyException ex)
			{
				System.Diagnostics.Debug.WriteLine($"並發更新錯誤: {ex.Message}");
				if (!EmpExists(viewModel.No))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"更新員工時發生錯誤: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
				ModelState.AddModelError("", $"更新員工時發生錯誤：{ex.Message}");
			}

			// 發生錯誤時重新載入下拉選單
			LoadDropdowns(viewModel);
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
			// 部門下拉選單 - 使用 No 作為 Value，DeptDescription 作為顯示文字
			viewModel.Depts = new SelectList(_context.Depts, "No", "DeptDescription", viewModel.DeptNo);

			// 職位下拉選單 - 使用 No 作為 Value，RoleName 作為 Text（供 RoleHelper 轉換用）
			viewModel.Roles = new SelectList(_context.Roles, "No", "RoleName", viewModel.RoleNo);

			// Debug 輸出，檢查資料是否正確載入
			var deptCount = _context.Depts.Count();
			var roleCount = _context.Roles.Count();
			System.Diagnostics.Debug.WriteLine($"載入了 {deptCount} 個部門，{roleCount} 個職位");
		}

		// 建立一個私有方法來載入編輯頁面的下拉式選單資料
		private void LoadDropdowns(EditEmpViewModel viewModel)
		{
			try
			{
				// 部門下拉選單
				var deptList = _context.Depts
					.Select(d => new SelectListItem
					{
						Value = d.No.ToString(),
						Text = d.DeptDescription,
						Selected = d.No == viewModel.DeptNo
					})
					.ToList();

				// 新增預設選項（可選）
				deptList.Insert(0, new SelectListItem
				{
					Value = "",
					Text = "請選擇部門",
					Selected = viewModel.DeptNo == 0
				});

				viewModel.Depts = deptList;

				// 職位下拉選單
				var roleList = _context.Roles
					.Select(r => new SelectListItem
					{
						Value = r.No.ToString(),
						Text = RoleHelper.GetRoleDisplayName(r.RoleName),
						Selected = r.No == viewModel.RoleNo
					})
					.ToList();

				// 新增預設選項（可選）
				roleList.Insert(0, new SelectListItem
				{
					Value = "",
					Text = "請選擇職位",
					Selected = viewModel.RoleNo == 0
				});

				viewModel.Roles = roleList;

				// Debug 輸出
				System.Diagnostics.Debug.WriteLine($"EditEmp LoadDropdowns - DeptNo: {viewModel.DeptNo}, RoleNo: {viewModel.RoleNo}");
				System.Diagnostics.Debug.WriteLine($"部門選項數量: {deptList.Count}, 職位選項數量: {roleList.Count}");

				// 檢查是否有選中的項目
				var selectedDept = deptList.FirstOrDefault(d => d.Selected);
				var selectedRole = roleList.FirstOrDefault(r => r.Selected);
				System.Diagnostics.Debug.WriteLine($"選中的部門: {selectedDept?.Text}, 選中的職位: {selectedRole?.Text}");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"LoadDropdowns 錯誤: {ex.Message}");
				// 提供空的選單以避免錯誤
				viewModel.Depts = new List<SelectListItem>();
				viewModel.Roles = new List<SelectListItem>();
			}
		}

		private bool EmpExists(string id)
		{
			return _context.Emps.Any(e => e.No == id);
		}
	}
}