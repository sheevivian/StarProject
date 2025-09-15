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
			// 除錯資訊
			System.Diagnostics.Debug.WriteLine($"收到的資料: Name={viewModel.Name}, DeptNo={viewModel.DeptNo}, RoleNo={viewModel.RoleNo}, HireDate={viewModel.HireDate}");

			// 檢查ModelState
			if (!ModelState.IsValid)
			{
				System.Diagnostics.Debug.WriteLine("ModelState驗證失敗:");
				foreach (var error in ModelState)
				{
					System.Diagnostics.Debug.WriteLine($"欄位: {error.Key}, 錯誤: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
				}

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
						System.Diagnostics.Debug.WriteLine($"準備發送Email到: {emp.Email}");

						// 改為使用 IEmailService 介面已存在的 SendAsync 方法，並自行組合郵件內容：
						await _emailService.SendAsync(
								emp.Email,
								"🎉 歡迎加入公司",
								$@"<h2>親愛的 {emp.Name}，歡迎加入！</h2>
									<p>您的員工編號是：<b>{emp.EmpCode}</b></p>
									<p>預設密碼為：<b>{defaultPassword}</b></p>
									<p>請盡快登入系統並修改密碼。</p>
									<p>— 人資部</p>",
								isHtml: true
							);

						TempData["EmailSent"] = true;
						System.Diagnostics.Debug.WriteLine("Email發送成功");
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"Email發送失敗: {ex.Message}");
						System.Diagnostics.Debug.WriteLine($"詳細錯誤: {ex}");
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
				System.Diagnostics.Debug.WriteLine($"建立員工錯誤: {ex.Message}");
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