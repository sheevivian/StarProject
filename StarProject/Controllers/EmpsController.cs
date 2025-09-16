using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using StarProject.Models;
using StarProject.ViewModels;
using StarProject.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarProject.Controllers
{
	public class EmpsController : Controller
	{
		private readonly StarProjectContext _context;
		private readonly IEmailService _emailService;

		public EmpsController(StarProjectContext context, IEmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}

		// 查看員工清單 - 需要員工管理或用戶管理權限
		[Permission("emp", "user")]
		// GET: Emps
		public async Task<IActionResult> Index()
		{
			var emps = await _context.Emps
				.Include(e => e.DeptNoNavigation)
				.Include(e => e.RoleNoNavigation)
				.ToListAsync();
			return View(emps);
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
			ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "DeptName");
			ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "RoleName");

			var viewModel = new CreateEmpViewModel
			{
				HireDate = DateTime.Today
			};

			return View(viewModel);
		}
		[Permission("emp")]
		// POST: Emps/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreateEmpViewModel viewModel)
		{
			// 檢查ModelState
			if (!ModelState.IsValid)
			{
				ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "DeptName", viewModel.DeptNo);
				ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "RoleName", viewModel.RoleNo);
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
						await _emailService.SendAsync(
							emp.Email,
							"歡迎加入阿波羅天文館 - 帳號資訊通知",
							$@"
						<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
						<div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>
							<h2 style='color: #333; margin: 0 0 10px 0;'>歡迎加入阿波羅天文館</h2>
						</div>

						<div style='background-color: white; padding: 20px; border: 1px solid #dee2e6; border-radius: 8px;'>
							<p style='margin: 0 0 15px 0; font-size: 16px;'>親愛的 <strong>{emp.Name}</strong> 您好：</p>
    
							<p style='margin: 0 0 15px 0; line-height: 1.6;'>
								恭喜您正式成為阿波羅天文館的一員！以下是您的帳號相關資訊，請妥善保管：
							</p>
    
							<div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
								<p style='margin: 0 0 10px 0;'><strong>員工編號：</strong>{emp.EmpCode}</p>
								<p style='margin: 0 0 10px 0;'><strong>預設密碼：</strong>{defaultPassword}</p>
								<p style='margin: 0;'><strong>到職日期：</strong>{emp.HireDate:yyyy年MM月dd日}</p>
							</div>
    
							<div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0;'>
								<h4 style='margin: 0 0 10px 0; color: #856404;'>⚠️ 重要提醒</h4>
								<ul style='margin: 0; padding-left: 20px; line-height: 1.6; color: #856404;'>
									<li>請於首次登入後立即修改密碼</li>
									<li>密碼應包含大小寫字母、數字，長度至少8位</li>
									<li>請勿與他人分享您的帳號密碼</li>
								</ul>
							</div>
    
							<p style='margin: 20px 0 15px 0; line-height: 1.6;'>
								如有任何問題，請隨時與人事部門聯繫。再次歡迎您的加入，期待與您一同探索星空的奧秘！
							</p>
    
							<div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6;'>
								<p style='margin: 0; color: #6c757d; font-size: 14px;'>
									此信件為系統自動發送，請勿直接回覆。<br>
									© 2025 阿波羅天文館人事部門
								</p>
							</div>
						</div>
					</div>",
							isHtml: true
						);

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

			ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "DeptName", viewModel.DeptNo);
			ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "RoleName", viewModel.RoleNo);
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

			ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "DeptName", emp.DeptNo);
			ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "RoleName", emp.RoleNo);
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

				// 重新載入下拉選單資料
				ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "DeptName", viewModel.DeptNo);
				ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "RoleName", viewModel.RoleNo);
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
			ViewData["DeptNo"] = new SelectList(_context.Depts, "No", "DeptName", viewModel.DeptNo);
			ViewData["RoleNo"] = new SelectList(_context.Roles, "No", "RoleName", viewModel.RoleNo);
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
		// POST: Emps/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(string id)
		{
			var emp = await _context.Emps.FindAsync(id);
			if (emp != null)
			{
				_context.Emps.Remove(emp);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		private bool EmpExists(string id)
		{
			return _context.Emps.Any(e => e.No == id);
		}
	}
}